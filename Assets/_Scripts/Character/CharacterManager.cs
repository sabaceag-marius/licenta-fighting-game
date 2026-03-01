using Project.Tools.DictionaryHelp;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CharacterManager : MonoBehaviour, ICharacterManager
{
    [SerializeField]
    private bool DrawDebug = true;

    #region Collisions

    [SerializeField]
    private float GroundCheckDistance = 0.02f;

    [SerializeField]
    private LayerMask GroundLayer;

    [SerializeField]
    private Vector2 GroundCheckSize = new Vector2(0.8f, 0.05f);

    private Collider2D platformCollider;

    private Collider2D currentGroundCollider;

    #endregion

    [SerializeField]
    private SerializableDictionary<AttackType, AttackDataSO> attackData;

    public Vector2 Velocity { get; set; }

    [SerializeField]
    private CharacterStats stats;

    public CharacterStats Stats
    {
        get => stats;
        set => stats = value;
    }
    
     public bool IsGrounded { get; set; }

     public int FacingDirection { get; set; } = -1; // 1 - right, -1 left

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
        if (rigidbody.velocity.y > 0.01f)
        {
            IsGrounded = false;
            return;
        }

        // define the position for the box cast (center of the feet)
        Vector2 boxCenter = (Vector2)transform.position + collider.offset + (Vector2.down * (collider.bounds.size.y / 2f));

        RaycastHit2D hit = Physics2D.BoxCast(
            boxCenter,
            GroundCheckSize,
            0f,
            Vector2.down,
            GroundCheckDistance,
            GroundLayer
        );

        if (hit.collider != null && hit.collider != platformCollider)
        {
            IsGrounded = true;
            currentGroundCollider = hit.collider;
        }
        else
        {
            IsGrounded = false;
            currentGroundCollider = null;
        }

        if (DrawDebug)
        {
            Color rayColor = IsGrounded ? Color.green : Color.red;
            Debug.DrawRay(boxCenter + new Vector2(-GroundCheckSize.x / 2, -GroundCheckDistance), Vector2.right * GroundCheckSize.x, rayColor);
            Debug.DrawRay(boxCenter + new Vector2(-GroundCheckSize.x / 2, 0), Vector2.down * GroundCheckDistance, rayColor);
            Debug.DrawRay(boxCenter + new Vector2(GroundCheckSize.x / 2, 0), Vector2.down * GroundCheckDistance, rayColor);
        }
    }

    void ICharacterManager.HandlePlatformCollisions()
    {
        if (!IsGrounded && rigidbody.velocity.y < 0 && Input.Movement.y < -0.5f)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, GroundLayer);

            if (hit.collider != null && hit.collider.CompareTag("OneWay"))
            {
                StartCoroutine(DisablePlatformCoroutine(hit.collider));
            }
        }

        //TODO: Add the value for shield dropping
        if (IsGrounded && Input.FlickDirection.y == -1)
        {
            if (currentGroundCollider != null && currentGroundCollider.CompareTag("OneWay"))
            {
                StartCoroutine(DisablePlatformCoroutine(currentGroundCollider));
            }
        }
    }

    private IEnumerator DisablePlatformCoroutine(Collider2D platformCollider)
    {
        this.platformCollider = platformCollider;

        Physics2D.IgnoreCollision(collider, platformCollider, true);

        yield return new WaitForSeconds(0.5f);

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, collider.bounds.size, 0, Vector2.zero, 0, GroundLayer);

        while (hit.collider == platformCollider)
        {
            yield return null;

            hit = Physics2D.BoxCast(transform.position, collider.bounds.size, 0, Vector2.zero, 0, GroundLayer);
        }

        Physics2D.IgnoreCollision(collider, platformCollider, false);
        this.platformCollider = null;
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

    public T GetGameObjectComponent<T>()
    {
        T component = GetComponent<T>();

        if (component == null)
            throw new Exception($"Component of type {nameof(T)} does not exist on this GameObject");

        return component;
    }
    public AttackDataSO? GetAttack(AttackType attackType)
    {
        if (!attackData.ContainsKey(attackType))
            return null;

        return attackData[attackType];
    }
}
