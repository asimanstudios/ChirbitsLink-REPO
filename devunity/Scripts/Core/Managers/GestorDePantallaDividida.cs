using UnityEngine;
using Unity.Netcode;
using ChibiCocina.Models;

namespace ChibiCocina.Nucleo
{
    public class GestorDePantallaDividida : MonoBehaviour
    {
        public static GestorDePantallaDividida Instancia { get; private set; }
        
        [Header("Configuración de Pantalla Dividida")]
        public Camera[] camaras;
        public RectTransform[] areasJugador;
        public bool pantallaDivididaActiva;
        public ModoPantallaDividida modoActual;
        
        // Estado de la configuración
        private int jugadoresActivos;
        private ConfiguracionPantalla[] configuraciones;
        
        // Eventos
        public System.Action<ModoPantallaDividida> OnModoCambiado;
        public System.Action<int> OnJugadoresActualizados;
        
        private void Awake()
        {
            if (Instancia == null)
            {
                Instancia = this;
                DontDestroyOnLoad(gameObject);
                InicializarPantallaDividida();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InicializarPantallaDividida()
        {
            jugadoresActivos = 1;
            modoActual = ModoPantallaDividida.UnJugador;
            pantallaDivididaActiva = false;
            
            InicializarConfiguraciones();
            AplicarConfiguracion(modoActual);
            
            Debug.Log("[GestorDePantallaDividida] Inicializado");
        }
        
        private void InicializarConfiguraciones()
        {
            configuraciones = new ConfiguracionPantalla[4];
            
            // Configuración para 1 jugador
            configuraciones[0] = new ConfiguracionPantalla
            {
                modo = ModoPantallaDividida.UnJugador,
                rectangulos = new Rect[] { new Rect(0, 0, 1, 1) },
                camarasActivas = new int[] { 0 }
            };
            
            // Configuración para 2 jugadores (horizontal)
            configuraciones[1] = new ConfiguracionPantalla
            {
                modo = ModoPantallaDividida.DosJugadoresHorizontal,
                rectangulos = new Rect[] { 
                    new Rect(0, 0, 0.5f, 1), 
                    new Rect(0.5f, 0, 0.5f, 1) 
                },
                camarasActivas = new int[] { 0, 1 }
            };
            
            // Configuración para 3 jugadores
            configuraciones[2] = new ConfiguracionPantalla
            {
                modo = ModoPantallaDividida.TresJugadores,
                rectangulos = new Rect[] { 
                    new Rect(0, 0.5f, 0.5f, 0.5f), 
                    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
                    new Rect(0.25f, 0, 0.5f, 0.5f)
                },
                camarasActivas = new int[] { 0, 1, 2 }
            };
            
            // Configuración para 4 jugadores
            configuraciones[3] = new ConfiguracionPantalla
            {
                modo = ModoPantallaDividida.CuatroJugadores,
                rectangulos = new Rect[] { 
                    new Rect(0, 0.5f, 0.5f, 0.5f), 
                    new Rect(0.5f, 0.5f, 0.5f, 0.5f),
                    new Rect(0, 0, 0.5f, 0.5f),
                    new Rect(0.5f, 0, 0.5f, 0.5f)
                },
                camarasActivas = new int[] { 0, 1, 2, 3 }
            };
        }
        
        public void ActualizarJugadores(int cantidadJugadores)
        {
            if (cantidadJugadores < 1 || cantidadJugadores > 4) return;
            
            jugadoresActivos = cantidadJugadores;
            ModoPantallaDividida nuevoModo = ObtenerModoParaJugadores(cantidadJugadores);
            
            if (nuevoModo != modoActual)
            {
                CambiarModo(nuevoModo);
            }
            
            OnJugadoresActualizados?.Invoke(jugadoresActivos);
            Debug.Log($"[GestorDePantallaDividida] Actualizado a {cantidadJugadores} jugadores");
        }
        
        private ModoPantallaDividida ObtenerModoParaJugadores(int jugadores)
        {
            return jugadores switch
            {
                1 => ModoPantallaDividida.UnJugador,
                2 => ModoPantallaDividida.DosJugadoresHorizontal,
                3 => ModoPantallaDividida.TresJugadores,
                4 => ModoPantallaDividida.CuatroJugadores,
                _ => ModoPantallaDividida.UnJugador
            };
        }
        
        private void CambiarModo(ModoPantallaDividida nuevoModo)
        {
            modoActual = nuevoModo;
            AplicarConfiguracion(nuevoModo);
            OnModoCambiado?.Invoke(nuevoModo);
            
            Debug.Log($"[GestorDePantallaDividida] Modo cambiado a: {nuevoModo}");
        }
        
        private void AplicarConfiguracion(ModoPantallaDividida modo)
        {
            ConfiguracionPantalla config = ObtenerConfiguracion(modo);
            if (config == null) return;
            
            // Desactivar todas las cámaras primero
            DesactivarTodasLasCamaras();
            
            // Activar cámaras necesarias y configurar sus viewports
            for (int i = 0; i < config.camarasActivas.Length && i < config.rectangulos.Length; i++)
            {
                int indiceCamara = config.camarasActivas[i];
                Rect viewport = config.rectangulos[i];
                
                if (indiceCamara < camaras.Length && camaras[indiceCamara] != null)
                {
                    camaras[indiceCamara].rect = viewport;
                    camaras[indiceCamara].enabled = true;
                }
            }
            
            pantallaDivididaActiva = modo != ModoPantallaDividida.UnJugador;
        }
        
        private void DesactivarTodasLasCamaras()
        {
            if (camaras == null) return;
            
            for (int i = 0; i < camaras.Length; i++)
            {
                if (camaras[i] != null)
                {
                    camaras[i].enabled = false;
                }
            }
        }
        
        private ConfiguracionPantalla ObtenerConfiguracion(ModoPantallaDividida modo)
        {
            foreach (var config in configuraciones)
            {
                if (config.modo == modo)
                    return config;
            }
            return null;
        }
        
        public void ActivarPantallaDividida(bool activar)
        {
            if (activar && jugadoresActivos > 1)
            {
                CambiarModo(ObtenerModoParaJugadores(jugadoresActivos));
            }
            else if (!activar)
            {
                CambiarModo(ModoPantallaDividida.UnJugador);
            }
        }
        
        public void ConfigurarCamara(int indiceJugador, Camera camara)
        {
            if (indiceJugador >= 0 && indiceJugador < camaras.Length)
            {
                camaras[indiceJugador] = camara;
                Debug.Log($"[GestorDePantallaDividida] Cámara configurada para jugador {indiceJugador}");
            }
        }
        
        public Camera ObtenerCamaraJugador(int indiceJugador)
        {
            if (indiceJugador >= 0 && indiceJugador < camaras.Length)
            {
                return camaras[indiceJugador];
            }
            return null;
        }
        
        public ModoPantallaDividida ObtenerModoActual()
        {
            return modoActual;
        }
        
        public int ObtenerJugadoresActivos()
        {
            return jugadoresActivos;
        }
        
        public bool EstaPantallaDivididaActiva()
        {
            return pantallaDivididaActiva;
        }
        
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
            GUILayout.Label($"Pantalla Dividida: {(pantallaDivididaActiva ? "Activa" : "Inactiva")}");
            GUILayout.Label($"Modo: {modoActual}");
            GUILayout.Label($"Jugadores: {jugadoresActivos}");
            
            if (GUILayout.Button("Cambiar Modo"))
            {
                int siguienteModo = ((int)modoActual + 1) % 4;
                ActualizarJugadores(siguienteModo + 1);
            }
            
            GUILayout.EndArea();
        }
    }
    
    public enum ModoPantallaDividida
    {
        UnJugador,
        DosJugadoresHorizontal,
        DosJugadoresVertical,
        TresJugadores,
        CuatroJugadores
    }
    
    [System.Serializable]
    public class ConfiguracionPantalla
    {
        public ModoPantallaDividida modo;
        public Rect[] rectangulos;
        public int[] camarasActivas;
    }
}
