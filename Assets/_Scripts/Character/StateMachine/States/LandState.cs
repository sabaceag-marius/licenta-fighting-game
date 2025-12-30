using UnityEngine;

public class LandState : BaseState
{
    public LandState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    private Timer landTimer;

    public override void Enter()
    {
        base.Enter();

        landTimer = new Timer(characterManager.Stats.LandLagFrames);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void HandleLogic()
    {
        base.HandleLogic();
        
        if (!landTimer.IsDone())
            return;
        
        stateMachine.ChangeState(stateMachine.IdleState);
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();
        
        Vector2 velocity = characterManager.Velocity;

        velocity.x.Decelerate(characterManager.Stats.Traction, Time.fixedDeltaTime);

        velocity.y = 0;
        
        characterManager.Velocity = velocity;
    }
}
