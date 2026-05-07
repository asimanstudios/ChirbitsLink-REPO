using UnityEngine;
using ChibiCocina.Models;

namespace ChibitsLink.Controllers
{
    /// <summary>
    /// Controlador de efectos de audio del jugador.
    /// Reproduce sonidos de pasos, salto, interacción y otros eventos del jugador.
    /// Gestiona temporalización de sonidos y variación para mayor realismo.
    /// </summary>
    /// <remarks>
    /// Crea automáticamente un AudioSource si no existe uno.
    /// Utiliza AudioService para gestión centralizada de sonidos.
    /// </remarks>
    public class PlayerAudioController : MonoBehaviour
    {
        [Header("Audio Configuration")]
        /// <summary>Clip de sonido para el salto del jugador</summary>
        public AudioClip jumpSound;
        /// <summary>Array de clips de sonido para los pasos (variación aleatoria)</summary>
        public AudioClip[] footstepSounds;
        /// <summary>Intervalo de tiempo entre pasos al caminar</summary>
        public float footstepInterval = 0.4f;
        /// <summary>Intervalo de tiempo entre pasos al correr</summary>
        public float runFootstepInterval = 0.3f;
        
        /// <summary>Componente AudioSource para reproducción de sonidos</summary>
        private AudioSource _audioSource;
        /// <summary>Servicio centralizado para gestión de audio</summary>
        private AudioService _audioService;
        /// <summary>Último tiempo en que se reprodujo un sonido de paso</summary>
        private float _lastFootstepTime;
        
        /// <summary>
        /// Inicializa el controlador de audio.
        /// Configura componentes y servicios necesarios para la reproducción de sonidos.
        /// </summary>
        private void Awake()
        {
            InitializeComponents();
            InitializeServices();
        }
        
        /// <summary>
        /// Inicializa los componentes de audio requeridos.
        /// Obtiene o crea el componente AudioSource según sea necesario.
        /// </summary>
        private void InitializeComponents()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        /// <summary>
        /// Inicializa los servicios de audio.
        /// Crea y configura el AudioService para gestión centralizada.
        /// </summary>
        private void InitializeServices()
        {
            _audioService = gameObject.AddComponent<AudioService>();
            _audioService.Initialize(_audioSource);
        }
        
        /// <summary>
        /// Actualiza la reproducción de sonidos de pasos basados en el movimiento del jugador.
        /// Controla la temporalización según velocidad y estado de carrera.
        /// </summary>
        /// <param name="isGrounded">Indica si el jugador está en el suelo</param>
        /// <param name="movementSpeed">Velocidad actual de movimiento</param>
        /// <param name="isRunning">Indica si el jugador está corriendo</param>
        /// <remarks>
        /// Solo reproduce sonidos si el jugador está en el suelo y moviéndose.
        /// Utiliza intervalos diferentes para caminar vs correr.
        /// </remarks>
        public void UpdateStepAudio(bool isGrounded, float movementSpeed, bool isRunning)
        {
            bool shouldPlayFootsteps = isGrounded && movementSpeed > 0.1f;
            if (!shouldPlayFootsteps) return;
            
            float currentInterval = isRunning ? runFootstepInterval : footstepInterval;
            bool canPlayFootstep = Time.time - _lastFootstepTime >= currentInterval;
            
            if (canPlayFootstep)
            {
                PlayRandomFootstep();
                _lastFootstepTime = Time.time;
            }
        }
        
        /// <summary>
        /// Reproduce el sonido de salto configurado.
        /// Debe llamarse cuando el jugador realiza un salto.
        /// </summary>
        /// <remarks>
        /// Utiliza AudioService para reproducción consistente.
        /// No reproduce sonido si jumpSound es null.
        /// </remarks>
        public void PlayJumpSound()
        {
            if (jumpSound != null)
            {
                _audioService.PlayJumpSound(jumpSound);
            }
        }
        
        /// <summary>
        /// Reproduce un sonido de paso aleatorio del array disponible.
        /// Proporciona variación para evitar sonidos repetitivos.
        /// </summary>
        /// <remarks>
        /// Selecciona aleatoriamente entre todos los clips disponibles.
        /// Usa PlayOneShot para permitir superposición de sonidos.
        /// </remarks>
        private void PlayRandomFootstep()
        {
            bool hasFootstepSounds = footstepSounds != null && footstepSounds.Length > 0;
            if (!hasFootstepSounds) return;
            
            AudioClip randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
            _audioSource.PlayOneShot(randomFootstep);
        }
    }
}
