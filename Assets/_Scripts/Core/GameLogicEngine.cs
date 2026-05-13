using Data;
using Data.Combat;
using Simulation;

namespace Core
{
    public class GameLogicEngine
    {
        public SimulationConfig Config { get; private set; }
        public GameState[] StateBuffer { get; private set; }
        public RawInput[][] InputBuffer { get; private set; }
        //TODO: Change to Character Type Enum based-array and do the same thing for CharacterStats
        public AttackData[][] Attacks {get; private set;}
        
        public ushort CurrentTick { get; private set; }
        public long TotalMatchFrames { get; private set; }
        public bool MatchEnded { get; private set; }
        public FixedFloat FixedDeltaTime { get; private set; }

        private GameSimulation gameSimulation;

        public void Initialize(SimulationConfig config, GameState initialState, AttackData[][] attacks, LogicCollider blastzone)
        {
            Config = config;
            FixedDeltaTime = (FixedFloat)1f / config.TargetFPS;
            TotalMatchFrames = (long)(config.MinutesPerMatch * 60 * config.TargetFPS);
            CurrentTick = 0;
            MatchEnded = false;
            Attacks = attacks;

            // Initialize internal simulation
            gameSimulation = new GameSimulation();
            gameSimulation.SetMinimumStaticColliderExtends(initialState.StaticColliders);
            gameSimulation.SetBlastzone(blastzone.GetBoundingBox());

            // Setup State Buffers
            StateBuffer = new GameState[config.BufferSize];
            StateBuffer[0] = initialState;

            for (int i = 1; i < config.BufferSize; i++)
            {
                InitializeGameStateMemory(ref StateBuffer[i], initialState.StaticColliders.Length, initialState.Characters.Length);
            }

            // Setup Input Buffers
            InputBuffer = new RawInput[initialState.Characters.Length][];
            for (int i = 0; i < initialState.Characters.Length; i++)
            {
                InputBuffer[i] = new RawInput[config.BufferSize];
            }
        }

        public void RunSingleTick(RawInput[] currentHardwareInputs)
        {
            int previousIndex = CurrentTick % Config.BufferSize;
            int currentIndex = (CurrentTick + 1) % Config.BufferSize;

            ref GameState previousState = ref StateBuffer[previousIndex];
            ref GameState currentState = ref StateBuffer[currentIndex];

            CopyGameStateData(previousState, ref currentState);

            currentState.FrameNumber++;

            for (int i = 0; i < currentState.Characters.Length; i++)
            {
                InputBuffer[i][CurrentTick % Config.BufferSize] = currentHardwareInputs[i];

                // Apply Input Delay
                RawInput simulationInput = new RawInput();
                int delayedTick = CurrentTick - Config.InputDelay;

                if (delayedTick >= 0)
                {
                    simulationInput = InputBuffer[i][delayedTick % Config.BufferSize];
                }

                currentState.Characters[i].RawInput = simulationInput;
            }

            gameSimulation.AdvanceFrame(ref currentState, previousState, Attacks);

            CurrentTick++;

            CheckMatchEnd(currentState);
        }

        public GameState GetCurrentGameState()
        {
            return StateBuffer[CurrentTick % Config.BufferSize];
        }

        private void CheckMatchEnd(GameState currentState)
        {
            if (CurrentTick >= TotalMatchFrames) 
            {
                MatchEnded = true;
                return;
            }

            for (int i = 0; i < currentState.Characters.Length; i++)
            {
                if (currentState.Characters[i].RemainingStocks == 0)
                {
                    MatchEnded = true;
                    break;
                }
            }
        }

        private static void InitializeGameStateMemory(ref GameState gameState, int staticColliderCount, int characterCount)
        {
            gameState.StaticColliders = new LogicCollider[staticColliderCount];
            gameState.Characters = new Data.Character.CharacterData[characterCount];

            for (int i = 0; i < gameState.Characters.Length; i++)
            {
                gameState.Characters[i].Hurtboxes = new Data.Combat.HurtboxData[1];
            }
        }

        public static void CopyGameStateData(GameState source, ref GameState destination)
        {
            destination.FrameNumber = source.FrameNumber;

            for (int i = 0; i < destination.StaticColliders.Length; i++)
            {
                destination.StaticColliders[i] = source.StaticColliders[i];
            }

            for (int i = 0; i < destination.Characters.Length; i++)
            {
                var preAllocatedHurtboxes = destination.Characters[i].Hurtboxes;
                destination.Characters[i] = source.Characters[i];
                
                destination.Characters[i].Hurtboxes = preAllocatedHurtboxes;
                destination.Characters[i].Hurtboxes[0] = source.Characters[i].Hurtboxes[0];
            }
        }

        public static void DeepCopyGameState(GameState source, ref GameState destination)
        {
            InitializeGameStateMemory(ref destination, source.StaticColliders.Length, source.Characters.Length);

            CopyGameStateData(source, ref destination);

        }
    }
}