using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseState
{
    public const string Param_AttackName = "AttackName";

    [FromParameter(Param_AttackName)]
    private string attackName;

    public AttackState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
    }

    public override void Enter(Dictionary<string, object> parameters = null)
    {
        base.Enter(parameters);

        Debug.Log($"Performing attack {attackName}");
    }

    public override void HandleLogic()
    {
        base.HandleLogic();

        if (CheckIfFalling())
            return;

        if (characterManager.IsGrounded)
        {
            stateMachine.ChangeState(stateMachine.LandState);
        }
    }
}