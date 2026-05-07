using UnityEngine;
using Unity.Netcode;
using ChibiCocina.Models;
using System.Collections.Generic;

namespace ChibitsLink.Services.Gameplay
{
    public class OrderManager : MonoBehaviour
    {
        public static OrderManager Instance { get; private set; }
        
        [Header("Order Configuration")]
        public GameObject[] ingredientPrefabs;
        public Transform[] deliveryPoints;
        public float timeBetweenOrders = 30f;
        public int maxActiveOrders = 5;
        
        // Order state
        private List<Pedido> _activeOrders;
        private List<Pedido> _completedOrders;
        private float _nextOrderTime;
        private int _currentOrderId;
        
        // Events
        public System.Action<Pedido> OnOrderCreated;
        public System.Action<Pedido> OnOrderCompleted;
        public System.Action<Pedido> OnOrderCancelled;
        public System.Action<int> OnOrdersUpdated;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeOrders();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeOrders()
        {
            _activeOrders = new List<Pedido>();
            _completedOrders = new List<Pedido>();
            _nextOrderTime = timeBetweenOrders;
            _currentOrderId = 1;
            
            Debug.Log("[GestorDePedidos] Inicializado");
        }
        
        private void Update()
        {
            ActualizarTiempoPedidos();
            VerificarPedidosExpirados();
        }
        
        private void ActualizarTiempoPedidos()
        {
            tiempoSiguientePedido -= Time.deltaTime;
            
            bool debeCrearPedido = tiempoSiguientePedido <= 0f && pedidosActivos.Count < maximoPedidosActivos;
            if (debeCrearPedido)
            {
                CrearNuevoPedido();
                tiempoSiguientePedido = tiempoEntrePedidos;
            }
        }
        
        private void VerificarPedidosExpirados()
        {
            for (int i = pedidosActivos.Count - 1; i >= 0; i--)
            {
                Pedido pedido = pedidosActivos[i];
                if (pedido.TiempoRestante <= 0f)
                {
                    CancelarPedido(pedido);
                }
            }
        }
        
        public Pedido CrearNuevoPedido()
        {
            if (pedidosActivos.Count >= maximoPedidosActivos)
            {
                Debug.LogWarning("[GestorDePedidos] Máximo de pedidos activos alcanzado");
                return null;
            }
            
            Pedido nuevoPedido = GenerarPedidoAleatorio();
            pedidosActivos.Add(nuevoPedido);
            
            OnPedidoCreado?.Invoke(nuevoPedido);
            OnPedidosActualizados?.Invoke(pedidosActivos.Count);
            
            Debug.Log($"[GestorDePedidos] Nuevo pedido creado: {nuevoPedido.Id}");
            return nuevoPedido;
        }
        
        private Pedido GenerarPedidoAleatorio()
        {
            var nuevoPedido = new Pedido
            {
                Id = idPedidoActual++,
                IngredientesRequeridos = GenerarIngredientesAleatorios(),
                TiempoLimite = Random.Range(60f, 180f),
                Estado = EstadoPedido.Activo,
                Recompensa = Random.Range(10, 50),
                ClienteId = Random.Range(1, 100)
            };
            
            nuevoPedido.TiempoRestante = nuevoPedido.TiempoLimite;
            return nuevoPedido;
        }
        
        private List<Ingrediente> GenerarIngredientesAleatorios()
        {
            var ingredientes = new List<Ingrediente>();
            int cantidadIngredientes = Random.Range(2, 5);
            
            for (int i = 0; i < cantidadIngredientes && i < prefabIngredientes.Length; i++)
            {
                int indiceAleatorio = Random.Range(0, prefabIngredientes.Length);
                var ingrediente = new Ingrediente
                {
                    Tipo = (TipoIngrediente)indiceAleatorio,
                    Cantidad = 1,
                    Prefab = prefabIngredientes[indiceAleatorio],
                    Procesado = false
                };
                
                ingredientes.Add(ingrediente);
            }
            
            return ingredientes;
        }
        
        public bool EntregarPedido(int pedidoId, List<Ingrediente> ingredientesEntregados)
        {
            Pedido pedido = BuscarPedidoActivo(pedidoId);
            if (pedido == null)
            {
                Debug.LogWarning($"[GestorDePedidos] Pedido {pedidoId} no encontrado");
                return false;
            }
            
            bool esEntregaCorrecta = VerificarIngredientes(pedido.IngredientesRequeridos, ingredientesEntregados);
            if (esEntregaCorrecta)
            {
                CompletarPedido(pedido);
                return true;
            }
            
            Debug.LogWarning($"[GestorDePedidos] Entrega incorrecta para pedido {pedidoId}");
            return false;
        }
        
