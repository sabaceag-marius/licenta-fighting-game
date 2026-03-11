
using System.Collections.Generic;
using UnityEngine;

public class AirDodgeState : BaseState
{
    public AirDodgeState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    Timer dodgeTimer;

    Vector2 dodgeDirection;

    public override void Enter(Dictionary<string, object> parameters = null)
    {
        base.Enter(parameters);

        dodgeDirection = characterManager.Input.Movement.normalized;

        characterManager.Velocity = dodgeDirection * characterManager.Stats.AirDodgePower;

        dodgeTimer = new Timer(characterManager.Stats.AirDodgeFrames);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleLogic()
    {
        base.HandleLogic();

        if (characterManager.IsGrounded)
        {
            characterManager.Velocity = dodgeDirection * characterManager.Stats.AirDodgePower;

            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }

        if (!dodgeTimer.IsDone())
            return;

        stateMachine.ChangeState(stateMachine.FallState);
    }

    public override void HandlePhysics()
    {
        //base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        velocity.x.Decelerate(characterManager.Stats.AirDodgeTraction, Time.fixedDeltaTime);
        velocity.y.Decelerate(characterManager.Stats.AirDodgeTraction, Time.fixedDeltaTime);

        characterManager.Velocity = velocity;
    }
}