using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private PlayerInput playerInput;

    #region InputActions

    private InputAction moveInputAction;
    private InputAction jumpInputAction;
    private InputAction dodgeInputAction;
    private InputAction attackInputAction;

    #endregion

    [Header("Settings")]

    [SerializeField]
    [Range(0, 1)]
    private float RightStickMinimumValue = 0.5f;

    [SerializeField] 
    private bool ConsoleLog;

    public RawInput GetRawInput()
    {
        RawInput input = new RawInput();

        if (playerInput == null)
            return input;
            
        // Left stick - analog

        Vector2 leftAnalog = moveInputAction.ReadValue<Vector2>();

        input.LeftStickX = (sbyte)(leftAnalog.x * 100f);
        input.LeftStickY = (sbyte)(leftAnalog.y * 100f);

        //TODO: Right stick - only store if we are holding the direction

        // Buttons

        if (jumpInputAction.IsPressed())
            input.Buttons |= (1 << 0);

        if (attackInputAction.IsPressed())
            input.Buttons |= (1 << 1);

        //if (specialAttackInputAction.IsPressed())
        //    input.Buttons |= (1 << 2);

        if (dodgeInputAction.IsPressed())
            input.Buttons |= (1 << 3);

        //if (grabInputAction.IsPressed())
        //    input.Buttons |= (1 << 4);

        if (ConsoleLog)
            LogInput(input);

        return input;
    }

    private void LogInput(RawInput input)
    {
        Debug.Log($"Movement: ({input.LeftStickX},{input.LeftStickY}); Jumped: {input.Jumped};" +
            $"Attacked {input.Attacked}; Dodged {input.Dodged}");
    }

    public void Initialize(PlayerInput assignedInput)
    {
        playerInput = assignedInput;

        //TODO: remove magic strings
        
        moveInputAction = playerInput.actions["Move"];
        jumpInputAction = playerInput.actions["Jump"];
        dodgeInputAction = playerInput.actions["Dodge"];
        attackInputAction = playerInput.actions["Attack"];
    }
}
