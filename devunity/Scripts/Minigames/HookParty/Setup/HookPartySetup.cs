using UnityEngine;

namespace ChibitsLink.GameSide.HookParty
{
    /// <summary>
    /// Actúa como el Bootstrap o Inyector local de la escena del minijuego.
    /// Garantiza configuraciones fijas antes de empezar.
    /// </summary>
    public class HookPartySetup : MonoBehaviour
    {
        [Header("Configuraciones de Escena")]
        [Tooltip("Cámara que muestra el cuadrado cerrado del juego")]
        public Camera areaCamera;

        private void Start()
        {
            if (areaCamera == null)
            {
                areaCamera = Camera.main;
            }

            if (areaCamera != null)
            {
                // Si preferimos estilo 2D perfecto, podemos configurarlo aquí:
                // areaCamera.orthographic = true;
            }
            
            // El PlayerManager se encargará en su evento OnSceneLoaded de 
            // leer lo que hay en spawnPoints e instanciar los presonajes base.
            
            Debug.Log("[HookParty] Base y terreno configurados. Esperando a PlayerManager.");
        }
    }
}
