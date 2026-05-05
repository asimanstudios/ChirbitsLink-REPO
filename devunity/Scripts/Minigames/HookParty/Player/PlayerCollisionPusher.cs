using UnityEngine;

namespace ChibitsLink.GameSide.HookParty
{
    /// <summary>
    /// Gestiona colisiones del jugador e introduce una fuerza hiperbólica (rebote smash)
    /// si chocamos en el aire o fuertemente contra los compañeros de equipo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerCollisionPusher : MonoBehaviour
    {
        [Header("Collision Push")]
        [Tooltip("Etiqueta para identificar a otro jugador (ej: 'Player')")]
        [SerializeField] private string playerTag = "Player";
        [Tooltip("Cuanto rebote aplicará al embestir/chocar contra el compañero")]
        [SerializeField] private float bounceForce = 15f;

        private Rigidbody _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            // Verificamos si chocamos contra otro jugador
            if (collision.gameObject.CompareTag(playerTag))
            {
                // Vector que se aleja del punto de impacto hacia este jugador 
                // para que rebote en dirección opuesta
                Vector3 awayFromContact = (transform.position - collision.transform.position).normalized;
                
                // Forzamos un pequeño "up-kick" hacia arriba para darle más jugo estilo Smash y que salgan despedidos
                awayFromContact.y += 0.2f;
                
                _rb.AddForce(awayFromContact.normalized * bounceForce, ForceMode.Impulse);
                
                // TODO: Aquí invocarías un sonido Vfx/Sfx de choque "BONK/POING"
            }
        }
    }
}
