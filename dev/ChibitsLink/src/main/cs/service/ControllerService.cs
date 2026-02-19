namespace ChibitsLink.main.cs.service;

using System.Threading.Tasks;
using ChibitsLink.main.cs.net;
using System.Text.Json;

public class ControllerService
{
    private readonly Connection _connection;
    private readonly AccountService _accountService;

    public ControllerService(Connection connection, AccountService accountService)
    {
        _connection = connection;
        _accountService = accountService;
    }

    public async Task SendJoystickMove(float x, float y)
    {
        var uid = _accountService.GetCurrentUser()?.Id ?? "anonymous";
        var data = new { type = "joystick", x = x, y = y, userId = uid };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }

    public async Task SendButtonPress(string buttonId)
    {
        var uid = _accountService.GetCurrentUser()?.Id ?? "anonymous";
        var data = new { type = "button", id = buttonId, state = "pressed", userId = uid };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }

    public async Task SendSensorData(string sensorType, float value)
    {
        var data = new { type = "sensor", sensor = sensorType, value = value };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }
}