using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputActions inputActions;

    #region InputActions
    
    private InputAction moveInputAction;
    private InputAction jumpInputAction;
    private InputAction dodgeInputAction;
    private InputAction attackInputAction;

    #endregion

    public FrameInput CurrentFrameInput  => InputBuffer.LastOrDefault() ?? new FrameInput();

    public Queue<FrameInput> InputBuffer;

    [FormerlySerializedAs("DashDifferenceTreshhold")]
    [SerializeField] private float FlickDifferenceTreshhold = 0.45f;

    [SerializeField] private float MinimumFlickValue = 0.75f;

    [SerializeField] private int BufferSize = 10;

    [SerializeField] private bool ConsoleLog;
    
    void Awake()
    {
        inputActions = new InputActions();

        playerInput = GetComponent<PlayerInput>();

        //inputActions.devices = new ReadOnlyArray<InputDevice>(new[] { playerInput.devices[0] });

        moveInputAction = inputActions.Player.Move;
        jumpInputAction = inputActions.Player.Jump;
        dodgeInputAction = inputActions.Player.Dodge;
        attackInputAction = inputActions.Player.Attack;

        InputBuffer = new Queue<FrameInput>();
    }

    private void GatherInput()
    {
        FrameInput frameInput = new FrameInput
        {
            Movement = moveInputAction.ReadValue<Vector2>(),
            JumpPressed = jumpInputAction.WasPressedThisFrame(),
            JumpHeld = jumpInputAction.IsPressed(),
            DodgePressed = dodgeInputAction.WasPressedThisFrame(),
            AttackPressed = attackInputAction.WasPressedThisFrame()
        };

        Vector2 flickDirection = Vector2.zero;

        if (Mathf.Abs(frameInput.Movement.x) - Mathf.Abs(CurrentFrameInput.Movement.x) >= MinimumFlickValue
            && Mathf.Abs(frameInput.Movement.x) >= MinimumFlickValue)
        {
            flickDirection.x = Mathf.Sign(frameInput.Movement.x);
        }

        if (Mathf.Abs(frameInput.Movement.y) - Mathf.Abs(CurrentFrameInput.Movement.y) >= MinimumFlickValue
            && Mathf.Abs(frameInput.Movement.y) >= MinimumFlickValue)
        {
            flickDirection.y = Mathf.Sign(frameInput.Movement.y);
        }

        frameInput.FlickDirection = flickDirection;

        frameInput.Dashed = Mathf.Abs(frameInput.FlickDirection.x) > 0;

        frameInput.FastFalled = frameInput.FlickDirection.y == -1;
        
        if (ConsoleLog)
        {
            LogInputs(frameInput);
        }

        if (InputBuffer.Count < BufferSize)
        {
            InputBuffer.Enqueue(frameInput);
        }
        else
        {
            InputBuffer.Dequeue();
            InputBuffer.Enqueue(frameInput);
        }
    }

    private void LogInputs(FrameInput frameInput)
    {
        if (frameInput.Movement != Vector2.zero)
        {
            // Debug.Log($"Direction: {frameInput.Direction}");
        }

        if (frameInput.JumpPressed)
        {
            Debug.Log($"Jump pressed!");
        }

        if (frameInput.JumpHeld)
        {
            Debug.Log($"Jump is being held!");
        }

        if (frameInput.Dashed)
        {
            Debug.Log("Dashed!" + $" {CurrentFrameInput.Direction} {frameInput.Direction}");
        }

        if (frameInput.FastFalled)
        {
            Debug.Log("Fast falled!");
        }

        if (frameInput.DodgePressed)
        {
            Debug.Log("Dodged!");
        }
    }

    private void Update() => GatherInput();
    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
}

public class FrameInput
{
    [Obsolete]
    public int Direction { get; set; }
    
    public Vector2 Movement { get; set; }

    public Vector2 FlickDirection { get; set; }

    public bool JumpPressed { get; set; }

    public bool JumpHeld { get; set; }
    
    public bool Dashed { get; set; }
    
    public bool FastFalled { get; set; }
    
    public bool DodgePressed { get; set; }

    public bool AttackPressed { get; set; }

    [Obsolete]
    public bool ChangedDirection { get;set; }
}
