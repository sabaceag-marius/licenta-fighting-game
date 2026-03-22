
using Data;

namespace Simulation
{
    public class DashState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input)
        {
            base.Enter(ref character, input);

            if (character.FacingDirection * input.Movement.x < 0)
            {
                FlipCharacter(ref character, (int)FixedMath.Sign(input.Movement.x));
            }

            FixedVector2 velocity = character.Velocity;

            velocity.x = character.FacingDirection * character.Stats.InitialDashSpeed;

            character.Velocity = velocity;
        }

        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (CheckIfFalling(ref character, input))
                return;

            if (CheckIfJumping(ref character, input))
                return;

            // Dash dance

            // Minimum dash time
            if (character.StateFrame < character.Stats.DashFrames)
                return;

            if (FixedMath.Abs(input.Movement.x) >= 0.1f)
            {
                character.CurrentState = CharacterStateType.Run;
            }
            else
            {
                character.CurrentState = CharacterStateType.Idle;
            }
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            FixedVector2 velocity = character.Velocity;

            velocity.x.Accelerate(character.Stats.RunningSpeed * character.FacingDirection, 
                character.Stats.DashAcceleration);

            character.Velocity = velocity;
        }
    }
}