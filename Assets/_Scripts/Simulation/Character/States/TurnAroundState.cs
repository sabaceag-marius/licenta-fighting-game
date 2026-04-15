
using Data;

namespace Simulation
{
    public class TurnAroundState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);

            //TODO: move the flip to Exit when adding animations ?

            FlipCharacter(ref character, (int)FixedMath.Sign(input.Movement.x));
        }

        public override void Exit(ref CharacterData character)
        {
            base.Exit(ref character);
        }

        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (CheckIfFalling(ref character, input))
                return;

            if (CheckIfJumping(ref character, input))
                return;

            if (FixedMath.Abs(character.Velocity.x) >= 0.1f)
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

            character.DynamicBody.Velocity.x.Decelerate(character.Stats.Traction);
        }
    }
}