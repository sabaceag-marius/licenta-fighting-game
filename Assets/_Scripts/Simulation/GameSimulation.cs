
namespace Simulation
{
    public class GameSimulation
    {
        private Data.Combat.AttackData[][] attackDatabase;
        private CharacterStateMachine characterStateMachine;
        private CharacterInputProcessor inputProcessor;

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

        public void AdvanceFrame(ref GameState gameState, GameState previousGameState)
        {
            for (int i = 0; i < gameState.CharactersCount; i++)
            {
                ref Data.CharacterData character = ref gameState.Characters[i];

                // Process input
                var input = inputProcessor.ProcessInput(character.RawInput, previousGameState.Characters[i].RawInput);

                // Handle logic and velocity based on the state
                characterStateMachine.AdvanceFrame(ref character, input, attackDatabase[i]);

                // Run physics
                PhysicsEngine.ApplyVelocity(ref character.DynamicBody);
            }

            // Check for all collisions for the colliders from the gameState in the PhysicsEngine

            // Check collisions between the dynamic bodies colliders and the static colliders
            PhysicsEngine.HandleGameStateCollisions(ref gameState);

            // Check for hitbox - hurtbox collision
            CombatEngine.ProcessAttacks(ref gameState, attackDatabase);
        }
    }
}