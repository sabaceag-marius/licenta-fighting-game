
using Data;
using UnityEngine;

namespace Simulation
{
    public class FallState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);

            character.IsFastFalling = false;
        }

        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (input.DodgePressed)
            {
                character.CurrentState = CharacterStateType.AirDodge;
                return;
            }

            HandlePlatformCollision(ref character, input);

            if (character.RemainingAirJumps > 0 && CheckIfJumping(ref character, input))
            {
                character.RemainingAirJumps--;
                return;
            }

            if (input.FastFalled)
            {
                character.IsFastFalling = true;
            }

            if (character.DynamicBody.IsGrounded)
            {
                character.CurrentState = CharacterStateType.Land;
            }

            if (CheckIfAttacking(ref character, input))
                return;
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

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

        protected override bool CheckIfAttacking(ref CharacterData character, ProcessedInput input)
        {
            if (!base.CheckIfAttacking(ref character, input))
                return false;
            
            character.CurrentState = CharacterStateType.Attack;
            character.AttackType = Data.Combat.AttackType.AirNeutral;

            return true;
        }
    }
}