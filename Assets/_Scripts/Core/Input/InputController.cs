using System.Collections;
using System.Collections.Generic;
using FluentAssertions.Formatting;
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

        Vector2 rightAnalog = moveInputAction.ReadValue<Vector2>();
        
        // public byte RightStick; //0000LeftRightUpDown

        if (rightAnalog.y <= -0.5f)
            input.RightStick |= (1 << 0);

        if (rightAnalog.y >= 0.5f)
            input.RightStick |= (1 << 1);

        if (rightAnalog.x >= 0.5f)
            input.RightStick |= (1 << 2);

        if (rightAnalog.x <= -0.5f)
            input.RightStick |= (1 << 3);

        // Buttons

        if (jumpInputAction.IsPressed())
            input.Buttons |= (1 << 0);

        if (attackInputAction.IsPressed())
            input.Buttons |= (1 << 1);

        if (dodgeInputAction.IsPressed())
            input.Buttons |= (1 << 3);

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

        moveInputAction = playerInput.actions["Move"];
        jumpInputAction = playerInput.actions["Jump"];
        dodgeInputAction = playerInput.actions["Dodge"];
        attackInputAction = playerInput.actions["Attack"];
    }
}
