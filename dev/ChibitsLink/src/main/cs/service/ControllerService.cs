using System.Text.Json;
using System.Threading.Tasks;
using ChibitsLink.main.cs.net;

namespace ChibitsLink.main.cs.service;

/// <summary>
/// Serializa y envía los estados del mando (joystick, botones, sensores) al servidor de juego
/// a través de la conexión TCP/WebSocket activa.
/// </summary>
public class ControllerService
{
    private readonly Connection _connection;
    private readonly AccountService _accountService;

    public ControllerService(Connection connection, AccountService accountService)
    {
        _connection = connection;
        _accountService = accountService;
    }

    /// <summary>
    /// Envía la posición del joystick al servidor. Los valores x e y están normalizados en el rango [-1, 1].
    /// </summary>
    public async Task SendJoystickMove(float x, float y)
    {
        var uid = _accountService.GetCurrentUser()?.Id ?? "anonymous";
        var data = new { type = "joystick", x = x, y = y, userId = uid };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }

    /// <summary>
    /// Envía el evento de pulsación de un botón del mando al servidor.
    /// </summary>
    public async Task SendButtonPress(string buttonId)
    {
        var uid = _accountService.GetCurrentUser()?.Id ?? "anonymous";
        var data = new { type = "button", id = buttonId, state = "pressed", userId = uid };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }

    /// <summary>
    /// Envía datos de un sensor del dispositivo (ej. acelerómetro) al servidor.
    /// </summary>
    public async Task SendSensorData(string sensorType, float value)
    {
        var data = new { type = "sensor", sensor = sensorType, value = value };
        await _connection.SendMessageAsync(JsonSerializer.Serialize(data));
    }
}