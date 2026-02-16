namespace ChibitsLink.main.cs.service;

using System.Threading.Tasks;
using ChibitsLink.main.cs.net;
using System.Text.Json;

public class ControllerService
{
    private readonly Connection _connection;

    public ControllerService(Connection connection)
    {
        _connection = connection;
    }

    public async Task SendJoystickMove(float x, float y)
    {
        var data = new { type = "joystick", x = x, y = y };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }

    public async Task SendButtonPress(string buttonId)
    {
        var data = new { type = "button", id = buttonId, state = "pressed" };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }

    public async Task SendSensorData(string sensorType, float value)
    {
        var data = new { type = "sensor", sensor = sensorType, value = value };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }
}