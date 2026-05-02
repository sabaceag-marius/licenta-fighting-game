
using Data;
using UnityEngine;

namespace Simulation
{
    public class TumbleState : BaseState
    {
        public override void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks)
        {
            base.Enter(ref character, input, characterAttacks);

            Debug.Log("Entered tumble state!");
        }

        public override void Exit(ref CharacterData character)
        {
            character.TechPenaltyFrames = 0;
        }

        public override void ExecuteDuringHitstop(ref CharacterData character, ProcessedInput input)
        {
            CheckForTechInput(ref character, input);
        }

        public override void HandlePrePhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            if (character.HitstunFrames == 0)
                return;

            CheckForTechInput(ref character, input);
        }

        public override void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            base.HandlePhysics(ref character, input);

            if (character.HitstunFrames > 0)
                return;

            FixedVector2 velocity = character.Velocity;
            // Horizontal movement

            // No movement or
            // Reached maximum speed

            if (FixedMath.Abs(input.Movement.x) < 0.1f ||
                FixedMath.Abs(character.Velocity.x) > character.Stats.AirSpeed)
            {
                velocity.x.Decelerate(character.Stats.AirFriction);
            }
            else
            {
                // We cannot turn back while falling, so we use just the direction of the 
                // movement to determine in which direction to move

                FixedFloat direction = FixedMath.Sign(input.Movement.x);

                velocity.x.Accelerate(character.Stats.AirSpeed * direction, character.Stats.AirAcceleration);
            }

            character.Velocity = velocity;
        }

        public override void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input)
        {
            if (CheckForTech(ref character, input))
            {
                Debug.Log("Teched!");

                character.HitstunFrames = 0;
                return;
            }

            //TODO: Add the bounce back in case of spikes / attacks that land you on the ground
            if (character.DynamicBody.IsGrounded)
            {
                //TODO: remove hitstun frames?
                character.HitstunFrames = 0;
                character.CurrentState = CharacterStateType.Land;
                return;
            }

            if (character.HitstunFrames > 0)
                return;

            if (CheckIfAirDodging(ref character, input))
                return;

            if (CheckIfJumping(ref character, input))
                return;

            if (CheckIfAttacking(ref character, input))
                return;
        }

        private bool CheckForTech(ref CharacterData character, ProcessedInput input)
        {
            ref LogicDynamicBody body = ref character.DynamicBody;

            if (body.HitWall)
            {
                if (character.TechWindowFrames > 0)
                {
                    character.TechWindowFrames = 0;
                    character.TechPenaltyFrames = 0;

                    body.ExternalVelocity = FixedVector2.zero;
                    body.Velocity = FixedVector2.zero;

                    character.CurrentState = CharacterStateType.Fall;

                    return true;
                }
                else
                {
                    character.HitstopFrames = 2;

                    // Calculate bounce and manually inject it back into the body
                    body.ExternalVelocity.x = -body.ExternalVelocityAtImpact.x * 0.75f;
                }
            }

            if (body.HitCeiling)
            {
                if (character.TechWindowFrames > 0)
                {
                    character.TechWindowFrames = 0;
                    character.TechPenaltyFrames = 0;

                    body.ExternalVelocity = FixedVector2.zero;
                    body.Velocity = FixedVector2.zero;
                    
                    character.CurrentState = CharacterStateType.Fall;

                    return true;
                }
                else
                {
                    character.HitstopFrames = 2;

                    // Calculate bounce and manually inject it back into the body
                    body.ExternalVelocity.y = -body.ExternalVelocityAtImpact.y * 0.75f;
                }
            }

            return false;
        }

        protected override bool CheckIfAttacking(ref CharacterData character, ProcessedInput input)
        {
            if (!base.CheckIfAttacking(ref character, input))
                return false;
            
            character.CurrentState = CharacterStateType.Attack;
            character.AttackType = Data.Combat.AttackType.AirNeutral;

            return true;
        }

        protected override bool CheckIfJumping(ref CharacterData character, ProcessedInput input)
        {
            if (character.RemainingAirJumps <= 0)
                return false;

            if (!base.CheckIfJumping(ref character, input))
                return false;

            character.RemainingAirJumps--;

            return true;
        }

        private void CheckForTechInput(ref CharacterData character, ProcessedInput input)
        {
            if (input.DodgePressed && character.TechPenaltyFrames == 0)
            {
                character.TechWindowFrames = 20;
                character.TechPenaltyFrames = 40;
            }
        }
    }
}