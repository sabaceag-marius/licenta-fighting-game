
using System;
using UnityEngine;

public class TurnAroundState : BaseState
{
    public TurnAroundState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    public override void Enter()
    {
        base.Enter();
        
        //TODO: move the flip to Exit when adding animations ?
        characterManager.Flip();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleLogic()
    {
        base.HandleLogic();
        
        if (CheckIfFalling())
            return;
        
        if (CheckIfJumping())
            return;
        
        if (Mathf.Abs(characterManager.Velocity.x) > 0.1f)
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

        velocity.x.Decelerate(characterManager.Stats.Traction, Time.fixedDeltaTime);
        
        characterManager.Velocity = velocity;
    }
}