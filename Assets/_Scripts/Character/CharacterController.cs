using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    #region Components

    private Rigidbody2D rigidBody;

    private BoxCollider2D collider;

    private InputManager inputManager;
    
    #endregion

    private Vector2 velocity;

    private bool isGrounded = true;

    [SerializeField] private bool Log;
    
    [SerializeField] 
    private LayerMask GroundLayerMask;
    
    private FrameInput currentFrameInput => inputManager.CurrentFrameInput;

    [SerializeField] 
    private CharacterStats characterStats;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        inputManager = GetComponent<InputManager>();
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        HandleGroundCollisions();
        HandleDashing();
        HandleHorizontalVelocity();
        HandleVerticalVelocity();
        ApplyMovement();
    }

    private bool isJumping => isGrounded && currentFrameInput.JumpPressed;

    private bool isDashing;
    private bool isRunning;
    private void HandleDashing()
    {
        if (isRunning && ( velocity.x.Abs() <= 0.1f || !isGrounded))
        {
            isRunning = false;
        }
        
        if (isDashing && velocity.x >= characterStats.RunningSpeed)
        {
            isDashing = false;
            isRunning = true;
        }
        
        if (currentFrameInput.Dashed && isGrounded && !isRunning)
        {
            isDashing = true;
            velocity.x = currentFrameInput.Direction * characterStats.InitialDashSpeed;
        }
    }   
    
    private void HandleGroundCollisions()
    {
        var groundHits = new RaycastHit2D[2];
        
        isGrounded = collider.Cast(Vector2.down, groundHits, 0.05f) > 0;
    }
    private void ApplyMovement()
    {
        rigidBody.velocity = velocity;
    }

    private void HandleHorizontalVelocity()
    {
        if (isDashing)
        {
            velocity.x.Accelerate(characterStats.RunningSpeed * currentFrameInput.Direction, characterStats.DashAcceleration, Time.fixedDeltaTime);
            return;
        }
        
        if (currentFrameInput.Movement.x != 0 && !currentFrameInput.ChangedDirection)
        {
            float movementXAbs = currentFrameInput.Movement.x.Abs();
            
            // Take the closest value between 0.5, 0.75 and 1 to multiply as a modifier to the velocity
            float modifier = 
                movementXAbs < 0.625f ? 0.5f :
                movementXAbs < 0.875f ? 0.75f :
                1f;

            float speed = isRunning ? characterStats.RunningSpeed : characterStats.WalkSpeed;
            
            velocity.x = speed * modifier * currentFrameInput.Direction;
            Debug.Log(velocity.x);
        }
        else
        {
            velocity.x.Decelerate(characterStats.Traction, Time.fixedDeltaTime);
        }
    }

    private void HandleVerticalVelocity()
    {
        if (isJumping)
        {
            velocity.y = characterStats.NormalJumpForce;
            
            return;
        }
        
        if (isGrounded)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y.Accelerate(- characterStats.Gravity, characterStats.FallSpeed, Time.fixedDeltaTime);
        }
    }
}
