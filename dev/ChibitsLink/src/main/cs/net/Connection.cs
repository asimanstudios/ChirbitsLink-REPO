namespace ChibitsLink.main.cs.net;

using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

public class Connection
{
    private ClientWebSocket? _webSocket;
    private System.Net.Sockets.TcpClient? _tcpClient;
    private System.Net.Sockets.NetworkStream? _tcpStream;
    private Plugin.BLE.Abstractions.Contracts.IDevice? _bluetoothDevice;
    private CancellationTokenSource? _cts;
    private System.Diagnostics.Stopwatch _pingStopwatch = new();
    private System.Timers.Timer? _heartbeatTimer;
    private bool _isConnecting;

    public bool IsConnected => 
        (_webSocket?.State == WebSocketState.Open) || 
        (_bluetoothDevice?.State == DeviceState.Connected) ||
        (_tcpClient?.Connected ?? false);

    public long Latency { get; private set; }
    public event Action<long>? LatencyUpdated;
    public event Action<string>? MessageReceived;
    public event Action? Disconnected;

    public void SetBluetoothDevice(Plugin.BLE.Abstractions.Contracts.IDevice device)
    {
        _bluetoothDevice = device;
    }

    // TCP Connection (App as Client)
    public async Task ConnectTcpAsync(string? host = null, int? port = null)
    {
        if (!_isConnecting && !(_tcpClient?.Connected ?? false))
        {
            _isConnecting = true;

            string finalHost = host ?? Microsoft.Maui.Storage.Preferences.Get("pref_server_ip", "127.0.0.1");
            int finalPort = port ?? Microsoft.Maui.Storage.Preferences.Get("pref_server_port", 11000);

            try
            {
                _tcpClient = new System.Net.Sockets.TcpClient();
                _cts = new CancellationTokenSource();
                await _tcpClient.ConnectAsync(finalHost, finalPort);
                _tcpStream = _tcpClient.GetStream();
                _ = ReceiveTcpLoop(); // Start receiving messages in background
                StartHeartbeat();
                System.Diagnostics.Debug.WriteLine($"Connected to Game Server at {finalHost}:{finalPort}");
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine($"TCP Socket Error: {ex.Message}");
                throw new ChibitsLink.main.cs.exception.NetworkException($"Error al conectar con la sala ({finalHost}:{finalPort}). Revisa tu conexión Wi-Fi.", ex);
            }
            catch (OperationCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine($"TCP Connection Cancelled: {ex.Message}");
                throw new ChibitsLink.main.cs.exception.NetworkException($"Conexión cancelada.", ex);
            }
            catch (ObjectDisposedException ex)
            {
                System.Diagnostics.Debug.WriteLine($"TCP Connection Object Disposed: {ex.Message}");
                throw new ChibitsLink.main.cs.exception.NetworkException($"Error inesperado al conectar ({finalHost}:{finalPort}).", ex);
            }
            catch (InvalidOperationException ex)
            {
                System.Diagnostics.Debug.WriteLine($"TCP Connection Invalid Operation: {ex.Message}");
                throw new ChibitsLink.main.cs.exception.NetworkException($"Error inesperado al conectar ({finalHost}:{finalPort}).", ex);
            }
            finally
            {
                _isConnecting = false;
            }
        }
    }

    // WebSocket Connection
    public async Task ConnectWebSocketAsync(string url)
    {
        _webSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
        _ = ReceiveWebSocketLoop(); // Start receiving messages in background
        StartHeartbeat();
    }

    public async Task SendMessageAsync(string message)
    {
        _pingStopwatch.Restart();
        var bytes = Encoding.UTF8.GetBytes(message);

        // Send via TCP
        if (_tcpClient != null && _tcpClient.Connected && _tcpStream != null)
        {
            await _tcpStream.WriteAsync(bytes, 0, bytes.Length, _cts?.Token ?? CancellationToken.None);
        }

        // Send via WebSocket
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
        }

        // Send via Bluetooth
        if (_bluetoothDevice != null && _bluetoothDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
        {
            try
            {
                var services = await _bluetoothDevice.GetServicesAsync();
                int serviceIndex = 0;
                bool characteristicWritten = false;
                while (serviceIndex < services.Count && !characteristicWritten)
                {
                    var service = services[serviceIndex];
                    var characteristics = await service.GetCharacteristicsAsync();
                    var characteristic = characteristics.FirstOrDefault(c => c.CanWrite);
                    if (characteristic != null)
                    {
                        await characteristic.WriteAsync(bytes);
                        characteristicWritten = true;
                    }
                    serviceIndex++;
                }
            }
            catch (System.IO.IOException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth Send IO Error: {ex.Message}");
            }
            catch (Plugin.BLE.Abstractions.Exceptions.BluetoothException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth Send Error: {ex.Message}");
            }
        }
    }

