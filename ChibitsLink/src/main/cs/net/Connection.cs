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
    private Plugin.BLE.Abstractions.Contracts.IDevice? _bluetoothDevice;
    private CancellationTokenSource? _cts;
    private System.Diagnostics.Stopwatch _pingStopwatch = new();

    public bool IsConnected => (_webSocket?.State == WebSocketState.Open) || (_bluetoothDevice?.State == DeviceState.Connected);
    public long Latency { get; private set; }
    public event Action<long>? LatencyUpdated;

    public void SetBluetoothDevice(Plugin.BLE.Abstractions.Contracts.IDevice device)
    {
        _bluetoothDevice = device;
    }

    // WebSocket Connection
    public async Task ConnectWebSocketAsync(string url)
    {
        _webSocket = new ClientWebSocket();
        _cts = new CancellationTokenSource();
        await _webSocket.ConnectAsync(new Uri(url), _cts.Token);
        _ = ReceiveLoop(); // Start receiving messages in background
    }

    public async Task SendMessageAsync(string message)
    {
        _pingStopwatch.Restart();

        // Send via WebSocket
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, _cts?.Token ?? CancellationToken.None);
        }

        // Send via Bluetooth
        if (_bluetoothDevice != null && _bluetoothDevice.State == Plugin.BLE.Abstractions.DeviceState.Connected)
        {
            try
            {
                var services = await _bluetoothDevice.GetServicesAsync();
                // Look for the first writable characteristic (Simplified for Unity)
                foreach (var service in services)
                {
                    var characteristics = await service.GetCharacteristicsAsync();
                    var characteristic = characteristics.FirstOrDefault(c => c.CanWrite);
                    if (characteristic != null)
                    {
                        var bytes = Encoding.UTF8.GetBytes(message);
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
        if (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnected by user", _cts?.Token ?? CancellationToken.None);
            _webSocket.Dispose();
            _webSocket = null;
        }
        _cts?.Cancel();
    }

    private async Task ReceiveLoop()
    {
        var buffer = new byte[4096];
        while (_webSocket != null && _webSocket.State == WebSocketState.Open)
        {
            try 
            {
                var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cts?.Token ?? CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) break;
                
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                
                _pingStopwatch.Stop();
                Latency = _pingStopwatch.ElapsedMilliseconds;
                LatencyUpdated?.Invoke(Latency);
            }
            catch 
            {
                break;
            }
        }
    }
}