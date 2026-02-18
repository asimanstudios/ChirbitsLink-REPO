namespace ChibitsLink.main.cs.controller;

/*
 * Controla el sistema de mando
 */
using ChibitsLink.main.cs.view;
using ChibitsLink.main.cs.service;
using System.Threading.Tasks;

public class ControllerController
{
    private readonly ControllerService _service;

    public ControllerController(ControllerService service)
    {
        _service = service;
    }

    public async Task HandleJoystickMoved(float x, float y)
    {
        await _service.SendJoystickMove(x, y);
    }

    public async Task HandleButtonPressed(string buttonId)
    {
        await _service.SendButtonPress(buttonId);
    }
}