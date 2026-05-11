
using Data.Character;

namespace Simulation
{
    public class AirDodgeState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);

            character.AirDodgeDirection = input.Movement.Normalize();

            character.Velocity = character.AirDodgeDirection * Simulation.Character.GlobalCharacterStats.AirDodgePower;

            character.Hurtboxes[0].State = Data.Combat.HurtboxState.Intangible;
        }

        public override void Exit(ref CharacterData character)
        {
            base.Exit(ref character);

            character.RemainingAirDodges--;
            character.AirDodgeCooldown = Simulation.Character.GlobalCharacterStats.AirDodgeCooldownFrames;

            character.Hurtboxes[0].State = Data.Combat.HurtboxState.Normal;
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            //base.HandlePhysics(ref character, input);

            FixedVector2 velocity = character.Velocity;

            velocity.x.Decelerate(Simulation.Character.GlobalCharacterStats.AirDodgeTraction);
            velocity.y.Decelerate(Simulation.Character.GlobalCharacterStats.AirDodgeTraction);

            character.Velocity = velocity;
        }

        public override void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            if (character.DynamicBody.IsGrounded)
            {
                character.Velocity = character.AirDodgeDirection * Simulation.Character.GlobalCharacterStats.AirDodgePower;

                character.CurrentState = CharacterStateType.Idle;
                return;
            }

            if (character.StateFrame == Simulation.Character.GlobalCharacterStats.AirDodgeFrames)
            {
                character.CurrentState = CharacterStateType.Fall;
            }
        }
    }
}