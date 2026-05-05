using UnityEngine;

namespace ChibiCocina.Models
{
    public class JumpModel
    {
        public float JumpForce { get; set; } = 5f;
        public float Gravity { get; set; } = -20f;
        public float FallMultiplier { get; set; } = 2.5f;
        public float LowJumpMultiplier { get; set; } = 2f;
        public float CoyoteTime { get; set; } = 0.15f;
        public float JumpBufferTime { get; set; } = 0.2f;
        
        public Vector3 VerticalVelocity { get; set; }
        public bool IsGrounded { get; set; }
        public float CoyoteTimeCounter { get; set; }
        public float JumpBufferCounter { get; set; }
        public int AirJumpsRemaining { get; set; } = 1;
        public bool WasGrounded { get; set; }
        
        public void ApplyCombatStats()
        {
            Gravity = -20f;
            JumpForce = 5f;
            FallMultiplier = 2.5f;
            LowJumpMultiplier = 2f;
        }
        
        public void ResetJumpState()
        {
            VerticalVelocity = Vector3.zero;
            CoyoteTimeCounter = 0f;
            JumpBufferCounter = 0f;
            AirJumpsRemaining = 1;
        }
        
        public void OnLanded()
        {
            CoyoteTimeCounter = CoyoteTime;
            AirJumpsRemaining = 1;
        }
    }
}
