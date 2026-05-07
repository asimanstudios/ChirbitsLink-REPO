using UnityEngine;
using ChibiCocina.Models;

namespace ChibitsLink.Controllers
{
    /// <summary>
    /// Controlador de acciones de combate del jugador.
    /// Maneja ataques, defensa y habilidades especiales durante minijuegos.
    /// Aplica estadísticas diferentes según el tipo de escena (combate vs lobby).
    /// </summary>
    /// <remarks>
    /// Detecta automáticamente el tipo de escena y aplica las estadísticas apropiadas.
    /// Las estadísticas de combate son más agresivas que las de lobby.
    /// </remarks>
    public class PlayerCombatController : MonoBehaviour
    {
        [Header("Combat Stats")]
        /// <summary>Velocidad de caminata en modo combate</summary>
        public float combatWalkSpeed = 6.5f;
        /// <summary>Velocidad de carrera en modo combate</summary>
        public float combatRunSpeed = 11f;
        /// <summary>Aceleración de movimiento en modo combate</summary>
        public float combatAcceleration = 35f;
        /// <summary>Control de movimiento en el aire durante combate</summary>
        public float combatAirControl = 1.0f;
        /// <summary>Velocidad de rotación en modo combate</summary>
        public float combatRotationSpeed = 25f;
        /// <summary>Fuerza de gravedad en modo combate</summary>
        public float combatGravity = -22f;
        /// <summary>Fuerza de salto en modo combate</summary>
        public float combatJumpForce = 8f;
        /// <summary>Multiplicador de caída en modo combate</summary>
        public float combatFallMultiplier = 1.5f;
        /// <summary>Multiplicador de salto bajo en modo combate</summary>
        public float combatLowJumpMultiplier = 1f;
        
        [Header("Lobby Stats")]
        /// <summary>Velocidad de caminata en modo lobby</summary>
        public float lobbyWalkSpeed = 4f;
        /// <summary>Velocidad de carrera en modo lobby</summary>
        public float lobbyRunSpeed = 7f;
        /// <summary>Aceleración de movimiento en modo lobby</summary>
        public float lobbyAcceleration = 20f;
        /// <summary>Control de movimiento en el aire durante lobby</summary>
        public float lobbyAirControl = 0.8f;
        /// <summary>Velocidad de rotación en modo lobby</summary>
        public float lobbyRotationSpeed = 15f;
        /// <summary>Fuerza de gravedad en modo lobby</summary>
        public float lobbyGravity = -18f;
        /// <summary>Fuerza de salto en modo lobby</summary>
        public float lobbyJumpForce = 6f;
        /// <summary>Multiplicador de caída en modo lobby</summary>
        public float lobbyFallMultiplier = 2f;
        /// <summary>Multiplicador de salto bajo en modo lobby</summary>
        public float lobbyLowJumpMultiplier = 1.5f;
        
        /// <summary>Modelo de movimiento para aplicar estadísticas</summary>
        private MovementModel _movementModel;
        /// <summary>Modelo de salto para aplicar estadísticas</summary>
        private JumpModel _jumpModel;
        
        /// <summary>
        /// Inicializa el controlador de combate.
        /// Prepara los componentes necesarios para la gestión de estadísticas.
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
        }
        
        /// <summary>
        /// Inicializa los componentes necesarios para el controlador.
        /// Los modelos serán inyectados externamente.
        /// </summary>
        private void InitializeComponents()
        {
            // Models will be injected
        }
        
        /// <summary>
        /// Configura los modelos de datos que controlarán las estadísticas.
        /// Debe llamarse después de inicializar los modelos del jugador.
        /// </summary>
        /// <param name="movementModel">Modelo que contiene datos de movimiento</param>
        /// <param name="jumpModel">Modelo que contiene datos de salto</param>
        public void InitializeModels(MovementModel movementModel, JumpModel jumpModel)
        {
            _movementModel = movementModel;
            _jumpModel = jumpModel;
        }
        
        /// <summary>
        /// Aplica estadísticas específicas según el tipo de escena actual.
        /// Detecta automáticamente si es escena de combate, lobby o por defecto.
        /// </summary>
        /// <remarks>
        /// Utiliza el nombre de la escena para determinar el tipo.
        /// Escenas de combate: contienen "Push" o "Smash".
        /// Escenas de lobby: contienen "menu" o "lobby".
        /// </remarks>
        public void ApplySceneSpecificStats()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            bool isCombatScene = sceneName.Contains("Push") || sceneName.Contains("Smash");
            bool isLobby = sceneName.Contains("menu") || sceneName.Contains("lobby");
            
            if (isCombatScene)
            {
                ApplyCombatStats();
            }
            else if (isLobby)
            {
                ApplyLobbyStats();
            }
            else
            {
                ApplyDefaultStats();
            }
        }
        
        /// <summary>
        /// Aplica las estadísticas configuradas para modo de combate.
        /// Establece valores más agresivos para jugabilidad competitiva.
        /// </summary>
        /// <remarks>
        /// Modifica tanto el modelo de movimiento como el de salto.
        /// Las estadísticas de combate priorizan velocidad y control.
        /// </remarks>
        private void ApplyCombatStats()
        {
            _movementModel.WalkSpeed = combatWalkSpeed;
            _movementModel.RunSpeed = combatRunSpeed;
            _movementModel.Acceleration = combatAcceleration;
            _movementModel.AirControl = combatAirControl;
            _movementModel.RotationSpeed = combatRotationSpeed;
            
            _jumpModel.Gravity = combatGravity;
            _jumpModel.JumpForce = combatJumpForce;
            _jumpModel.FallMultiplier = combatFallMultiplier;
            _jumpModel.LowJumpMultiplier = combatLowJumpMultiplier;
            
            Debug.Log("[PlayerCombatController] Applied combat stats");
        }
        
        /// <summary>
        /// Aplica las estadísticas configuradas para modo de lobby.
        /// Establece valores más relajados para navegación casual.
        /// </summary>
        /// <remarks>
        /// Modifica tanto el modelo de movimiento como el de salto.
        /// Las estadísticas de lobby priorizan comodidad sobre rendimiento.
        /// </remarks>
        private void ApplyLobbyStats()
        {
            _movementModel.WalkSpeed = lobbyWalkSpeed;
            _movementModel.RunSpeed = lobbyRunSpeed;
            _movementModel.Acceleration = lobbyAcceleration;
            _movementModel.AirControl = lobbyAirControl;
            _movementModel.RotationSpeed = lobbyRotationSpeed;
            
            _jumpModel.Gravity = lobbyGravity;
            _jumpModel.JumpForce = lobbyJumpForce;
            _jumpModel.FallMultiplier = lobbyFallMultiplier;
            _jumpModel.LowJumpMultiplier = lobbyLowJumpMultiplier;
            
            Debug.Log("[PlayerCombatController] Applied lobby stats");
        }
        
        /// <summary>
        /// Aplica las estadísticas por defecto configuradas en el inspector.
        /// Reservado para futuras personalizaciones de escenas específicas.
        /// </summary>
        /// <remarks>
        /// Actualmente no modifica nada ya que los valores por defecto
        /// se establecen directamente en el inspector de Unity.
        /// </remarks>
        private void ApplyDefaultStats()
        {
            // Default stats are already set in the inspector
            // This method exists for future customization
            Debug.Log("[PlayerCombatController] Applied default stats");
        }
    }
}
