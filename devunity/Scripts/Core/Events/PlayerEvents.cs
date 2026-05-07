using UnityEngine;
using UnityEngine.Events;
using ChibiCocina.Models;

namespace ChibitsLink.Events
{
    /// <summary>
    /// Evento de Unity para movimiento del jugador.
    /// Proporciona el vector de movimiento como parámetro.
    /// </summary>
    [System.Serializable]
    public class PlayerMoveEvent : UnityEvent<Vector2> { }

    /// <summary>
    /// Evento de Unity para salto del jugador.
    /// Se dispara cuando el jugador realiza un salto.
    /// </summary>
    [System.Serializable]
    public class PlayerJumpEvent : UnityEvent { }

    /// <summary>
    /// Evento de Unity para interacción del jugador.
    /// Se dispara cuando el jugador intenta interactuar.
    /// </summary>
    [System.Serializable]
    public class PlayerInteractEvent : UnityEvent { }
}
