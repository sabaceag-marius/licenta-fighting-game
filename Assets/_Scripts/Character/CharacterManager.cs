using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterManager : MonoBehaviour, ICharacterManager
{
    [SerializeField]
    private float GroundCheckDistance = 0.02f;

    public Vector2 Velocity { get; set; }

    [SerializeField]
    private CharacterStats stats;

    public CharacterStats Stats
    {
        get => stats;
        set => stats = value;
    }
    
     public bool IsGrounded { get; set; }

     public int FacingDirection { get; set; } = 1;
     
     public FrameInput Input => inputManager.CurrentFrameInput;

    public int RemainingAirJumps { get; set; }

    #region Components

    private Rigidbody2D rigidbody;

    private Collider2D collider;

    private InputManager inputManager;

    private SpriteRenderer spriteRenderer;
    
    #endregion

    private CharacterStateMachine stateMachine;
    
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
        inputManager = GetComponent<InputManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        stateMachine = new CharacterStateMachine(this);
    }

    private void Update()
    {
        stateMachine.CurrentState.HandleLogic();
    }

    private void FixedUpdate()
    {
        CheckCollisions();

        stateMachine.CurrentState.HandlePhysics();

        ApplyMovement();
    }
    
    private void CheckCollisions()
    {
        var groundHits = new RaycastHit2D[2];
        
        IsGrounded = collider.Cast(Vector2.down, groundHits, GroundCheckDistance) > 0;
    }
    
    private void ApplyMovement()
    {
        rigidbody.velocity = Velocity;
    }
    
    public void ChangeColor(Color color)
    {
        spriteRenderer.color = color;
    }
    
    public void Flip()
    {
        FacingDirection *= -1;

        var scale = transform.localScale;

        scale.x *= -1;

        transform.localScale = scale;
    }
}
