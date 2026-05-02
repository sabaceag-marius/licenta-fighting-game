
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

        public override void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            ref LogicDynamicBody body = ref character.DynamicBody;

            if (body.HitWall)
            {
                // We use the recorded impact velocity because the current velocity is 0!
                if (FixedMath.Abs(body.ExternalVelocityAtImpact.x) > 3)
                {
                    // Calculate bounce and manually inject it back into the body
                    body.ExternalVelocity.x = -body.ExternalVelocityAtImpact.x * 0.75f;
                }
            }

            if (body.HitCeiling)
            {
                // We use the recorded impact velocity because the current velocity is 0!
                if (FixedMath.Abs(body.ExternalVelocityAtImpact.y) > 3)
                {
                    // Calculate bounce and manually inject it back into the body
                    body.ExternalVelocity.y = -body.ExternalVelocityAtImpact.y * 0.75f;
                }
            }

            if (character.DynamicBody.IsGrounded)
            {
                //TODO: remove hitstun frames?
                character.CurrentState = CharacterStateType.Land;
            }

            // Check if hitstun expired -> transition into fall

            if (character.HitstunFrames == 0)
            {
                character.CurrentState = CharacterStateType.Fall;
            }
        }
    }
}