
using System;
using UnityEngine;

public class WalkState : BaseState
{
    public WalkState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    public override void HandleLogic()
    {
        base.HandleLogic();

        characterManager.HandlePlatformCollisions();

        if (CheckIfFalling())
            return;
        
        if (CheckIfJumping())
            return;
        
        if (characterManager.Input.Dashed)
        {
            stateMachine.ChangeState(stateMachine.DashState);
            return;
        }
        
        if (MathF.Abs(characterManager.Input.Movement.x) <= 0.1f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        if (MathF.Abs(velocity.x) > characterManager.Stats.WalkSpeed)
        {
            velocity.x.Decelerate(characterManager.Stats.Traction, Time.fixedDeltaTime);
        }
        else
        {
            float directionXAmount = Mathf.Abs(characterManager.Input.Movement.x);

            // We want to have different speeds based on how far away the movement stick is
            float walkSpeedModifier =
                directionXAmount < 0.625f ? 0.5f :
                directionXAmount < 0.875f ? 0.75f :
                1f;

            velocity.x = characterManager.Stats.WalkSpeed * walkSpeedModifier * characterManager.FacingDirection;
        }

        if (characterManager.FacingDirection * characterManager.Input.Movement.x < 0)
        {
            characterManager.Flip();
        }
        
        characterManager.Velocity = velocity;
    }
}