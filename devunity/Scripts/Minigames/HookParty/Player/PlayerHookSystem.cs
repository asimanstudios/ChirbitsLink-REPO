using UnityEngine;

namespace ChibitsLink.GameSide.HookParty
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HookPartyController))]
    public class PlayerHookSystem : MonoBehaviour
    {
        [Header("Hook Settings")]
        [Tooltip("Distancia máxima del raycast del gancho")]
        [SerializeField] private float hookMaxDistance = 40f;
        [Tooltip("Capas donde se puede anclar el gancho (ej: Paredes, Techo)")]
        [SerializeField] private LayerMask hookableLayer;
        [Tooltip("Fuerza ascendente al MANTENER el botón")]
        [SerializeField] private float retractForce = 35f; // Mayor fuerza de tracción
        [Tooltip("Velocidad a la que se acorta la cuerda en metros/seg al mantener pulsado")]
        [SerializeField] private float retractSpeed = 12f; // Trepa más rápido
        [Tooltip("Fuerza de impulso lateral para balancearse (Swing)")]
        [SerializeField] private float swingForce = 25f;
        
        [Header("Visuals (Orientación)")]
        [Tooltip("Valores para arreglar si el personaje da la espalda. Por ejemplo: Right = (0,0,0), Left = (0,180,0)")]
        [SerializeField] private Vector3 lookRightRotation = new Vector3(0, 0f, 0); 
        [SerializeField] private Vector3 lookLeftRotation = new Vector3(0, 180f, 0); 

        [Header("UX Reference (Set via SetupUX)")]
        private GameObject _feetPrefab;
        private GameObject _tipPrefab;
        private AudioClip _shootSound;
        private AudioClip _hitSound;
        private AudioClip _cutSound;
        private bool _useAnimations = true;
        private RuntimeAnimatorController _overrideController;

        private GameObject _feetInstance;
        private GameObject _tipInstance;

        private Animator _animator;
        private LineRenderer hookLineRenderer;
        private HookPartyController _controller;
        private Rigidbody _rb;
        private SpringJoint _hookJoint;
        
        private bool _isHooked;
        private Vector3 _anchorPoint;
        private bool _wasButtonDown;

        public void SetupUX(GameObject feet, GameObject tip, AudioClip shoot, AudioClip hit, AudioClip cut, bool useAnims, RuntimeAnimatorController controller)
        {
            _feetPrefab = feet;
            _tipPrefab = tip;
            _shootSound = shoot;
            _hitSound = hit;
            _cutSound = cut;
            _useAnimations = useAnims;
            _overrideController = controller;

            // Aplicar controller si viene uno específico para este modo
            if (_overrideController != null)
            {
                if (_animator == null) _animator = GetComponentInChildren<Animator>();
                if (_animator != null) _animator.runtimeAnimatorController = _overrideController;
            }

            // Instanciar la 'caja' de los pies de forma permanente para el minijuego
            if (_feetPrefab != null && _feetInstance == null)
            {
                _feetInstance = Instantiate(_feetPrefab, transform.position, transform.rotation, transform);
                _feetInstance.transform.localPosition = new Vector3(0, 0.4f, 0); // Elevado hasta la cintura (aprox)
                _feetInstance.transform.localScale = Vector3.one * 0.7f; // Encogido
                _feetInstance.tag = "Player"; // Asegurar que sea detectable como jugador
            }
        }

        private void Start()
        {
            _controller = GetComponent<HookPartyController>();
            _rb = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            
            if (hookableLayer.value == 0)
            {
                hookableLayer = ~0; 
            }

            hookLineRenderer = GetComponent<LineRenderer>();
            if (hookLineRenderer == null)
            {
                hookLineRenderer = gameObject.AddComponent<LineRenderer>();
                hookLineRenderer.startWidth = 0.15f;
                hookLineRenderer.endWidth = 0.08f;
                hookLineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                hookLineRenderer.startColor = Color.black; 
                hookLineRenderer.endColor = Color.black;
                hookLineRenderer.positionCount = 2;
                hookLineRenderer.numCapVertices = 5;
            }
            if (hookLineRenderer != null)
            {
                hookLineRenderer.enabled = false;
            }

            // Expandir zona de recolección para que sea mucho más fácil pillar monedas
            SphereCollider collectionZone = gameObject.AddComponent<SphereCollider>();
            collectionZone.isTrigger = true;
            collectionZone.radius = 2.5f;
        }

        private void FixedUpdate()
        {
            bool justPressed = _controller.ConsumeHookTrigger();

            if (justPressed)
            {
                if (_isHooked)
                {
                    ReleaseHook();
                }
                else
                {
                    TryShootHook();
                }
            }

            if (_isHooked)
            {
                ManageRopeLength();
                SwingWithJoystick();
            }

            // Animación: Si estamos en el aire (HookParty es siempre aire/vuelo), activamos la pose de saltar
            if (_useAnimations && _animator != null)
            {
                _animator.SetBool("saltar", true);
                
                // Si el joystick tiene movimiento lateral, podríamos activar andar/correr, 
                // pero al estar en el aire, mejor dejar solo el salto o una pose dinámica.
                _animator.SetBool("andar", false);
                _animator.SetBool("correr", false);
            }

            _rb.linearDamping = _isHooked ? 0.5f : 0.1f;
        }

        private void Update()
        {
            if (_isHooked && hookLineRenderer != null)
            {
                hookLineRenderer.SetPosition(0, transform.position);
                hookLineRenderer.SetPosition(1, _anchorPoint);
            }
        }

        private void TryShootHook()
        {
            Vector2 aimDir = _controller.AimDirection;
            if (aimDir.sqrMagnitude < 0.01f) aimDir = Vector2.up; 
            
            Vector3 direction3D = new Vector3(aimDir.x, aimDir.y, 0f).normalized;

            // Animación: Gesto de lanzar/interactuar
            if (_useAnimations && _animator != null) _animator.SetTrigger("interactua");

            // Audio: Disparo
            if (_shootSound != null) AudioSource.PlayClipAtPoint(_shootSound, transform.position, 1f);

            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction3D, hookMaxDistance, hookableLayer);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            bool anchorAssigned = false;
            foreach (var hit in hits)
            {
                // Solo permitimos enganchar a objetos con la etiqueta correcta que no sean triggers ni el propio jugador
                bool canAttachToHit = hit.collider.CompareTag("hookable")
                                      && !hit.collider.isTrigger
                                      && hit.collider.gameObject != gameObject
                                      && hit.collider.transform.root != transform
                                      && !anchorAssigned;
                if (canAttachToHit)
                {
                    _isHooked = true;
                    _anchorPoint = hit.point;
                    anchorAssigned = true;

                    // Audio: Impacto
                    if (_hitSound != null) AudioSource.PlayClipAtPoint(_hitSound, _anchorPoint, 1f);

                    // VFX: Punta del gancho
                    if (_tipPrefab != null)
                    {
                        _tipInstance = Instantiate(_tipPrefab, _anchorPoint, Quaternion.LookRotation(hit.normal));
                    }

                    _hookJoint = gameObject.AddComponent<SpringJoint>();
                    _hookJoint.autoConfigureConnectedAnchor = false;
                    _hookJoint.connectedAnchor = _anchorPoint;

                    float dist = Vector3.Distance(transform.position, _anchorPoint);
                    _hookJoint.maxDistance = dist * 0.9f;   
                    _hookJoint.minDistance = dist * 0.1f;

                    _hookJoint.spring = 15f; 
                    _hookJoint.damper = 5f; 
                    _hookJoint.massScale = 1.5f; 
                    
                    if (hookLineRenderer != null) hookLineRenderer.enabled = true;
                }
            }
        }

        private void ManageRopeLength()
        {
            if (_hookJoint != null)
            {
                Vector2 aimDir = _controller.AimDirection;

                if (aimDir.y > 0.3f)
                {
                    _hookJoint.maxDistance -= retractSpeed * Time.fixedDeltaTime;
                    _hookJoint.maxDistance = Mathf.Max(0.5f, _hookJoint.maxDistance); // Límite inferior más bajo
                    
                    // CORRECCIÓN CLAVE: El límite mínimo de la telaraña no puede bloquear al máximo
                    _hookJoint.minDistance = Mathf.Min(_hookJoint.minDistance, _hookJoint.maxDistance);

                    Vector3 dirToAnchor = (_hookJoint.connectedAnchor - transform.position).normalized;
                    _rb.AddForce(dirToAnchor * retractForce, ForceMode.Acceleration);
                }

                bool isExtendingRope = aimDir.y < -0.3f;
                if (isExtendingRope)
                {
                    _hookJoint.maxDistance += retractSpeed * Time.fixedDeltaTime;
                    _hookJoint.maxDistance = Mathf.Min(hookMaxDistance, _hookJoint.maxDistance);
                }
            }
        }

        private void SwingWithJoystick()
        {
            Vector2 aimDir = _controller.AimDirection;

            if (Mathf.Abs(aimDir.x) > 0.1f)
            {
                _rb.AddForce(Vector3.right * aimDir.x * swingForce, ForceMode.Acceleration);

                // Rotación Visual: Así mirará adonde se está columpiando
                if (aimDir.x > 0.1f) transform.localRotation = Quaternion.Euler(lookRightRotation);

                bool shouldLookLeft = aimDir.x < -0.1f;
                if (shouldLookLeft) transform.localRotation = Quaternion.Euler(lookLeftRotation);
            }
        }

        private void ReleaseHook()
        {
            _isHooked = false;

            // Audio: Corte
            if (_cutSound != null) AudioSource.PlayClipAtPoint(_cutSound, transform.position, 1f);

            // Limpieza VFX (Solo la punta del gancho, los pies son permanentes)
            if (_tipInstance != null) Destroy(_tipInstance);

            if (_hookJoint != null)
            {
                Destroy(_hookJoint);
            }
            if (hookLineRenderer != null)
            {
                hookLineRenderer.enabled = false;
            }
        }
    }
}
