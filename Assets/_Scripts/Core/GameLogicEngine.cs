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
        public AttackData[][] Attacks { get; private set; }

        public ushort CurrentTick { get; private set; }
        public long TotalMatchFrames { get; private set; }
        public bool MatchEnded { get; private set; }
        public FixedFloat FixedDeltaTime { get; private set; }

        #region Rollback Sync

        public int LastReceivedExecutionFrame { get; private set; } = -1;

        public int RemoteRawAdvantage { get; private set; } = 0;

        public int OldestDesyncFrame { get; private set; } = -1;

        #endregion

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

        public void RunSingleTick(RawInput[] currentHardwareInputs, bool isRollback = false)
        {
            int previousIndex = CurrentTick % Config.BufferSize;
            int currentIndex = (CurrentTick + 1) % Config.BufferSize;

            ref GameState previousState = ref StateBuffer[previousIndex];
            ref GameState currentState = ref StateBuffer[currentIndex];

            CopyGameStateData(previousState, ref currentState);

            currentState.FrameNumber++;

            for (int i = 0; i < currentState.Characters.Length; i++)
            {
                int bufferIndex = CurrentTick % Config.BufferSize;

                if (!isRollback)
                {
                    // Local input
                    if (currentHardwareInputs[i].IsConfirmed)
                    {
                        InputBuffer[i][bufferIndex] = currentHardwareInputs[i];
                    }
                    else 
                    {
                        // We do not have input for this character yet, so we try to predict it:
                        // Duplicate the input from the previous frame

                        int prevBufferIndex = (CurrentTick - 1 + Config.BufferSize) % Config.BufferSize;
                
                        RawInput predictedInput = InputBuffer[i][prevBufferIndex];
                        predictedInput.FrameId++; // Increment the frame to the next one
                        predictedInput.IsConfirmed = false;   // Explicitly mark it as a guess
                        
                        InputBuffer[i][bufferIndex] = predictedInput;
                    }    
                }

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

        public void ProcessRollback()
        {
            // No desyncs detected from the received network packet
            if (OldestDesyncFrame == -1)
                return;

            int targetTick = CurrentTick;

            // Move back before the desynchronization
            CurrentTick = (ushort) OldestDesyncFrame;

            // Re-simulate the physics back to the present

            while (CurrentTick < targetTick)
            {
                // We pass null for the input array, as we do not use it during rollback
                RunSingleTick(null, isRollback: true);    
            }

            // Reset the desync flag
            OldestDesyncFrame = -1;
        }

        public GameState GetCurrentGameState()
        {
            return StateBuffer[CurrentTick % Config.BufferSize];
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

        public void ReceiveNetworkPacket(NetworkPacket packet)
        {
            byte playerId = packet.PlayerId;

            if (packet.LatestExecutionFrame > LastReceivedExecutionFrame)
            {
                LastReceivedExecutionFrame = packet.LatestExecutionFrame;
                RemoteRawAdvantage = packet.RawAdvantage;
            }

            for (int i = 0; i < Core.Networking.NetworkUtils.REDUNDANCY_COUNT; i++)
            {
                RawInput input = packet.Inputs[i];

                // Ignore unitialized frames at the end of the MatchEnded
                if (input.FrameId == 0 && input.IsEmpty)
                    continue;
                
                // Ignore inputs that are out of the ring buffer
                if (input.FrameId <= CurrentTick - Config.BufferSize)
                    continue;

                int bufferIndex = input.FrameId % Config.BufferSize;
                RawInput existingInput = InputBuffer[playerId][bufferIndex];

                // Check if the input is already confirmed

                if (existingInput.IsConfirmed)
                    continue;

                // We predicted the wrong input
                if (input.Buttons != existingInput.Buttons 
                    || input.LeftStickX != existingInput.LeftStickX
                    || input.LeftStickY != existingInput.LeftStickY
                    || input.RightStick != existingInput.RightStick)
                {
                    // Overwrite with the correct input
                    InputBuffer[playerId][bufferIndex] = input;

                    int executedTick = CurrentTick - Config.InputDelay;

                    // Do not mark for rollback if the frame arrived earlier!!

                    if (input.FrameId <= executedTick && (OldestDesyncFrame == -1 || input.FrameId < OldestDesyncFrame))
                    {
                        // Mark the frame we have to rollback from
                        OldestDesyncFrame = input.FrameId;
                    }
                }

                // Confirm the input
                InputBuffer[playerId][bufferIndex].IsConfirmed = true;
            }    
        }

        public int GetLocalRawAdvantage(int executionFrame)
        {
            return executionFrame - LastReceivedExecutionFrame;
        }

        public int GetTrueFrameAdvantage(int executionFrame)
        {
            int localRawAdvantage = GetLocalRawAdvantage(executionFrame);

            // GGPO formula: (Local Advantage - Remote Advantage) / 2
            return FixedMath.RoundToInt((localRawAdvantage - RemoteRawAdvantage) / 2f);
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
    }
}