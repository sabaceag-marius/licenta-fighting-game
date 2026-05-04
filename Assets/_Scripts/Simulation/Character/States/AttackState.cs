
using System;
using Data.Character;

namespace Simulation
{
    public class AttackState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);

            character.CurrentAttackFrame = 0;
            character.AttackDurationCount = characterAttacks[(int)character.AttackType].TotalDurationFrames;
            character.AttackFrameCount = characterAttacks[(int)character.AttackType].Frames.Length;

            character.IsFastFalling = false;

            character.HitTargetsMask = 0;
        }

        public override void Exit(ref CharacterData character)
        {
            base.Exit(ref character);

        }

        public override void HandlePrePhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            FixedFloat logicProgress = (FixedFloat)character.StateFrame / (FixedFloat)character.AttackDurationCount;

            int currentAnimationFrame = (int)Math.Floor(logicProgress * character.AttackFrameCount);

            if (character.CurrentAttackFrame != currentAnimationFrame)
            {
                character.CurrentAttackFrame = currentAnimationFrame;
            }

            if (character.IsAerialAttack && input.FastFalled)
            {
                character.IsFastFalling = true;
            }
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            if (!character.IsAerialAttack)
                return;

            FixedVector2 velocity = character.Velocity;

            // Horizontal movement

            // No movement or
            // Reached maximum speed

            if (FixedMath.Abs(input.Movement.x) < 0.1f ||
                FixedMath.Abs(character.Velocity.x) > character.Stats.AirSpeed)
            {
                velocity.x.Decelerate(character.Stats.AirFriction);
            }
            else
            {
                // We cannot turn back while falling, so we use just the direction of the 
                // movement to determine in which direction to move

                FixedFloat direction = FixedMath.Sign(input.Movement.x);

                velocity.x.Accelerate(character.Stats.AirSpeed * direction, character.Stats.AirAcceleration);
            }

             if (character.IsFastFalling)
             {
                 velocity.y = - character.Stats.FastFallSpeed;
             }

            character.Velocity = velocity;
        }

        public override void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            if (character.StateFrame >= character.AttackDurationCount
                || character.AttackFrameCount == 0)
            {
                character.CurrentState = character.DynamicBody.IsGrounded ? CharacterStateType.Idle : CharacterStateType.Fall;
                return;
            }
            
            if (character.IsAerialAttack && character.DynamicBody.IsGrounded)
            {
                character.CurrentState = CharacterStateType.Land;
            }
        }
    }
}