        private bool VerificarIngredientes(List<Ingrediente> requeridos, List<Ingrediente> entregados)
        {
            if (requeridos.Count != entregados.Count) return false;
            
            for (int i = 0; i < requeridos.Count; i++)
            {
                var requerido = requeridos[i];
                var entregado = entregados[i];
                
                bool coincideTipo = requerido.Tipo == entregado.Tipo;
                bool coincideCantidad = requerido.Cantidad == entregado.Cantidad;
                bool estaProcesado = requerido.Procesado == entregado.Procesado;
                
                if (!coincideTipo || !coincideCantidad || !estaProcesado)
                    return false;
            }
            
            return true;
        }
        
        private void CompletarPedido(Pedido pedido)
        {
            pedido.Estado = EstadoPedido.Completado;
            pedido.TiempoCompletado = Time.time;
            
            pedidosActivos.Remove(pedido);
            pedidosCompletados.Add(pedido);
            
            OnPedidoCompletado?.Invoke(pedido);
            OnPedidosActualizados?.Invoke(pedidosActivos.Count);
            
            Debug.Log($"[GestorDePedidos] Pedido {pedido.Id} completado. Recompensa: {pedido.Recompensa}");
        }
        
        private void CancelarPedido(Pedido pedido)
        {
            pedido.Estado = EstadoPedido.Cancelado;
            pedido.TiempoCancelado = Time.time;
            
            pedidosActivos.Remove(pedido);
            pedidosCompletados.Add(pedido);
            
            OnPedidoCancelado?.Invoke(pedido);
            OnPedidosActualizados?.Invoke(pedidosActivos.Count);
            
            Debug.Log($"[GestorDePedidos] Pedido {pedido.Id} cancelado por tiempo");
        }
        
        private Pedido BuscarPedidoActivo(int pedidoId)
        {
            return pedidosActivos.Find(p => p.Id == pedidoId);
        }
        
        public List<Pedido> ObtenerPedidosActivos()
        {
            return new List<Pedido>(pedidosActivos);
        }
        
        public List<Pedido> ObtenerPedidosCompletados()
        {
            return new List<Pedido>(pedidosCompletados);
        }
        
        public Pedido ObtenerPedido(int pedidoId)
        {
            Pedido pedido = BuscarPedidoActivo(pedidoId);
            if (pedido == null)
            {
                pedido = pedidosCompletados.Find(p => p.Id == pedidoId);
            }
            return pedido;
        }
        
        public int ObtenerPedidosActivosCount()
        {
            return pedidosActivos.Count;
        }
        
        public int ObtenerPedidosCompletadosCount()
        {
            return pedidosCompletados.Count;
        }
        
        public void LimpiarPedidosCompletados()
        {
            pedidosCompletados.Clear();
            Debug.Log("[GestorDePedidos] Pedidos completados limpiados");
        }
        
        public void ReiniciarSistema()
        {
            pedidosActivos.Clear();
            pedidosCompletados.Clear();
            tiempoSiguientePedido = tiempoEntrePedidos;
            idPedidoActual = 1;
            
            Debug.Log("[GestorDePedidos] Sistema reiniciado");
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 480, 300, 200));
            GUILayout.Label($"Pedidos Activos: {pedidosActivos.Count}/{maximoPedidosActivos}");
            GUILayout.Label($"Próximo pedido: {tiempoSiguientePedido:F1}s");
            GUILayout.Label($"Total completados: {pedidosCompletados.Count}");
            
            if (GUILayout.Button("Crear Pedido Manual"))
            {
                CrearNuevoPedido();
            }
            
            if (GUILayout.Button("Limpiar Completados"))
            {
                LimpiarPedidosCompletados();
            }
            
            if (GUILayout.Button("Reiniciar Sistema"))
            {
                ReiniciarSistema();
            }
            
            GUILayout.EndArea();
        }
    }
    
    [System.Serializable]
    public class Pedido
    {
        public int Id;
        public List<Ingrediente> IngredientesRequeridos;
        public float TiempoLimite;
        public float TiempoRestante;
        public EstadoPedido Estado;
        public int Recompensa;
        public int ClienteId;
        public float TiempoCreacion;
        public float TiempoCompletado;
        public float TiempoCancelado;
    }
    
    [System.Serializable]
    public class Ingrediente
    {
        public TipoIngrediente Tipo;
        public int Cantidad;
        public GameObject Prefab;
        public bool Procesado;
    }
    
    public enum EstadoPedido
    {
        Activo,
        Completado,
        Cancelado
    }
    
    public enum TipoIngrediente
    {
        Tomate,
        Lechuga,
        Carne,
        Pan,
        Queso,
        Cebolla,
        Pepino,
        Huevo
    }
}
