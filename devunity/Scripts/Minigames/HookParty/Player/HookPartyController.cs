using UnityEngine;

namespace ChibitsLink.GameSide.HookParty
{
    /// <summary>
    /// Escucha los eventos que llegan desde el PlayerManager (MAUI -> TCP -> PlayerManager)
    /// y los expone al resto del Player (HookSystem, Visualizer) a través de propiedades.
    /// </summary>
    public class HookPartyController : MonoBehaviour, PlayerManager.IChibitsController
    {
        [Tooltip("Dirección actual desde el MAUI virtual joystick")]
        public Vector2 AimDirection { get; private set; }
        
        [Tooltip("True si el botón ha sido pulsado. Se consume al leerlo.")]
        public bool TriggerHook { get; private set; }

        /// <summary>
        /// Recibe el vector de movimiento (X, Y) del joystick.
        /// </summary>
        public void ProcessJoystick(float x, float y)
        {
            AimDirection = new Vector2(x, y);
        }

        /// <summary>
        /// Recibe los eventos de botón pulsado/soltado.
        /// </summary>
        public void ProcessButton(string buttonId, string state)
        {
            bool hasValidButtonPayload = !string.IsNullOrEmpty(buttonId) && !string.IsNullOrEmpty(state);
            if (hasValidButtonPayload)
            {
                string s = state.ToLower();
                bool isDown = (s == "down" || s == "pressed" || s == "true" || s == "1");

                if (isDown)
                {
                    TriggerHook = true;
                }
            }
        }

        public bool ConsumeHookTrigger()
        {
            if (TriggerHook)
            {
                TriggerHook = false;
                return true;
            }
            return false;
        }
    }
}
