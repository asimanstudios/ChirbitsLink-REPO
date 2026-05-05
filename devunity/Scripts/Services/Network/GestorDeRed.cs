using Unity.Netcode;
using UnityEngine;

namespace ChibiCocina.Nucleo
{
    public class GestorDeRed : MonoBehaviour
    {
        public static GestorDeRed Instancia;

        private void Awake()
        {
            if (Instancia == null) Instancia = this;
            else Destroy(gameObject);
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                if (GUILayout.Button("Iniciar como Host (Anfitrión)")) NetworkManager.Singleton.StartHost();
                if (GUILayout.Button("Iniciar como Cliente")) NetworkManager.Singleton.StartClient();
                if (GUILayout.Button("Iniciar como Servidor")) NetworkManager.Singleton.StartServer();
            }
            else
            {
                string modo = NetworkManager.Singleton.IsHost ? "Host" : NetworkManager.Singleton.IsServer ? "Servidor" : "Cliente";
                GUILayout.Label("Modo: " + modo);
                if (GUILayout.Button("Desconectar")) NetworkManager.Singleton.Shutdown();
            }
            GUILayout.EndArea();
        }
    }
}
