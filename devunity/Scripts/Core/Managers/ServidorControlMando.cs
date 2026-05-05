using UnityEngine;
using Unity.Netcode;
using ChibiCocina.Models;

namespace ChibiCocina.Nucleo
{
    public class ServidorControlMando : MonoBehaviour
    {
        public static ServidorControlMando Instancia { get; private set; }
        
        [Header("Configuración del Servidor")]
        public int puerto = 7777;
        public int maximoConexiones = 8;
        public float timeoutConexion = 10f;
        
        // Estado del servidor
        private bool servidorActivo;
        private int clientesConectados;
        private System.Collections.Generic.Dictionary<ulong, ClienteInfo> clientes;
        
        // Eventos
        public System.Action<int> OnClientesActualizados;
        public System.Action<ulong> OnClienteConectado;
        public System.Action<ulong> OnClienteDesconectado;
        public System.Action<string> OnMensajeRecibido;
        
        private void Awake()
        {
            if (Instancia == null)
            {
                Instancia = this;
                DontDestroyOnLoad(gameObject);
                InicializarServidor();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InicializarServidor()
        {
            clientes = new System.Collections.Generic.Dictionary<ulong, ClienteInfo>();
            clientesConectados = 0;
            servidorActivo = false;
            
            // Suscribir a eventos de NetworkManager
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += ManejarClienteConectado;
                NetworkManager.Singleton.OnClientDisconnectCallback += ManejarClienteDesconectado;
            }
            
            Debug.Log("[ServidorControlMando] Servidor inicializado");
        }
        
        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= ManejarClienteConectado;
                NetworkManager.Singleton.OnClientDisconnectCallback -= ManejarClienteDesconectado;
            }
        }
        
        public bool IniciarServidor()
        {
            if (servidorActivo)
            {
                Debug.LogWarning("[ServidorControlMando] El servidor ya está activo");
                return false;
            }
            
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[ServidorControlMando] NetworkManager no encontrado");
                return false;
            }
            
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.ConnectionData.Port = (ushort)puerto;
            
            if (NetworkManager.Singleton.StartServer())
            {
                servidorActivo = true;
                Debug.Log($"[ServidorControlMando] Servidor iniciado en puerto {puerto}");
                return true;
            }
            
            Debug.LogError("[ServidorControlMando] No se pudo iniciar el servidor");
            return false;
        }
        
        public void DetenerServidor()
        {
            if (!servidorActivo) return;
            
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            servidorActivo = false;
            clientes.Clear();
            clientesConectados = 0;
            
            Debug.Log("[ServidorControlMando] Servidor detenido");
        }
        
        private void ManejarClienteConectado(ulong clientId)
        {
            if (clientes.ContainsKey(clientId)) return;
            
            var nuevoCliente = new ClienteInfo
            {
                Id = clientId,
                Nombre = $"Jugador_{clientId}",
                Conectado = true,
                TiempoConexion = Time.time
            };
            
            clientes.Add(clientId, nuevoCliente);
            clientesConectados++;
            
            OnClienteConectado?.Invoke(clientId);
            OnClientesActualizados?.Invoke(clientesConectados);
            
            Debug.Log($"[ServidorControlMando] Cliente conectado: {clientId} ({nuevoCliente.Nombre})");
        }
        
        private void ManejarClienteDesconectado(ulong clientId)
        {
            if (!clientes.ContainsKey(clientId)) return;
            
            var cliente = clientes[clientId];
            cliente.Conectado = false;
            cliente.TiempoDesconexion = Time.time;
            
            clientes.Remove(clientId);
            clientesConectados--;
            
            OnClienteDesconectado?.Invoke(clientId);
            OnClientesActualizados?.Invoke(clientesConectados);
            
            Debug.Log($"[ServidorControlMando] Cliente desconectado: {clientId} ({cliente.Nombre})");
        }
        
        public void EnviarMensajeATodos(string mensaje)
        {
            if (!servidorActivo) return;
            
            // Enviar mensaje a todos los clientes conectados
            Debug.Log($"[ServidorControlMando] Enviando mensaje a todos: {mensaje}");
        }
        
        public void EnviarMensajeACliente(ulong clientId, string mensaje)
        {
            if (!servidorActivo || !clientes.ContainsKey(clientId)) return;
            
            // Enviar mensaje a cliente específico
            Debug.Log($"[ServidorControlMando] Enviando mensaje a {clientId}: {mensaje}");
        }
        
        public bool EstaServidorActivo()
        {
            return servidorActivo;
        }
        
        public int ObtenerClientesConectados()
        {
            return clientesConectados;
        }
        
        public ClienteInfo? ObtenerClienteInfo(ulong clientId)
        {
            return clientes.ContainsKey(clientId) ? clientes[clientId] : null;
        }
        
        public System.Collections.Generic.IEnumerable<ClienteInfo> ObtenerTodosLosClientes()
        {
            return clientes.Values;
        }
        
        private void Update()
        {
            if (servidorActivo)
            {
                ProcesarMensajesPendientes();
            }
        }
        
        private void ProcesarMensajesPendientes()
        {
            // Lógica para procesar mensajes recibidos de clientes
            // Esta es una implementación básica que se puede expandir
        }
        
        private void OnGUI()
        {
            if (!servidorActivo) return;
            
            GUILayout.BeginArea(new Rect(10, 320, 300, 150));
            GUILayout.Label($"Servidor Activo - Puerto: {puerto}");
            GUILayout.Label($"Clientes: {clientesConectados}/{maximoConexiones}");
            
            if (GUILayout.Button("Detener Servidor"))
            {
                DetenerServidor();
            }
            
            GUILayout.EndArea();
        }
    }
    
    [System.Serializable]
    public class ClienteInfo
    {
        public ulong Id;
        public string Nombre;
        public bool Conectado;
        public float TiempoConexion;
        public float TiempoDesconexion;
        public int Ping;
    }
}
