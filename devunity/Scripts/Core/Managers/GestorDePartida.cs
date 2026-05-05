using UnityEngine;
using Unity.Netcode;
using ChibiCocina.Models;

namespace ChibiCocina.Nucleo
{
    public class GestorDePartida : MonoBehaviour
    {
        public static GestorDePartida Instancia { get; private set; }
        
        [Header("Configuración de Partida")]
        public int jugadoresMaximos = 4;
        public float tiempoPreparacion = 5f;
        public float tiempoPartida = 300f;
        
        // Estado de la partida
        private EstadoPartida estadoActual;
        private float tiempoRestante;
        private int jugadoresConectados;
        
        // Eventos
        public System.Action<EstadoPartida> OnEstadoCambiado;
        public System.Action<float> OnTiempoActualizado;
        public System.Action<int> OnJugadoresActualizados;
        
        private void Awake()
        {
            if (Instancia == null)
            {
                Instancia = this;
                DontDestroyOnLoad(gameObject);
                InicializarPartida();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InicializarPartida()
        {
            estadoActual = EstadoPartida.Espera;
            tiempoRestante = tiempoPreparacion;
            jugadoresConectados = 0;
            
            Debug.Log("[GestorDePartida] Inicializado en estado de espera");
        }
        
        private void Update()
        {
            if (estadoActual == EstadoPartida.Preparacion || estadoActual == EstadoPartida.Jugando)
            {
                ActualizarTiempo();
            }
        }
        
        private void ActualizarTiempo()
        {
            tiempoRestante -= Time.deltaTime;
            OnTiempoActualizado?.Invoke(tiempoRestante);
            
            bool tiempoAgotado = tiempoRestante <= 0f;
            if (tiempoAgotado)
            {
                CambiarEstado(estadoActual == EstadoPartida.Preparacion ? EstadoPartida.Jugando : EstadoPartida.Terminada);
            }
        }
        
        public void IniciarPartida()
        {
            if (estadoActual != EstadoPartida.Espera) return;
            
            CambiarEstado(EstadoPartida.Preparacion);
            tiempoRestante = tiempoPreparacion;
            
            Debug.Log("[GestorDePartida] Iniciando preparación de partida");
        }
        
        public void FinalizarPartida()
        {
            CambiarEstado(EstadoPartida.Terminada);
            Debug.Log("[GestorDePartida] Partida finalizada");
        }
        
        private void CambiarEstado(EstadoPartida nuevoEstado)
        {
            EstadoPartida estadoAnterior = estadoActual;
            estadoActual = nuevoEstado;
            
            switch (estadoActual)
            {
                case EstadoPartida.Preparacion:
                    tiempoRestante = tiempoPreparacion;
                    break;
                case EstadoPartida.Jugando:
                    tiempoRestante = tiempoPartida;
                    break;
                case EstadoPartida.Terminada:
                    ProcesarResultados();
                    break;
            }
            
            OnEstadoCambiado?.Invoke(estadoActual);
            Debug.Log($"[GestorDePartida] Estado cambiado: {estadoAnterior} -> {estadoActual}");
        }
        
        private void ProcesarResultados()
        {
            // Lógica para procesar resultados de la partida
            Debug.Log("[GestorDePartida] Procesando resultados finales");
        }
        
        public void JugadorConectado()
        {
            jugadoresConectados++;
            OnJugadoresActualizados?.Invoke(jugadoresConectados);
            
            Debug.Log($"[GestorDePartida] Jugador conectado. Total: {jugadoresConectados}/{jugadoresMaximos}");
        }
        
        public void JugadorDesconectado()
        {
            jugadoresConectados = Mathf.Max(0, jugadoresConectados - 1);
            OnJugadoresActualizados?.Invoke(jugadoresConectados);
            
            Debug.Log($"[GestorDePartida] Jugador desconectado. Total: {jugadoresConectados}/{jugadoresMaximos}");
        }
        
        public EstadoPartida ObtenerEstadoActual()
        {
            return estadoActual;
        }
        
        public float ObtenerTiempoRestante()
        {
            return tiempoRestante;
        }
        
        public int ObtenerJugadoresConectados()
        {
            return jugadoresConectados;
        }
        
        public bool PuedeIniciarPartida()
        {
            return estadoActual == EstadoPartida.Espera && jugadoresConectados >= 2;
        }
    }
    
    public enum EstadoPartida
    {
        Espera,
        Preparacion,
        Jugando,
        Terminada
    }
}
