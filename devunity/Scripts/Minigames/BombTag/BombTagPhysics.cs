using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ChibitsLink.Minigames.BombTag
{
    /// <summary>
    /// Sistema de física para el minijuego BombTag.
    /// Maneja colisiones, explosiones y efectos físicos de las bombas.
    /// Controla transferencias de bomba y efectos visuales.
    /// </summary>
    /// <remarks>
    /// Gestiona el ciclo de vida completo de las bombas.
    /// Implementa detección de proximidad para transferencias.
    /// </remarks>
    public class BombTagPhysics : MonoBehaviour
    {
        [Header("Physics Configuration")]
        /// <summary>Distancia máxima para transferencia de bomba</summary>
        public float transferDistance = 1.7f;
        /// <summary>Tiempo de espera entre transferencias</summary>
        public float transferCooldownTime = 1.2f;
        /// <summary>Tiempo de espera inicial después de spawn</summary>
        public float initialCooldownTime = 1.5f;
        /// <summary>Altura vertical de la bomba sobre el jugador</summary>
        public float verticalOffset = 2f;
        
        /// <summary>Instancia actual de la bomba en el juego</summary>
        private GameObject _bombInstance;
        /// <summary>Jugador que actualmente porta la bomba</summary>
        private GameObject _currentCarrier;
        /// <summary>Configuración del juego BombTag</summary>
        private BombaTag _config;
        /// <summary>Contador de tiempo para cooldown de transferencia</summary>
        private float _transferCooldown;
        /// <summary>Indica si la bomba está explotando actualmente</summary>
        private bool _isExploding = false;
        
        /// <summary>
        /// Inicializa el sistema de física con la configuración del juego.
        /// Establece valores iniciales para el funcionamiento.
        /// </summary>
        /// <param name="config">Configuración del juego BombTag</param>
        public void Initialize(BombaTag config)
        {
            _config = config;
            _transferCooldown = 0f;
            _isExploding = false;
        }
        
        /// <summary>
        /// Spawnea una nueva bomba para el jugador objetivo.
        /// Destruye cualquier bomba existente antes de crear la nueva.
        /// </summary>
        /// <param name="target">Jugador que recibirá la bomba</param>
        /// <remarks>
        /// Valida que tanto el target como la configuración sean válidos.
        /// Establece inmediatamente el portador después del spawn.
        /// </remarks>
        public void SpawnBomb(GameObject target)
        {
            if (target != null && _config != null && _config.bombPrefab != null)
            {
                // Destroy existing bomb
                if (_bombInstance != null)
                {
                    Destroy(_bombInstance);
                }
                
                _bombInstance = Instantiate(_config.bombPrefab);
                _bombInstance.SetActive(true);
                SetCarrier(target);
                
                Debug.Log($"[BombTagPhysics] Bomb spawned for {target.name}");
            }
            else
            {
                Debug.LogError("[BombTagPhysics] Cannot spawn bomb - missing target or configuration");
            }
        }
        
        /// <summary>
        /// Establece un nuevo portador para la bomba actual.
        /// Posiciona la bomba sobre el jugador y desactiva su física.
        /// </summary>
        /// <param name="newCarrier">Nuevo jugador que portará la bomba</param>
        /// <remarks>
        /// Parentea la bomba al jugador para seguimiento preciso.
        /// Desactiva colliders y física para evitar interferencias.
        /// </remarks>
        public void SetCarrier(GameObject newCarrier)
        {
            if (newCarrier != null && _bombInstance != null)
            {
                _currentCarrier = newCarrier;
                
                // Parent bomb to carrier
                _bombInstance.transform.SetParent(newCarrier.transform, false);
                
                // Position bomb above carrier
                float height = verticalOffset + 0.5f;
                _bombInstance.transform.localPosition = Vector3.up * height;
                _bombInstance.transform.localRotation = Quaternion.identity;
                
                // Disable physics on bomb
                DisableBombPhysics();
                
                // Set initial cooldown
                _transferCooldown = initialCooldownTime;
            }
        }
        
        /// <summary>
        /// Desactiva los componentes físicos de la bomba.
        /// Deshabilita colliders y Rigidbody para evitar conflictos.
        /// </summary>
        /// <remarks>
        /// Necesario cuando la bomba está parenteada a un jugador.
        /// Previene colisiones no deseadas durante el transporte.
        /// </remarks>
        private void DisableBombPhysics()
        {
            if (_bombInstance != null)
            {
                foreach (var collider in _bombInstance.GetComponentsInChildren<Collider>())
                {
                    collider.enabled = false;
                }
                
                Rigidbody rb = _bombInstance.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
            }
        }
        
        /// <summary>
        /// Actualiza el sistema de transferencia de bomba.
        /// Detecta jugadores cercanos y realiza transferencias si corresponde.
        /// </summary>
        /// <param name="alivePlayers">Lista de jugadores vivos</param>
        /// <returns>True si ocurrió una transferencia</returns>
        /// <remarks>
        /// Respeta el cooldown entre transferencias.
        /// Solo transfiere si hay jugadores dentro de la distancia.
        /// </remarks>
        public bool UpdateTransfer(List<GameObject> alivePlayers)
        {
            bool hasTransferred = false;
            
            if (!_isExploding && _currentCarrier != null && _bombInstance != null)
            {
                // Update cooldown
                if (_transferCooldown > 0)
                {
                    _transferCooldown -= Time.deltaTime;
                }
                else
                {
                    // Check for transfers
                    GameObject transferTarget = FindTransferTarget(alivePlayers);
                    if (transferTarget != null)
                    {
                        SetCarrier(transferTarget);
                        _transferCooldown = transferCooldownTime;
                        hasTransferred = true;
                    }
                }
            }
            
            return hasTransferred;
        }
        
        /// <summary>
        /// Busca un jugador cercano para transferir la bomba.
        /// </summary>
        /// <param name="alivePlayers">Lista de jugadores vivos</param>
        /// <returns>Jugador cercano o null si no hay nadie</returns>
        /// <remarks>
        /// Excluye al portador actual de la búsqueda.
        /// Usa distancia euclidiana para detección.
        /// </remarks>
        private GameObject FindTransferTarget(List<GameObject> alivePlayers)
        {
            GameObject result = null;
            if (_currentCarrier != null)
            {
                Vector3 carrierPosition = _currentCarrier.transform.position;
                float distance;
                
                foreach (GameObject player in alivePlayers)
                {
                    if (player != _currentCarrier && result == null)
                    {
                        distance = Vector3.Distance(carrierPosition, player.transform.position);
                        if (distance < transferDistance)
                        {
                            result = player;
                        }
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Actualiza los aspectos visuales de la bomba.
        /// Posiciona la bomba y actualiza efectos según tiempo restante.
        /// </summary>
        /// <param name="remainingTime">Tiempo restante antes de la explosión</param>
        /// <remarks>
        /// Actualiza posición, escala y efectos de sonido.
        /// La escala y sonido se intensifican cuando queda poco tiempo.
        /// </remarks>
        public void UpdateBombVisuals(float remainingTime)
        {
            if (_bombInstance != null && _currentCarrier != null)
            {
                // Update position to follow carrier
                float height = verticalOffset + 0.5f;
                _bombInstance.transform.position = _currentCarrier.transform.position + Vector3.up * height;
                _bombInstance.transform.rotation = Quaternion.identity;
                
                // Update scale based on remaining time
                UpdateBombScale(remainingTime);
                
                // Update sound effects
                UpdateBombSound(remainingTime);
            }
        }
        
        /// <summary>
        /// Actualiza la escala de la bomba según el tiempo restante.
        /// Crea un efecto de pulsación que se intensifica con el tiempo.
        /// </summary>
        /// <param name="remainingTime">Tiempo restante</param>
        /// <remarks>
        /// Usa mayor frecuencia y amplitud cuando queda poco tiempo.
        /// Apunta al transform "Model" o usa el raíz si no existe.
        /// </remarks>
        private void UpdateBombScale(float remainingTime)
        {
            if (_bombInstance != null)
            {
                float frequency = remainingTime <= 5f ? 15f : 5f;
                float amplitude = remainingTime <= 5f ? 0.15f : 0.05f;
                float scale = 0.5f + Mathf.Sin(Time.time * frequency) * amplitude;
                
                Transform modelTransform = _bombInstance.transform.Find("Model") ?? _bombInstance.transform;
                modelTransform.localScale = Vector3.one * scale;
            }
        }
        
        /// <summary>
        /// Actualiza los efectos de sonido de la bomba.
        /// Reproduce sonido de tick con pitch variable según tiempo.
        /// </summary>
        /// <param name="remainingTime">Tiempo restante</param>
        /// <remarks>
        /// Aumenta el pitch cuando queda poco tiempo para urgencia.
        /// Configura AudioSource si no existe uno.
        /// </remarks>
        private void UpdateBombSound(float remainingTime)
        {
            if (_bombInstance != null && _config != null)
            {
                if (remainingTime > 0 && _config.tickSFX != null)
                {
                    AudioSource audioSource = _bombInstance.GetComponent<AudioSource>() ?? _bombInstance.AddComponent<AudioSource>();
                    
                    if (!audioSource.isPlaying)
                    {
                        audioSource.clip = _config.tickSFX;
                        audioSource.loop = true;
                        audioSource.Play();
                    }
                    
                    audioSource.pitch = remainingTime <= 5f ? 1.5f : 1.0f;
                }
            }
        }
        
        /// <summary>
        /// Procesa la secuencia de explosión de la bomba.
        /// Spawnea efectos visuales, de sonido y elimina al portador.
        /// </summary>
        /// <returns>IEnumerator para la corutina</returns>
        /// <remarks>
        /// Espera antes de desactivar al jugador para efectos.
        /// Limpia la bomba y resetea el estado de explosión.
        /// </remarks>
        public IEnumerator ProcessExplosion()
        {
            if (!_isExploding && _currentCarrier != null)
            {
                _isExploding = true;
                GameObject victim = _currentCarrier;
                
                Debug.Log($"[BombTagPhysics] BOOM! Explosion for {victim.name}");
                
                // Spawn explosion effects
                SpawnExplosionEffects(victim);
                
                // Clean up bomb
                if (_bombInstance != null)
                {
                    Destroy(_bombInstance);
                    _bombInstance = null;
                }
                
                _currentCarrier = null;
                
                // Disable victim after delay
                yield return new WaitForSecondsRealtime(0.3f);
                if (victim != null)
                {
                    victim.SetActive(false);
                }
                
                _isExploding = false;
            }
        }
        
        /// <summary>
        /// Spawnea los efectos visuales y de sonido de la explosión.
        /// Crea efectos temporales que se destruyen automáticamente.
        /// </summary>
        /// <param name="victim">Jugador que explotó</param>
        /// <remarks>
        /// Usa PlayClipAtPoint para audio espacial.
        /// Los efectos visuales se destruyen después de 2 segundos.
        /// </remarks>
        private void SpawnExplosionEffects(GameObject victim)
        {
            if (_config != null && victim != null)
            {
                // Spawn visual effect
                if (_config.explosionVFX != null)
                {
                    GameObject explosion = Instantiate(_config.explosionVFX, victim.transform.position, Quaternion.identity);
                    Destroy(explosion, 2f);
                }
                
                // Play sound effect
                if (_config.explosionSFX != null)
                {
                    // Play through a temporary audio source for spatial audio
                    AudioSource.PlayClipAtPoint(_config.explosionSFX, victim.transform.position);
                }
            }
        }
        
        /// <summary>
        /// Destruye la bomba actual y limpia el estado.
        /// Utilizado para resetear entre rondas o al finalizar.
        /// </summary>
        /// <remarks>
        /// Limpia todos los estados relacionados con la bomba.
        /// Prepara el sistema para un nuevo spawn.
        /// </remarks>
        public void DestroyBomb()
        {
            if (_bombInstance != null)
            {
                Destroy(_bombInstance);
                _bombInstance = null;
            }
            
            _currentCarrier = null;
            _isExploding = false;
        }
        
        /// <summary>
        /// Obtiene el jugador que actualmente porta la bomba.
        /// </summary>
        /// <returns>Jugador portador o null</returns>
        public GameObject GetCurrentCarrier() => _currentCarrier;
        
        /// <summary>
        /// Verifica si la bomba está explotando actualmente.
        /// </summary>
        /// <returns>True si está en proceso de explosión</returns>
        public bool IsExploding() => _isExploding;
        
        /// <summary>
        /// Verifica si actualmente hay una bomba en juego.
        /// </summary>
        /// <returns>True si hay una instancia de bomba</returns>
        public bool HasBomb() => _bombInstance != null;
    }
}
