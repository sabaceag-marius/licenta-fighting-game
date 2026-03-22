
using Data;

namespace Simulation
{
    public class LandState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input)
        {
            base.Enter(ref character, input);

            character.RemainingAirJumps = character.Stats.AirJumpCount;
        }

        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (character.StateFrame == character.Stats.LandLagFrames)
            {
                character.CurrentState = CharacterStateType.Idle;
            }
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            character.DynamicBody.Velocity.x.Decelerate(character.Stats.Traction);
        }
    }
}