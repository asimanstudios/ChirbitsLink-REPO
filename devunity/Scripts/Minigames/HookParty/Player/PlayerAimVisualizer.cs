using UnityEngine;

namespace ChibitsLink.GameSide.HookParty
{
    /// <summary>
    /// Crea automáticamente un punto visual rojo que orbita al jugador, 
    /// moviéndose en la dirección del joystick para ayudar a apuntar el gancho.
    /// </summary>
    public class PlayerAimVisualizer : MonoBehaviour
    {
        private HookPartyController controller;
        private Transform aimRoot;
        private GameObject dot;

        [Header("Settings")]
        [Tooltip("Deadzone local para no vibrar con valores mínimos.")]
        [SerializeField] private float joystickDeadzone = 0.1f;
        [SerializeField] private float dotDistance = 2.0f; // Distancia del puntito respecto al jugador

        private void Start()
        {
            controller = GetComponent<HookPartyController>();

            // Auto-montar el sistema de apuntado (Pivot virtual + Puntito)
            aimRoot = new GameObject("VirtualAimRoot").transform;
            aimRoot.SetParent(transform);
            aimRoot.localPosition = Vector3.zero;

            // Creamos la esferita (el puntito de mira)
            dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.transform.SetParent(aimRoot);
            dot.transform.localPosition = new Vector3(dotDistance, 0, 0); 
            dot.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            
            // Eliminar colisiones del punto para que no estorbe físicamente
            Destroy(dot.GetComponent<Collider>());
            
            // Dale un color llamativo (Rojo)
            Renderer r = dot.GetComponent<Renderer>();
            if (r != null)
            {
                // Material simple no afectado por luces complejas para que siempre se vea
                r.material = new Material(Shader.Find("Unlit/Color"));
                r.material.color = Color.red;
            }
        }

        private void Update()
        {
            bool canRenderAim = controller != null && aimRoot != null;
            if (canRenderAim)
            {
                Vector2 dir = controller.AimDirection;

                // Mostrar el punto solo si el joystick se está tocando
                if (dir.sqrMagnitude > joystickDeadzone * joystickDeadzone)
                {
                    dot.SetActive(true);
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    aimRoot.rotation = Quaternion.Euler(0, 0, angle);
                }
                else
                {
                    // Ocultar la mira si no usas el joystick
                    dot.SetActive(false);
                    // Cuando soltamos joystick, apuntamos arriba por defecto por si tira el gancho
                    aimRoot.rotation = Quaternion.Euler(0, 0, 90f);
                }
            }
        }
    }
}
