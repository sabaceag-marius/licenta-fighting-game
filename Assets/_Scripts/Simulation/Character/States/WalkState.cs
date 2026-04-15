
using Data;

namespace Simulation
{
    public class WalkState : BaseState
    {
        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (CheckIfFalling(ref character, input))
                return;

            if (CheckIfJumping(ref character, input))
                return;

            if (input.Dashed)
            {
                character.CurrentState = CharacterStateType.Dash;
                return;
            }

            if (FixedMath.Abs(input.Movement.x) < 0.1)
            {
                character.CurrentState = CharacterStateType.Idle;
                return;
            }

            if (CheckIfAttacking(ref character, input))
                return;
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            FixedVector2 velocity = character.Velocity;

            if (FixedMath.Abs(velocity.x) > character.Stats.WalkSpeed)
            {
                velocity.x.Decelerate(character.Stats.Traction);
            }
            else
            {
                FixedFloat directionXAmount = FixedMath.Abs(input.Movement.x);

                // We want to have different speeds based on how far away the movement stick is
                FixedFloat walkSpeedModifier =
                    directionXAmount < 0.625f ? 0.5f :
                    directionXAmount < 0.875f ? 0.75f :
                    1f;

                velocity.x = character.Stats.WalkSpeed * walkSpeedModifier * character.FacingDirection;
            }
            
            if (character.FacingDirection * input.Movement.x < 0)
            {
                FlipCharacter(ref character, (int)FixedMath.Sign(input.Movement.x));
            }

            character.Velocity = velocity;
        }

        protected override bool CheckIfAttacking(ref CharacterData character, ProcessedInput input)
        {
            if (!base.CheckIfAttacking(ref character, input))
                return false;
            
            character.CurrentState = CharacterStateType.Attack;
            character.AttackType = Data.Combat.AttackType.GroundForward;

            return true;
        }
    }
}