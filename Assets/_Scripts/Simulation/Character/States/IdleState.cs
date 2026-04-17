
using Data;

namespace Simulation
{
    public class IdleState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);

            character.RemainingAirJumps = character.Stats.AirJumpCount;
            character.RemainingAirDodges = character.Stats.AirDodgesCount;
            character.AirDodgeCooldown = 0;
        }

        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            HandlePlatformCollision(ref character, input);

            if (CheckIfFalling(ref character, input))
                return;

            if (CheckIfJumping(ref character, input))
                return;

            // Check for dash

            if (input.Dashed)
            {
                character.CurrentState = CharacterStateType.Dash;
                return;
            }

            if (FixedMath.Abs(input.Movement.x) >= 0.1f)
            {
                character.CurrentState = CharacterStateType.Walk;
                return;
            }

            if (CheckIfAttacking(ref character, input))
                return;
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            character.DynamicBody.Velocity.x.Decelerate(character.Stats.Traction);
        }

        protected override bool CheckIfAttacking(ref CharacterData character, ProcessedInput input)
        {
            if (!base.CheckIfAttacking(ref character, input))
                return false;
            
            character.CurrentState = CharacterStateType.Attack;
            character.AttackType = Data.Combat.AttackType.GroundNeutral;

            return true;
        }
    }
}