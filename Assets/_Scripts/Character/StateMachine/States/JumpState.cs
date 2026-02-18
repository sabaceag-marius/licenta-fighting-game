
using System.Collections.Generic;
using UnityEngine;

public class JumpState : BaseState
{
    private Timer jumpSquatTimer;

    private float jumpForce;

    private bool isGroundedJump;

    public JumpState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    public override void Enter(Dictionary<string, object> parameters = null)
    {
        base.Enter(parameters);

        isGroundedJump = characterManager.IsGrounded;
        jumpSquatTimer = new Timer(isGroundedJump ? characterManager.Stats.JumpWindupFrames : 0);
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
            if (isGroundedJump)
            {
                jumpForce = characterManager.Input.JumpHeld
                    ? characterManager.Stats.NormalJumpForce
                    : characterManager.Stats.ShortJumpForce;
            }
            else
            {
                jumpForce = characterManager.Stats.AirJumpForce;
            }

            return;
        }
        
        if (!jumpSquatTimer.IsDone() || characterManager.Velocity.y <= 0)
            return;

        if (characterManager.Input.DodgePressed)
        {
            stateMachine.ChangeState(stateMachine.AirDodgeState);
            return;
        }

        stateMachine.ChangeState(stateMachine.FallState);
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        Vector2 velocity = characterManager.Velocity;

        if (jumpSquatTimer.IsDone() && jumpForce != 0)
        {
            velocity.y = jumpForce;
        }
        
        characterManager.Velocity = velocity;
    }
}