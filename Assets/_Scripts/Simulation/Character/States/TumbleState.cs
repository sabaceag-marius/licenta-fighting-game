
using Data;
using UnityEngine;

namespace Simulation
{
    public class TumbleState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);
        }

        public override void HandlePrePhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            // if (character.DynamicBody.IsGrounded)
            // {
            //     //TODO: remove hitstun frames?
            //     character.CurrentState = CharacterStateType.Land;
            // }

            // Check if hitstun expired -> transition into fall

            if (character.HitstunFrames == 0)
            {
                character.CurrentState = CharacterStateType.Fall;
            }
        }
    }
}