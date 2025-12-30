
using UnityEngine;

public class RunState : BaseState
{
    public RunState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }
    
    public override void HandleLogic()
    {
        base.HandleLogic();
        
        if (CheckIfFalling())
            return;
        
        if (CheckIfJumping())
            return;
        
        if (characterManager.FacingDirection * characterManager.Input.Movement.x < 0)
        {
            stateMachine.ChangeState(stateMachine.TurnAroundState);
            return;
        }
        
        if (Mathf.Abs(characterManager.Velocity.x) <= 0.1f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        // We decrease the speed if the movement stick is neutral (as opposed to moving to IdleState)
        // or if we go beyond the maximum running speed
        
        if (Mathf.Abs(characterManager.Input.Movement.x) < 0.1f || Mathf.Abs(velocity.x) > characterManager.Stats.RunningSpeed)
        {
            velocity.x.Decelerate(characterManager.Stats.Traction, Time.fixedDeltaTime);
        }
        else
        {
            velocity.x.Accelerate(characterManager.Stats.RunningSpeed * characterManager.FacingDirection, characterManager.Stats.DashAcceleration, Time.fixedDeltaTime);
        }
        
        characterManager.Velocity = velocity;
    }
}