
using System;

namespace Simulation
{
    public class GameSimulation
    {
        private Data.Combat.AttackData[][] attackDatabase;
        
        private CharacterStateMachine characterStateMachine;
        
        private CharacterInputProcessor inputProcessor;

        private LogicBox blastzoneBoundingBox;        
        
        public GameSimulation()
        {
            characterStateMachine = new CharacterStateMachine();
            inputProcessor = new CharacterInputProcessor();
        }

        public void InitializeAttackData(Data.Combat.AttackData[][] attacks)
        {
            attackDatabase = attacks;
        }

        public Data.Combat.AttackData GetCharacterAttack(int characterIndex, Data.Combat.AttackType attackType)
        {
            return attackDatabase[characterIndex][(int)attackType];
        }

        /*
            The order of operations that happens when simulating a frame:

            - For each character:
                - process the input

                - CharacterStateMachine - logic and physics based on the character's state
                    
                    1. Pre-Physics Logic - processes inputs and timers

                    2. Physics Request - based on the logic, we set the velocity and then call the physics engine

                    3. Pos-Physics Logic - Logic happening after physics, based on the collisions (Tech, Land, etc.)

            - CombatEngine - check across all characters if there are any collisions between their hitboxes and hurtboxes + Transition handling
            
            - Check for each character if they left the blastzone
        */
        public void AdvanceFrame(ref GameState gameState, GameState previousGameState)
        {
            for (int i = 0; i < gameState.CharactersCount; i++)
            {
                ref Data.CharacterData character = ref gameState.Characters[i];

                // Process input
                var input = inputProcessor.ProcessInput(character.RawInput, previousGameState.Characters[i].RawInput);

                // Handle logic and velocity based on the state
                characterStateMachine.AdvanceFrame(ref character, input, attackDatabase[i], gameState.StaticColliders);

                // Run physics
                // PhysicsEngine.ApplyVelocity(ref character.DynamicBody);

                GameRulesEngine.CheckBlastZone(ref character, blastzoneBoundingBox);
            }
            
            // Check for hitbox - hurtbox collision
            CombatEngine.ProcessAttacks(ref gameState, attackDatabase);

            for (int i = 0; i < gameState.CharactersCount; i++)
            {
                ref Data.CharacterData character = ref gameState.Characters[i];

                GameRulesEngine.CheckBlastZone(ref character, blastzoneBoundingBox);
            }
            
            // for (int i = 0; i < gameState.CharactersCount; i++)
            // {
            //     ref Data.CharacterData character = ref gameState.Characters[i];

            //     // Process input
            //     var input = inputProcessor.ProcessInput(character.RawInput, previousGameState.Characters[i].RawInput);

            //     // Handle logic and velocity based on the state
            //     characterStateMachine.AdvanceFrame(ref character, input, attackDatabase[i]);

            //     // Run physics
            //     PhysicsEngine.ApplyVelocity(ref character.DynamicBody);

            //     GameRulesEngine.CheckBlastZone(ref character, blastzoneBoundingBox);
            // }

            // // Check for all collisions for the colliders from the gameState in the PhysicsEngine

            // // Check collisions between the dynamic bodies colliders and the static colliders
            // PhysicsEngine.HandleGameStateCollisions(ref gameState);

            // // Check for hitbox - hurtbox collision
            // CombatEngine.ProcessAttacks(ref gameState, attackDatabase);
        }

        public void SetBlastzone(LogicBox collider)
        {
            blastzoneBoundingBox = collider;
        }
    }
}