    public async Task DisconnectAsync()
    {
        if (_cts != null)
        {
            try { _cts.Cancel(); } catch { }
        }

        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            try { await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected by user", CancellationToken.None); } catch { }
            _webSocket.Dispose();
            _webSocket = null;
        }

        if (_tcpClient != null)
        {
            _tcpStream?.Close();
            _tcpClient.Close();
            _tcpClient.Dispose();
            _tcpClient = null;
        }

        if (_cts != null)
        {
            try { _cts.Dispose(); } catch { }
            _cts = null;
        }
    }

    private async Task ReceiveTcpLoop()
    {
        var buffer = new byte[4096];
        var sb = new StringBuilder();
        var client = (System.Net.Sockets.TcpClient?)null;
        var stream = (System.Net.Sockets.NetworkStream?)null;
        try
        {
            bool keepReading = true;
            while (keepReading)
            {
                client = _tcpClient;
                stream = _tcpStream;
                if (client != null && client.Connected && stream != null)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, _cts?.Token ?? CancellationToken.None);
                    if (bytesRead > 0)
                    {
                        var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        sb.Append(chunk);

                        string data = sb.ToString();
                        int newlineIndex;
                        while ((newlineIndex = data.IndexOf('\n')) >= 0)
                        {
                            string msg = data.Substring(0, newlineIndex).Trim('\r');
                            data = data.Substring(newlineIndex + 1);
                            if (!string.IsNullOrWhiteSpace(msg))
                            {
                                ProcessReceivedMessage(msg);
                            }
                        }
                        sb.Clear();
                        sb.Append(data);
                    }
                    else
                    {
                        keepReading = false;
                    }
                }
                else
                {
                    keepReading = false;
                }
            }
        }
        catch (System.IO.IOException ex)
        {
            Debug.WriteLine($"TCP Receive IO Exception: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("TCP Receive: loop cancelled (normal disconnect).");
        }
        catch (ObjectDisposedException ex)
        {
            Debug.WriteLine($"TCP Receive Object Disposed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"TCP Receive Invalid Operation: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"TCP Receive Argument Exception: {ex.Message}");
        }
        
        StopHeartbeat();
        
        // Evitar lanzar el evento si la desconexión fue intencionada (ej. llamada a DisconnectAsync)
        bool wasCancelled = false;
        try 
        {
            wasCancelled = _cts?.IsCancellationRequested ?? false;
        }
        catch (ObjectDisposedException) 
        {
            wasCancelled = true; // Si está disposed, es que se llamó a DisconnectAsync
        }

        if (_tcpClient != null && !wasCancelled)
        {
            Disconnected?.Invoke();
        }
    }

    private async Task ReceiveWebSocketLoop()
    {
        var buffer = new byte[4096];
        try
        {
            bool keepReading = true;
            while (keepReading)
            {
                var ws = _webSocket;
                if (ws != null && ws.State == WebSocketState.Open)
                {
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts?.Token ?? CancellationToken.None);
                    if (result.MessageType != WebSocketMessageType.Close)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessReceivedMessage(message);
                    }
                    else
                    {
                        keepReading = false;
                    }
                }
                else
                {
                    keepReading = false;
                }
            }
        }
        catch (System.Net.WebSockets.WebSocketException ex)
        {
            Debug.WriteLine($"WebSocket Exception: {ex.Message}");
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("WebSocket Receive: loop cancelled (normal disconnect).");
        }
        catch (ObjectDisposedException ex)
        {
            Debug.WriteLine($"WebSocket Receive Object Disposed: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Debug.WriteLine($"WebSocket Receive Invalid Operation: {ex.Message}");
        }
        catch (ArgumentException ex)
        {
            Debug.WriteLine($"WebSocket Receive Argument Exception: {ex.Message}");
        }

        bool wasCancelled = false;
        try 
        {
            wasCancelled = _cts?.IsCancellationRequested ?? false;
        }
        catch (ObjectDisposedException) 
        {
            wasCancelled = true; // Si está disposed, es que se llamó a DisconnectAsync
        }

        if (_webSocket != null && _webSocket.State != WebSocketState.Closed && !wasCancelled)
        {
            Disconnected?.Invoke();
        }
    }

    private void ProcessReceivedMessage(string message)
    {
        _pingStopwatch.Stop();
        Latency = _pingStopwatch.ElapsedMilliseconds;
        LatencyUpdated?.Invoke(Latency);
        MessageReceived?.Invoke(message);
        System.Diagnostics.Debug.WriteLine($"Message Received: {message}");
    }

    private void StartHeartbeat()
    {
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = new System.Timers.Timer(5000); // 5 seconds
        _heartbeatTimer.Elapsed += async (s, e) => 
        {
            if (IsConnected)
            {
                try 
                {
                    // Send a lightweight ping to keep connection alive and detect failures
                    await SendMessageAsync("PING");
                }
                catch 
                {
                    StopHeartbeat();
                    Disconnected?.Invoke();
                }
            }
            else
            {
                StopHeartbeat();
                Disconnected?.Invoke();
            }
        };
        _heartbeatTimer.AutoReset = true;
        _heartbeatTimer.Enabled = true;
    }

    private void StopHeartbeat()
    {
        _heartbeatTimer?.Stop();
        _heartbeatTimer?.Dispose();
        _heartbeatTimer = null;
    }
}