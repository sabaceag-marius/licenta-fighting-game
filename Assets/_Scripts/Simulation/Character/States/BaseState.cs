
using Data.Character;
using UnityEngine;

namespace Simulation
{
    public abstract class BaseState : ICharacterState
    {
        public virtual void Enter(ref CharacterData character, ProcessedInput input, Data.Combat.AttackData[] characterAttacks) 
        {
            character.StateFrame = 0;
            character.StateChanged = false;
        }

        public virtual void Exit(ref CharacterData character) { }

        public virtual void HandlePrePhysicsLogic(ref CharacterData character, ProcessedInput input) { }

        public virtual void HandlePhysics(ref CharacterData character, ProcessedInput input)
        {
            if (character.DynamicBody.IsGrounded)
            {
                character.DynamicBody.Velocity.y -= 0.01f;
            }
            else
            {
                character.DynamicBody.Velocity.y.Accelerate(-character.Stats.FallSpeed, character.Stats.Gravity);
            }
        }

        public virtual void HandlePostPhysicsLogic(ref CharacterData character, ProcessedInput input) { }

        public void Execute(ref CharacterData character, ProcessedInput input, LogicCollider[] staticColliders, FixedFloat minimumSafeStepX, FixedFloat minimumSafeStepY)
        {
            HandlePrePhysicsLogic(ref character, input);
            
            HandlePhysics(ref character, input);

            // add physics engine
            PhysicsEngine.SimulateCharacterPhysics(ref character, staticColliders, minimumSafeStepX, minimumSafeStepY);
            
            HandlePostPhysicsLogic(ref character, input);
        }

        public virtual void ExecuteDuringHitstop(ref CharacterData character, ProcessedInput input) { }

        /// <summary>
        /// Check if we should change the character's state to Jump
        /// </summary>
        /// <param name="character"></param>
        /// <param name="input"></param>
        /// <returns>true if the state changed, false otherwise</returns>
        protected virtual bool CheckIfJumping(ref CharacterData character, ProcessedInput input)
        {
            if (!input.JumpPressed)
                return false;

            character.CurrentState = CharacterStateType.Jump;

            return true;
        }

        /// <summary>
        /// Check if we should change the character's state to Fall
        /// </summary>
        /// <param name="character"></param>
        /// <param name="input"></param>
        /// <returns>true if the state changed, false otherwise</returns>
        protected bool CheckIfFalling(ref CharacterData character, ProcessedInput input)
        {
            if (character.DynamicBody.IsGrounded)
                return false;

            character.CurrentState = CharacterStateType.Fall;

            return true;
        }

        /// <summary>
        /// Check if we should change the character's state to Attack
        /// </summary>
        /// <param name="character"></param>
        /// <param name="input"></param>
        /// <returns>true if the state changed, false otherwise</returns>
        protected virtual bool CheckIfAttacking(ref CharacterData character, ProcessedInput input)
        {
            if (!input.AttackPressed)
                return false;

            character.CurrentState = CharacterStateType.Attack;

            return true;
        }

        // <summary>
        /// Check if we should change the character's state to Attack
        /// </summary>
        /// <param name="character"></param>
        /// <param name="input"></param>
        /// <returns>true if the state changed, false otherwise</returns>
        protected virtual bool CheckIfAirDodging(ref CharacterData character, ProcessedInput input)
        {
            if (!input.DodgePressed)
                return false;

            if (character.RemainingAirDodges <= 0)
                return false;

            if (character.AirDodgeCooldown > 0)
                return false;

            character.CurrentState = CharacterStateType.AirDodge;

            return true;
        }

        protected void FlipCharacter(ref CharacterData character, int direction)
        {
            character.FacingDirection = direction;
        }

        protected void HandlePlatformCollision(ref CharacterData character, ProcessedInput input)
        {
            // Fall through the platform if:
            // You are grounded on the platform and you flick down (Idle and Walk)
            // or if you are falling above the platform and hold down
             
            if ((character.DynamicBody.IsGrounded && input.FlickDirection.y == -1) ||
                (!character.DynamicBody.IsGrounded && character.DynamicBody.Velocity.y < 0 && input.Movement.y <= -0.5))
            {
                character.IgnorePlatformCollisionFrames = Simulation.Character.GlobalCharacterStats.IgnorePlatformCollisionFrames;
            }
        }
    }
}