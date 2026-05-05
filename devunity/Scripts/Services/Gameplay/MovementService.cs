using UnityEngine;
using ChibiCocina.Models;

public class MovementService : MonoBehaviour
{
    [Header("Movement Config - Cocina Only")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float acceleration = 12f;
    public float friction = 10f;
    public float airControl = 0.6f;
    public float rotationSpeed = 15f;
    public float gravity = -20f;
    
    [Header("Jump - Cocina Only")]
    public float jumpForce = 5f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.2f;

    private CharacterController controller;
    private Transform transformRef;
    private PlayerModel playerModel;

    private Vector2 moveInput;
    private Vector3 currentVelocity;
    private Vector3 verticalVelocity;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    public void Initialize(CharacterController charController, Transform playerTransform, PlayerModel model)
    {
        controller = charController;
        transformRef = playerTransform;
        playerModel = model;
    }

    public void UpdateMovement()
    {
        bool canUpdateMovement = controller != null && controller.enabled;
        if (canUpdateMovement)
        {
            // NO early returns: usar guards
            if (playerModel.MoveInput.magnitude > 0.01f)
            {
                ApplyHorizontalMovement(playerModel.MoveInput);
            }
            else
            {
                ApplyFriction();
            }

            CheckGround();
            HandleJump();
            ApplyGravity();
            controller.Move((currentVelocity + verticalVelocity) * Time.deltaTime);
        }
    }

    private void ApplyHorizontalMovement(Vector2 input)
    {
        float targetSpeed = (playerModel.IsRunning ? runSpeed : walkSpeed) * input.magnitude;
        Vector3 direction = new Vector3(input.x, 0, input.y).normalized;
        
        float accel = playerModel.IsGrounded ? acceleration : acceleration * airControl;
        currentVelocity = Vector3.Lerp(currentVelocity, direction * targetSpeed, accel * Time.deltaTime);

        // Rotate
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transformRef.rotation = Quaternion.Slerp(transformRef.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    private void ApplyFriction()
    {
        currentVelocity = Vector3.Lerp(currentVelocity, Vector3.zero, friction * Time.deltaTime);
    }

    private void CheckGround()
    {
        playerModel.IsGrounded = controller.isGrounded;
        if (playerModel.IsGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }

        bool shouldDecayCoyoteTime = !playerModel.IsGrounded && coyoteTimeCounter > 0;
        if (shouldDecayCoyoteTime)
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        if (playerModel.JumpRequested && coyoteTimeCounter > 0 && jumpBufferCounter > 0)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            playerModel.JumpRequested = false;
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0;
        }

        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;
    }

    private void ApplyGravity()
    {
        if (playerModel.IsGrounded && verticalVelocity.y < 0)
        {
            verticalVelocity.y = -2f;
        }

        float multiplier = 1f;
        if (verticalVelocity.y < 0)
        {
            multiplier = fallMultiplier;
        }

        bool isRisingWithoutJumpHeld = verticalVelocity.y > 0 && !JumpHeld();
        if (isRisingWithoutJumpHeld) // Trackear button state en model
        {
            multiplier = lowJumpMultiplier;
        }

        verticalVelocity.y += gravity * multiplier * Time.deltaTime;
    }

    private bool JumpHeld()
    {
        return playerModel.JumpRequested; // Expandir model para track
    }
}

