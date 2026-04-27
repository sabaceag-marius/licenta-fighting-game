
using Data;

namespace Simulation
{
    public class JumpState : BaseState
    {
        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            // Air jump
            if (!character.DynamicBody.IsGrounded)
            {
                character.DynamicBody.Velocity.y = character.Stats.AirJumpForce;
                return;
            }

            // Grounded jump - We only apply the jump force after the jump squat is finished

            if (character.StateFrame == character.Stats.JumpWindupFrames)
            {
                character.DynamicBody.Velocity.y = input.JumpHeld 
                    ? character.Stats.NormalJumpForce 
                    : character.Stats.ShortJumpForce;
            }
        }

        public override void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            // Swap to fall state after jump squat or instantly if we are midair
            if (!character.DynamicBody.IsGrounded || character.StateFrame == character.Stats.JumpWindupFrames)
            {
                character.CurrentState = CharacterStateType.Fall; 
                return;
            }
        }
    }
}