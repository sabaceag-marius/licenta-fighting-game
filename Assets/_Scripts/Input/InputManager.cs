using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    private InputActions inputActions;

    #region InputActions
    
    private InputAction moveInputAction;
    private InputAction jumpInputAction;
    
    #endregion
    
    public FrameInput CurrentFrameInput  => InputBuffer.LastOrDefault() ?? new FrameInput();

    public Queue<FrameInput> InputBuffer;

    [SerializeField] private float DashDifferenceTreshhold = 0.45f;

    [SerializeField] private int BufferSize = 10;

    [SerializeField] private bool ConsoleLog;
    
    void Awake()
    {
        inputActions = new InputActions();

        moveInputAction = inputActions.Player.Move;
        jumpInputAction = inputActions.Player.Jump;

        InputBuffer = new Queue<FrameInput>();
    }

    private void GatherInput()
    {
        FrameInput frameInput = new FrameInput
        {
            Movement = moveInputAction.ReadValue<Vector2>(),
            JumpPressed = jumpInputAction.WasPressedThisFrame(),
            JumpHeld = jumpInputAction.IsPressed(),
        };

        frameInput.Dashed = 
            Mathf.Abs(frameInput.Movement.x) - Mathf.Abs(CurrentFrameInput.Movement.x) >= DashDifferenceTreshhold 
            && Mathf.Abs(frameInput.Movement.x) >= .9f;

        frameInput.FastFalled =
            Mathf.Abs(frameInput.Movement.y) - Mathf.Abs(CurrentFrameInput.Movement.y) >= DashDifferenceTreshhold 
            && frameInput.Movement.y <= -.75f;
        
        if (ConsoleLog)
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
    
    private void Update() => GatherInput();
    private void OnEnable() => inputActions.Enable();
    private void OnDisable() => inputActions.Disable();
}

public class FrameInput
{
    [Obsolete]
    public int Direction { get; set; }
    
    public Vector2 Movement { get; set; }

    public bool JumpPressed { get; set; }
    
    public bool JumpHeld { get; set; }
    
    public bool Dashed { get; set; }
    
    public bool FastFalled { get; set; }
    
    [Obsolete]
    public bool ChangedDirection { get;set; }
}
