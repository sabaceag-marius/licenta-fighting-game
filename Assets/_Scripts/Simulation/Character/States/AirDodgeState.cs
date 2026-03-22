
using Data;

namespace Simulation
{
    public class AirDodgeState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input)
        {
            base.Enter(ref character, input);

            character.AirDodgeDirection = input.Movement.Normalize();

            character.Velocity = character.AirDodgeDirection * character.Stats.AirDodgePower;
        }

        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (character.DynamicBody.IsGrounded)
            {
                character.Velocity = character.AirDodgeDirection * character.Stats.AirDodgePower;

                character.CurrentState = CharacterStateType.Idle;
                return;
            }

            if (character.StateFrame == character.Stats.AirDodgeFrames)
            {
                character.CurrentState = CharacterStateType.Fall;
            }
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            //base.HandlePhysics(ref character, input);

            FixedVector2 velocity = character.Velocity;

            velocity.x.Decelerate(character.Stats.AirDodgeTraction);
            velocity.y.Decelerate(character.Stats.AirDodgeTraction);

            character.Velocity = velocity;
        }
    }
}