
using Data;
using UnityEngine;

namespace Simulation
{
    public class HitState : BaseState
    {
        public override void Exit(ref CharacterData character)
        {
            character.RemainingAirDodges = character.Stats.AirDodgesCount;
            character.AirDodgeCooldown = 0;
        }

        public override void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            if (character.DynamicBody.IsGrounded)
            {
                //TODO: remove hitstun frames?
                character.CurrentState = CharacterStateType.Land;
                return;
            }

            // Check if hitstun expired -> transition into fall

            if (character.HitstunFrames == 0)
            {
                character.CurrentState = CharacterStateType.Fall;
            }
        }
    }
}