
using Data;

namespace Simulation
{
    public class IdleState : BaseState
    {

        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (CheckIfFalling(ref character, input))
                return;

            if (CheckIfJumping(ref character, input))
                return;

            // Check for dash

            if (FixedMath.Abs(input.Movement.x) > 0.1f)
            {
                character.CurrentState = CharacterStateType.Walk;
                return;
            }

            // Check for attack
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            character.DynamicBody.Velocity.x.Decelerate(character.Stats.Traction);
        }
    }
}