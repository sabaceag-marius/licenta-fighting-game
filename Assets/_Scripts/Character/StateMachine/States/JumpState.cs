
using UnityEngine;

public class JumpState : BaseState
{
    private Timer jumpSquatTimer;

    private float jumpForce;
    
    public JumpState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    public override void Enter()
    {
        base.Enter();

        jumpSquatTimer = new Timer(characterManager.Stats.JumpWindupFrames);
        jumpForce = 0;
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleLogic()
    {
        base.HandleLogic();

        if (jumpSquatTimer.IsDone() && jumpForce == 0)
        {
            jumpForce = characterManager.Input.JumpHeld
                ? characterManager.Stats.JumpForce
                : characterManager.Stats.JumpForce * 0.5f;

            return;
        }
        
        if (!jumpSquatTimer.IsDone() || characterManager.Velocity.y <= 0)
            return;
        
        stateMachine.ChangeState(stateMachine.FallState);
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        if (jumpSquatTimer.IsDone() && velocity.y <= 0)
        {
            velocity.y = jumpForce;
        }
        
        characterManager.Velocity = velocity;
    }
}