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

    public bool IsConnected => 
        (_webSocket?.State == WebSocketState.Open) || 
        (_bluetoothDevice?.State == DeviceState.Connected) ||
        (_tcpClient?.Connected ?? false);

    public long Latency { get; private set; }
    public event Action<long>? LatencyUpdated;
    public event Action<string>? MessageReceived;

    public void SetBluetoothDevice(Plugin.BLE.Abstractions.Contracts.IDevice device)
    {
        _bluetoothDevice = device;
    }

    // TCP Connection (App as Client)
    public async Task ConnectTcpAsync(string? host = null, int? port = null)
    {
        try
        {
            string finalHost = host ?? Microsoft.Maui.Storage.Preferences.Get("pref_server_ip", "127.0.0.1");
            int finalPort = port ?? Microsoft.Maui.Storage.Preferences.Get("pref_server_port", 11000);

            _tcpClient = new System.Net.Sockets.TcpClient();
            _cts = new CancellationTokenSource();
            await _tcpClient.ConnectAsync(finalHost, finalPort);
            _tcpStream = _tcpClient.GetStream();
            _ = ReceiveTcpLoop(); // Start receiving messages in background
            System.Diagnostics.Debug.WriteLine($"Connected to Game Server at {finalHost}:{finalPort}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"TCP Connection Error: {ex.Message}");
            throw;
        }
    }

    // WebSocket Connection
    public async Task ConnectWebSocketAsync(string url)
    {
        _webSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
        _ = ReceiveWebSocketLoop(); // Start receiving messages in background
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
                foreach (var service in services)
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    var characteristic = characteristics.FirstOrDefault(c => c.CanWrite);
                    if (characteristic != null)
                    {
                        await characteristic.WriteAsync(bytes);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Bluetooth Send Error: {ex.Message}");
            }
        }
    }

    public async Task DisconnectAsync()
    {
        _cts?.Cancel();

        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected by user", CancellationToken.None);
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

        _cts?.Dispose();
    }

    private async Task ReceiveTcpLoop()
    {
        var buffer = new byte[4096];
        while (_tcpClient != null && _tcpClient.Connected && _tcpStream != null)
        {
            try
            {
                int bytesRead = await _tcpStream.ReadAsync(buffer, 0, buffer.Length, _cts?.Token ?? CancellationToken.None);
                if (bytesRead == 0) break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                ProcessReceivedMessage(message);
            }
            catch
            {
                break;
            }
        }
    }

    private async Task ReceiveWebSocketLoop()
    {
        var buffer = new byte[4096];
        while (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            try 
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts?.Token ?? CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
                
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                ProcessReceivedMessage(message);
            }
            catch 
            {
                break;
            }
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
}