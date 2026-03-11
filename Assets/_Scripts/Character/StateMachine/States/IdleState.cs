
using System;
using System.Collections.Generic;
using UnityEngine;

public class IdleState : BaseState
{
    public IdleState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    public override void Enter(Dictionary<string, object> parameters = null)
    {
        base.Enter(parameters);

        characterManager.RemainingAirJumps = characterManager.Stats.AirJumpCount;
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
        
        if (MathF.Abs(characterManager.Input.Movement.x) >= 0.1f)
        {
            stateMachine.ChangeState(stateMachine.WalkState);
            return;
        }

        if (CheckIfAttacking())
            return;
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        velocity.x.Decelerate(characterManager.Stats.Traction, Time.fixedDeltaTime);
        
        characterManager.Velocity = velocity;
    }

    protected override bool CheckIfAttacking()
    {
        if (!characterManager.Input.AttackPressed)
            return false;

        stateMachine.ChangeState(stateMachine.AttackState, new Dictionary<string, object> { {AttackState.Param_AttackType, AttackType.GroundForward} });

        return true;
    }
}