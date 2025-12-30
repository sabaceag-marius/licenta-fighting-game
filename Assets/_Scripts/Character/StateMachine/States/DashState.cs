
using System;
using UnityEngine;

public class DashState : BaseState
{
    private Timer dashTimer;

    public DashState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (characterManager.FacingDirection * characterManager.Input.Movement.x < 0)
        {
            characterManager.Flip();
        }
        
        Vector2 velocity = characterManager.Velocity;

        velocity.x = characterManager.FacingDirection * characterManager.Stats.InitialDashSpeed;
        
        characterManager.Velocity = velocity;

        dashTimer = new Timer(characterManager.Stats.DashFrames);
    }

    public override void HandleLogic()
    {
        base.HandleLogic();

        if (CheckIfFalling())
            return;
        
        if (CheckIfJumping())
            return;
        
        //Dash dance
        if (characterManager.Input.Dashed && characterManager.FacingDirection * characterManager.Input.Movement.x < 0)
        {
            stateMachine.ChangeState(stateMachine.DashState);
            return;
        }
        
        // Minimum dash time
        if (!dashTimer.IsDone())
            return;
        
        // Do not transition to the next state until we reach the running speed
        if (Mathf.Abs(characterManager.Velocity.x) < Mathf.Abs(characterManager.Stats.RunningSpeed))
            return;
        
        if (Mathf.Abs(characterManager.Input.Movement.x) >= 0.1f)
        {
            stateMachine.ChangeState(stateMachine.RunState);
        }
        else
        {
            stateMachine.ChangeState(stateMachine.IdleState);
        }
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        velocity.x.Accelerate(characterManager.Stats.RunningSpeed * characterManager.FacingDirection, characterManager.Stats.DashAcceleration, Time.fixedDeltaTime);
        
        characterManager.Velocity = velocity;
    }
}