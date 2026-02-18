using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateMachine
{
    public BaseState CurrentState { get; private set; }

    private readonly ICharacterManager characterManager;

    #region States

    public readonly IdleState IdleState;
    
    public readonly WalkState WalkState;
    
    public readonly DashState DashState;

    public readonly RunState RunState;

    public readonly TurnAroundState TurnAroundState;

    public readonly FallState FallState;

    public readonly JumpState JumpState;
    
    public readonly LandState LandState;

    public readonly AirDodgeState AirDodgeState;

    public readonly AttackState AttackState;

    #endregion

    public CharacterStateMachine(ICharacterManager characterManager)
    {
        this.characterManager = characterManager;

        IdleState = new IdleState(this.characterManager, this, ColorUtils.HexToColor("FFFFFF"));
        WalkState = new WalkState(this.characterManager, this, ColorUtils.HexToColor("00BE2C"));
        DashState = new DashState(this.characterManager, this, ColorUtils.HexToColor("FF0000"));
        RunState = new RunState(this.characterManager, this, ColorUtils.HexToColor("00FF00"));
        TurnAroundState = new TurnAroundState(this.characterManager, this, ColorUtils.HexToColor("d60466"));
        FallState = new FallState(this.characterManager, this, ColorUtils.HexToColor("c4c101"));
        JumpState = new JumpState(this.characterManager, this, ColorUtils.HexToColor("FFFF00"));
        LandState = new LandState(this.characterManager, this, ColorUtils.HexToColor("00FFFF"));
        AirDodgeState = new AirDodgeState(this.characterManager, this, ColorUtils.HexToColor("00A8BF"));
        AttackState = new AttackState(this.characterManager, this, ColorUtils.HexToColor("A800BF"));

        Initialize(IdleState);
    }

    public void Initialize(BaseState state, Dictionary<string, object> parameters = null)
    {
        CurrentState = state;
        CurrentState.Enter(parameters);
    }

    public void ChangeState(BaseState state, Dictionary<string, object> parameters = null)
    {
        Debug.Log($"Changed state from {CurrentState.GetType()} to {state.GetType()}");
        CurrentState.Exit();
        CurrentState = state;

        CurrentState.Enter(parameters);
    }
}
