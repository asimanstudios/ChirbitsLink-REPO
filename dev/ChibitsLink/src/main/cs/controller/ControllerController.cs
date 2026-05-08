namespace ChibitsLink.main.cs.controller;

/*
 * Controla el sistema de mando: delega las entradas del usuario (joystick, botones)
 * al servicio de comunicación con el servidor de Unity.
 */
using System.Threading.Tasks;
using ChibitsLink.main.cs.service;

public class ControllerController
{
    private readonly ControllerService _service;
    // Constructor
    public ControllerController(ControllerService service)
    {
        _service = service;
    }
    // Controlar el Joystick
    public async Task HandleJoystickMoved(float x, float y)
    {
        await _service.SendJoystickMove(x, y);
    }
    // Controlar Botones
    public async Task HandleButtonPressed(string buttonId)
    {
        await _service.SendButtonPress(buttonId);
    }
}