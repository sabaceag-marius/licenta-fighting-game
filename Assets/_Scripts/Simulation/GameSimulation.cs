
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Simulation
{
    public class GameSimulation
    {
        private CharacterStateMachine characterStateMachine;
        private CharacterInputProcessor inputProcessor;

        public GameSimulation()
        {
            characterStateMachine = new CharacterStateMachine();
            inputProcessor = new CharacterInputProcessor();
        }

        public void AdvanceFrame(ref GameState gameState, GameState previousGameState)
        {
            for (int i = 0; i < gameState.CharactersCount; i++)
            {
                ref Data.CharacterData character = ref gameState.Characters[i];

                // Process input
                //TODO: take into consideration previous inputFrame
                var input = inputProcessor.ProcessInput(character.RawInput, previousGameState.Characters[i].RawInput);

                // Handle logic and velocity based on the state
                characterStateMachine.AdvanceFrame(ref character, input);

                // Run physics
                PhysicsEngine.ApplyVelocity(ref character.DynamicBody);
            }

            // Check for all collisions for the colliders from the gameState in the PhysicsEngine

            // TODO: Separate this from the hitbox hurtbox collision when implementing combat

            // Check collisions between the dynamic bodies colliders and the static colliders
            PhysicsEngine.HandleGameStateCollisions(ref gameState);
        }
    }
}