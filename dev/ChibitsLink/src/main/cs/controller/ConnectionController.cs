namespace ChibitsLink.main.cs.controller;

using System.Threading.Tasks;
using ChibitsLink.main.cs.net;
using Plugin.BLE.Abstractions.Contracts;

/// <summary>
/// Delegado de conexión: envuelve la capa de red para operaciones de bajo nivel.
/// </summary>
public class ConexionController
{
    private readonly Connection _connection;

    public ConexionController(Connection connection)
    {
        _connection = connection;
    }

    public async Task ConnectViaWebSocket(string url)
    {
        await _connection.ConnectWebSocketAsync(url);
    }

    public void SetBluetoothDevice(IDevice device)
    {
        _connection.SetBluetoothDevice(device);
    }
}