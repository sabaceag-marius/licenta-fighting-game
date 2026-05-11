
using Data.Character;

namespace Simulation
{
    public class LandState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);

            character.RemainingAirJumps = character.Stats.AirJumpCount;
            character.RemainingAirDodges = Simulation.Character.GlobalCharacterStats.AirDodgesCount;
            character.AirDodgeCooldown = 0;
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            character.DynamicBody.Velocity.x.Decelerate(character.Stats.Traction);
        }

        public override void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            if (character.StateFrame == Simulation.Character.GlobalCharacterStats.LandLagFrames)
            {
                character.CurrentState = CharacterStateType.Idle;
            }
        }
    }
}