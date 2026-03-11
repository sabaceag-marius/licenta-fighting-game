
using System.Collections.Generic;
using UnityEngine;

public class FallState : BaseState
{
    public FallState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    private bool isFastFalling;

    public override void Enter(Dictionary<string, object> parameters = null)
    {
        base.Enter(parameters);

        isFastFalling = false;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleLogic()
    {
        base.HandleLogic();

        if (characterManager.Input.DodgePressed)
        {
            stateMachine.ChangeState(stateMachine.AirDodgeState);
            return;
        }

        characterManager.HandlePlatformCollisions();

        if (characterManager.RemainingAirJumps > 0 && CheckIfJumping())
        {
            characterManager.RemainingAirJumps--;
            return;
        }

        if (characterManager.Input.FastFalled)
        {
            isFastFalling = true;
        }
        
        if (characterManager.IsGrounded)
        {
            stateMachine.ChangeState(stateMachine.LandState);
        }

        if (CheckIfAttacking())
            return;
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        // Horizontal movement
        
        // No movement or
        // Reached maximum speed 
        if (Mathf.Abs(characterManager.Input.Movement.x) < 0.1f || 
            Mathf.Abs(velocity.x) > characterManager.Stats.AirSpeed)
        {
            velocity.x.Decelerate(characterManager.Stats.AirFriction, Time.fixedDeltaTime);
        }
        else
        {
            // Since we can't turn back when midair, we can't use the FacingDirection to determine
            // in which direction to move the character
            float direction = Mathf.Sign(characterManager.Input.Movement.x);
            
            velocity.x.Accelerate(characterManager.Stats.AirSpeed * direction, characterManager.Stats.AirAcceleration, Time.fixedDeltaTime);
        }
        
        // Falling

        if (isFastFalling)
        {
            velocity.y = - characterManager.Stats.FastFallSpeed;
        }
                
        characterManager.Velocity = velocity;
    }

    protected override bool CheckIfAttacking()
    {
        if (!characterManager.Input.AttackPressed)
            return false;

        AttackType attackType = Mathf.Abs(characterManager.Input.Movement.x) > 0.1f ? AttackType.AirForward : AttackType.AirNeutral;

        stateMachine.ChangeState(stateMachine.AttackState, new Dictionary<string, object> { { AttackState.Param_AttackType, attackType } });

        return true;
    }
}