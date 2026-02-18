using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

public abstract class BaseState
{
    public bool CanControl { get; set; }

    public Color SpriteColor { get; set; } = Color.cyan;

    protected readonly ICharacterManager characterManager;
    protected readonly CharacterStateMachine stateMachine;
    
    public BaseState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color)
    {
        this.characterManager = characterManager;
        this.stateMachine = stateMachine;

        SpriteColor = color;
    }

    public virtual void Enter(Dictionary<string, object> parameters = null)
    {
        characterManager.ChangeColor(SpriteColor);

        if (parameters != null && parameters.Count > 0)
        {
            ApplyParameters(parameters);
        }
    }

    public virtual void Exit()
    {
        
    }

    public virtual void HandleLogic()
    {
        
    }

    public virtual void HandlePhysics()
    {
        Vector2 velocity = characterManager.Velocity;

        if (characterManager.IsGrounded)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y.Accelerate(-characterManager.Stats.FallSpeed, characterManager.Stats.Gravity, Time.fixedDeltaTime);
        }

        characterManager.Velocity = velocity;
    }

    protected bool CheckIfFalling()
    {
        if (characterManager.IsGrounded)
        {
            return false;
        }

        stateMachine.ChangeState(stateMachine.FallState);

        return true;
    }
    
    protected bool CheckIfJumping()
    {
        if (!characterManager.Input.JumpPressed)
        {
            return false;
        }

        stateMachine.ChangeState(stateMachine.JumpState);

        return true;
    }

    protected virtual bool CheckIfAttacking()
    {
        return false;
    }

    private void ApplyParameters(Dictionary<string, object> parameters)
    {
        var type = this.GetType();

        // 1. Handle Fields
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<FromParameterAttribute>();

            if (attribute != null)
            {
                if (parameters.TryGetValue(attribute.ParameterName, out object value))
                {
                    // Check if value is assignable to the field type
                    if (value != null && field.FieldType.IsAssignableFrom(value.GetType()))
                    {
                        field.SetValue(this, value);
                    }
                    else
                    {
                        Debug.LogWarning($"State {type.Name}: Parameter '{attribute.ParameterName}' exists but type {value?.GetType()} cannot be cast to {field.FieldType}. Keeping default value.");
                    }
                }
            }
        }

        // 2. Handle Properties
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            var attribute = prop.GetCustomAttribute<FromParameterAttribute>();

            if (attribute != null && prop.CanWrite)
            {
                if (parameters.TryGetValue(attribute.ParameterName, out object value))
                {
                    if (value != null && prop.PropertyType.IsAssignableFrom(value.GetType()))
                    {
                        prop.SetValue(this, value);
                    }
                    else
                    {
                        Debug.LogWarning($"State {type.Name}: Parameter '{attribute.ParameterName}' exists but type {value?.GetType()} cannot be cast to {prop.PropertyType}. Keeping default value.");
                    }
                }
            }
        }
    }
}
