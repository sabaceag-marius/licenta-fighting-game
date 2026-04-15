
using Data;
using UnityEngine;

namespace Simulation
{
    public class RunState : BaseState
    {
        public override void HandleLogic(ref CharacterData character, ProcessedInput input)
        {
            if (CheckIfFalling(ref character, input))
                return;

            if (CheckIfJumping(ref character, input))
                return;

            if (character.FacingDirection * input.Movement.x < 0)
            {
                character.CurrentState = CharacterStateType.TurnAround;
                return;
            }

            if (FixedMath.Abs(character.Velocity.x) < 0.1f)
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

            // We decrease the speed if the movement stick is neutral (as opposed to moving to IdleState)
            // or if we go beyond the maximum running speed

            if (FixedMath.Abs(input.Movement.x) < 0.1f || FixedMath.Abs(velocity.x) > character.Stats.RunningSpeed)
            {
                velocity.x.Decelerate(character.Stats.Traction);
            }
            else
            {
                velocity.x.Accelerate(character.Stats.RunningSpeed * character.FacingDirection, character.Stats.DashAcceleration);
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