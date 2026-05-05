using UnityEngine;

namespace ChibiCocina.Models
{
    public class MovementModel
    {
        public float WalkSpeed { get; set; } = 4f;
        public float RunSpeed { get; set; } = 7f;
        public float Acceleration { get; set; } = 12f;
        public float Friction { get; set; } = 10f;
        public float AirControl { get; set; } = 0.6f;
        public float RotationSpeed { get; set; } = 15f;
        public float LerpSpeed { get; set; } = 25f;
        
        public Vector3 CurrentVelocity { get; set; }
        public Vector3 ExternalForce { get; set; }
        public Vector2 MoveInput { get; set; }
        public Vector2 TargetMoveInput { get; set; }
        public bool IsRunning { get; set; }
        
        public void ApplyCombatStats()
        {
            WalkSpeed = 6.5f;
            RunSpeed = 11f;
            Acceleration = 35f;
            AirControl = 1.0f;
            RotationSpeed = 25f;
        }
        
        public void ResetVelocity()
        {
            CurrentVelocity = Vector3.zero;
            ExternalForce = Vector3.zero;
        }
    }
}
