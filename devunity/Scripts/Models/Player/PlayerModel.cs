using UnityEngine;

namespace ChibiCocina.Models
{
    public class PlayerModel
    {
        public Vector2 MoveInput { get; set; }
        public bool IsRunning { get; set; }
        public bool JumpRequested { get; set; }
        public bool InteractRequested { get; set; }
        public float VerticalVelocity { get; set; }
        public bool IsGrounded { get; set; }
        public GameObject HeldObject { get; set; }
        
        public void ResetInputs()
        {
            MoveInput = Vector2.zero;
            JumpRequested = false;
            InteractRequested = false;
        }
    }
}

