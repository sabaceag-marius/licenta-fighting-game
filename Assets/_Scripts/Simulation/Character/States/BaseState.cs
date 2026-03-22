
using Data;
using UnityEngine;

namespace Simulation
{
    public abstract class BaseState : ICharacterState
    {
        public virtual void Enter(ref CharacterData character, ProcessedInput input) 
        {
            character.StateFrame = 0;
        }

        public virtual void Exit(ref CharacterData character) { }

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

        public abstract void HandleLogic(ref CharacterData character, ProcessedInput input);

        public void Execute(ref CharacterData character, ProcessedInput input)
        {
            HandlePhysics(ref character, input);

            HandleLogic(ref character, input);
        }

        /// <summary>
        /// Check if we should change the character's state to Jump
        /// </summary>
        /// <param name="character"></param>
        /// <param name="input"></param>
        /// <returns>true if the state changed, false otherwise</returns>
        protected bool CheckIfJumping(ref CharacterData character, ProcessedInput input)
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

        protected void FlipCharacter(ref CharacterData character, int direction)
        {
            character.FacingDirection = direction;
        }
    }
}