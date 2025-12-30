using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    
    public virtual void Enter()
    {
        characterManager.ChangeColor(SpriteColor);
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
            velocity.y.Accelerate(- characterManager.Stats.Gravity, characterManager.Stats.FallSpeed, Time.fixedDeltaTime);
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
}
