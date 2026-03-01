using System;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : BaseState
{
    public const string Param_AttackType = "AttackType";

    [FromParameter(Param_AttackType)]
    private AttackType attackType;

    private IAttackController attackController;

    private AttackDataSO attackData;

    private int currentFrame = 0;

    private bool isFastFalling;

    public AttackState(ICharacterManager characterManager, CharacterStateMachine stateMachine, Color color) : base(characterManager, stateMachine, color)
    {
        attackController = characterManager.GetGameObjectComponent<IAttackController>();
    }

    public override void Enter(Dictionary<string, object> parameters = null)
    {
        base.Enter(parameters);
        
        Debug.Log($"Performing attack {attackType}");

        attackData = characterManager.GetAttack(attackType);

        if (attackData == null)
        {
            Debug.LogWarning($"The character does not have assigned an attack of type {attackType}");

            stateMachine.ChangeState(characterManager.IsGrounded ? stateMachine.IdleState : stateMachine.FallState);

            return;
        }

        currentFrame = 0;
    }

    public override void Exit()
    {
        base.Exit();

        attackController.ClearHitTargets();
    }

    public override void HandleLogic()
    {
        base.HandleLogic();

        if (attackData == null)
            return;

        if (currentFrame >= attackData.TotalDurationFrames)
        {
            stateMachine.ChangeState(characterManager.IsGrounded ? stateMachine.IdleState : stateMachine.FallState);
        }

        AttackFrame frameData = attackData.Frames.Find(f => f.frameIndex == currentFrame);

        if (frameData != null && frameData.Hitboxes.Count > 0)
        {
            attackController.GenerateHitboxes(frameData.Hitboxes);
        }

        currentFrame++;

        if (attackData.IsAerialAttack)
        {
            HandleAerialAttackLogic();
        }

        //TODO: Add if we move a bit formard while attacking?
        //if (CheckIfFalling())
        //    return;
    }

    public override void HandlePhysics()
    {
        base.HandlePhysics();

        if (!attackData.IsAerialAttack)
            return;

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
            velocity.y = -characterManager.Stats.FastFallSpeed;
        }

        characterManager.Velocity = velocity;
    }

    private void HandleAerialAttackLogic()
    {
        if (characterManager.Input.FastFalled)
        {
            isFastFalling = true;
        }

        if (characterManager.IsGrounded)
        {
            stateMachine.ChangeState(stateMachine.LandState);
        }
    }
}