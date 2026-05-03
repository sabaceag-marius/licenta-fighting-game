
using System;

namespace Simulation
{
    public class GameSimulation
    {
        private Data.Combat.AttackData[][] attackDatabase;
        
        private CharacterStateMachine characterStateMachine;
        
        private CharacterInputProcessor inputProcessor;

        private LogicCollider blastzoneBoundingBox;  
        private FixedFloat MinimumStaticColliderExtendsX;
        private FixedFloat MinimumStaticColliderExtendsY;      
        
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
                    
                    1. Pre-Physics Logic - Logic that is not related to changing the state, usually based on the input (fastfall, start tech window, etc.)

                    2. Physics Request - based on the logic, we set the velocity and then call the physics engine

                    3. Pos-Physics Logic - Logic happening after physics, based on the collisions (Tech, Land, etc.) + Transition between states

            - CombatEngine - check across all characters if there are any collisions between their hitboxes and hurtboxes
            
            - Check for each character if they left the blastzone
        */
        public void AdvanceFrame(ref GameState gameState, GameState previousGameState)
        {
            for (int i = 0; i < gameState.Characters.Length; i++)
            {
                ref Data.CharacterData character = ref gameState.Characters[i];

                // Process input
                var input = inputProcessor.ProcessInput(character.RawInput, previousGameState.Characters[i].RawInput);

                // Handle logic and velocity based on the state
                characterStateMachine.AdvanceFrame(ref character, input, attackDatabase[i], gameState.StaticColliders, MinimumStaticColliderExtendsX, MinimumStaticColliderExtendsY);
            }
            
            // Check for hitbox - hurtbox collision
            CombatEngine.ProcessAttacks(ref gameState, attackDatabase);

            // Check if any character left the blastzone
            for (int i = 0; i < gameState.Characters.Length; i++)
            {
                ref Data.CharacterData character = ref gameState.Characters[i];

                GameRulesEngine.CheckBlastZone(ref character, blastzoneBoundingBox);
            }
        }

        public void SetBlastzone(LogicCollider collider)
        {
            blastzoneBoundingBox = collider;
        }

        public void SetMinimumStaticColliderExtends(LogicCollider[] colliders)
        {
            MinimumStaticColliderExtendsX = 0.5f;
            MinimumStaticColliderExtendsY = 0.5f;

            for (int i = 0; i < colliders.Length; i++)
            {
                LogicCollider boundingBox = colliders[i].GetBoundingBox();

                if (boundingBox.Extents.x < MinimumStaticColliderExtendsX)
                {
                    MinimumStaticColliderExtendsX = boundingBox.Extents.x;
                }

                if (boundingBox.Extents.y < MinimumStaticColliderExtendsY)
                {
                    MinimumStaticColliderExtendsY = boundingBox.Extents.y;
                }
            }
        }
    }
}