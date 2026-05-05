using UnityEngine;

namespace ChibitsLink.GameSide
{
    /// <summary>
    /// Componente para identificar de forma única a un jugador en la escena sin depender del nombre del GameObject.
    /// </summary>
    public class PlayerIdentity : MonoBehaviour
    {
        public string userId;
        public string username;
        public int level = 1;
    }
}
