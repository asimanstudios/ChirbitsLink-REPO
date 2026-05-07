using UnityEngine;

namespace ChibitsLink.Services.Gameplay
{
    /// <summary>
    /// Servicio centralizado para gestión de audio del jugador.
    /// Maneja reproducción de sonidos de pasos, salto y otros efectos.
    /// Proporciona control de temporalización y variación de audio.
    /// </summary>
    /// <remarks>
    /// Utiliza AudioSource para reproducción de clips de audio.
    /// Implementa variación aleatoria para pasos más realistas.
    /// </remarks>
    public class AudioService : MonoBehaviour
    {
        [Header("Step Audio")]
        /// <summary>Array de clips de sonido para los pasos</summary>
        public AudioClip[] stepSounds;
        /// <summary>Intervalo de tiempo entre pasos al caminar</summary>
        public float normalStepInterval = 0.4f;
        /// <summary>Intervalo de tiempo entre pasos al correr</summary>
        public float runStepInterval = 0.3f;

        /// <summary>Componente AudioSource para reproducción de sonidos</summary>
        private AudioSource audioSource;
        /// <summary>Contador de tiempo para controlar intervalo de pasos</summary>
        private float stepTimer;
        /// <summary>Indica si el jugador está corriendo actualmente</summary>
        private bool isRunning;

        /// <summary>
        /// Inicializa el servicio de audio con el AudioSource proporcionado.
        /// Configura las propiedades del AudioSource para uso óptimo.
        /// </summary>
        /// <param name="source">AudioSource a utilizar para reproducción</param>
        public void Initialize(AudioSource source)
    {
        audioSource = source;
        SetupAudioSource();
    }

        /// <summary>
        /// Configura las propiedades del AudioSource para uso en el juego.
        /// Establece configuración espacial y prioridad apropiadas.
        /// </summary>
        /// <remarks>
        /// Configura para audio 3D espacial con prioridad alta.
        /// Evita reproducción automática al iniciar.
        /// </remarks>
        private void SetupAudioSource()
    {
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.priority = 0;
            audioSource.volume = 1f;
        }
    }

        /// <summary>
        /// Actualiza la reproducción de sonidos de pasos según el estado del jugador.
        /// Controla la temporalización basada en velocidad y estado de carrera.
        /// </summary>
        /// <param name="grounded">Indica si el jugador está en el suelo</param>
        /// <param name="velocityMagnitude">Magnitud de la velocidad actual</param>
        /// <param name="running">Indica si el jugador está corriendo</param>
        /// <remarks>
        /// Solo reproduce pasos si está en el suelo y moviéndose.
        /// Usa intervalos diferentes para caminar vs correr.
        /// Resetea el contador cuando no se cumplen las condiciones.
        /// </remarks>
        public void UpdateStepAudio(bool grounded, float velocityMagnitude, bool running)
    {
        bool canPlaySteps = grounded && velocityMagnitude >= 0.1f;
        if (!canPlaySteps)
        {
            stepTimer = 0;
        }
        else
        {
            isRunning = running;
            float interval = running ? runStepInterval : normalStepInterval;
            stepTimer += Time.deltaTime;

            if (stepTimer >= interval)
            {
                stepTimer = 0;
                PlayRandomStep();
            }
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
        private void PlayRandomStep()
    {
        bool hasStepAudio = stepSounds != null && stepSounds.Length > 0 && audioSource != null;
        if (hasStepAudio)
        {
            int index = Random.Range(0, stepSounds.Length);
            audioSource.PlayOneShot(stepSounds[index]);
        }
    }

        /// <summary>
        /// Reproduce el sonido de salto especificado.
        /// </summary>
        /// <param name="jumpSound">Clip de audio de salto a reproducir</param>
        /// <remarks>
        /// Valida que tanto el clip como el AudioSource no sean null.
        /// Usa PlayOneShot para no interrumpir otros sonidos.
        /// </remarks>
        public void PlayJumpSound(AudioClip jumpSound)
    {
        if (jumpSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(jumpSound);
        }
    }
}

