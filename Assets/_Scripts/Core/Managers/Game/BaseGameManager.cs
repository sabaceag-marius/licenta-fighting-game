
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using Data;
using Data.Combat;
using System.Threading;
using System.Diagnostics;
using UnityEngine.Profiling;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Core
{
    public abstract class BaseGameManager : MonoBehaviour
    {
        [Header("Simulation Configuration")]
        [SerializeField] protected SimulationConfig config = new SimulationConfig { TargetFPS = 60, MinutesPerMatch = 3, InputDelay = 3, BufferSize = 60 };

        [Header("Debug settings")]
        [SerializeField] private bool ShowHitboxes = true;

        protected Core.GameLogicEngine logicEngine;
        protected Character[] characters;
        private CinemachineBrain cameraBrain;

        #region Multi-threading

        private Thread simulationThread;
        private volatile bool isRunning;

        protected RawInput[] threadInput;
        protected object inputLock = new object();

        private GameState renderState, previousRenderState;
        private object stateLock = new object();

        // private volatile float threadInterpolationAlpha;

        private volatile bool isGamePaused = false;

        #endregion

        # region Template methods

        protected virtual void ProcessBackgroundTasks() { }

        protected virtual bool ShouldTickAccumulator() { return true; }

        protected abstract void GetSimulationInput(ref RawInput[] simulationInput);

        protected abstract void GatherLocalInput();

        // something to check if the match ended

        #endregion

        void Awake()
        {
            if (Camera.main != null)
                cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
        }

        protected virtual void Start()
        {
            // Build initial data from Unity Scene
            GameState initialState = BuildInitialState(out LogicCollider blastzone);
            AttackData[][] attackData = GatherAttackData();

            logicEngine = new Core.GameLogicEngine();
            logicEngine.Initialize(config, initialState, attackData, blastzone);

            threadInput = new RawInput[initialState.Characters.Length];

            // Get initial render states

            renderState = new GameState();
            previousRenderState = new GameState();

            Core.GameLogicEngine.DeepCopyGameState(initialState, ref renderState);
            Core.GameLogicEngine.DeepCopyGameState(initialState, ref previousRenderState);
            
            // Start the thread

            isRunning = true;
            simulationThread = new Thread(SimulationThreadLoop)
            {
                IsBackground = true
            };

            simulationThread.Start();

            #if UNITY_EDITOR
                EditorApplication.pauseStateChanged += OnEditorPauseStateChanged;
            #endif
        }

        private void SimulationThreadLoop()
        {
            Profiler.BeginThreadProfiling("RollbackEngine", "SimulationThread");
            
            Stopwatch stopwatch = Stopwatch.StartNew();

            double accumulator = 0;
            double fixedDeltaTime = logicEngine.FixedDeltaTime;
            double lastTime = stopwatch.Elapsed.TotalSeconds;

            RawInput[] currentInput = new RawInput[characters.Length];

            while (isRunning)
            {
                if (isGamePaused)
                {
                    lastTime = stopwatch.Elapsed.TotalSeconds;
                    Thread.Sleep(1);
                    continue;    
                }

                // Calculate delta time
                double currentTime = stopwatch.Elapsed.TotalSeconds;
                double frameTime = currentTime - lastTime;
                lastTime = currentTime;
                accumulator += frameTime;

                // Network processing and rollback

                ProcessBackgroundTasks();

                while (accumulator >= fixedDeltaTime)
                {
                    if (!ShouldTickAccumulator())
                    {
                        accumulator -= fixedDeltaTime;
                        continue;
                    }

                    // Get the input for the simulation

                    GetSimulationInput(ref currentInput);

                    Profiler.BeginSample("LogicEngine.RunSingleTick");

                    // Tick the logic
                    logicEngine.RunSingleTick(currentInput);

                    Profiler.EndSample();

                    // Copy the states for the Unity thread to render
                    lock (stateLock)
                    {
                        GameLogicEngine.CopyGameStateData(renderState, ref previousRenderState);
                        GameLogicEngine.CopyGameStateData(logicEngine.GetCurrentGameState(), ref renderState);
                    }

                    if (logicEngine.MatchEnded)
                    {
                        isRunning = false;
                    }

                    accumulator -= fixedDeltaTime;
                }

                // Interpolation factor for unity to draw smooth movement
                // threadInterpolationAlpha = (float)(accumulator / fixedDeltaTime);

                // If the accumulator is empty, let the thread sleep for 1 millisecond
                Thread.Sleep(1);
            }

            Profiler.EndThreadProfiling();
        }

        private void Update()
        {
            if (logicEngine.MatchEnded)
            {
                EndMatch(logicEngine.StateBuffer[logicEngine.CurrentTick % config.BufferSize]);
                return; 
            }

            GatherLocalInput();

            lock (stateLock)
            {
                UpdateVisuals(renderState, previousRenderState, logicEngine.CurrentTick);
            }
        }

        private GameState BuildInitialState(out LogicCollider blastzoneCollider)
        {
            GameState gameState = new GameState();

            BaseColliderFactory[] colliderFactories = FindObjectsOfType<BaseColliderFactory>().Where(c => c.GetComponent<Character>() == null).ToArray();
            var staticColliders = colliderFactories.Where(i => i.Layer != ColliderLayer.Blastzone).ToArray();

            gameState.StaticColliders = new LogicCollider[Math.Min(staticColliders.Length, 100)];
            for (int i = 0; i < gameState.StaticColliders.Length; i++)
            {
                gameState.StaticColliders[i] = staticColliders[i].GetLogicCollider();
            }

            blastzoneCollider = colliderFactories.First(i => i.Layer == ColliderLayer.Blastzone).GetLogicCollider();
                
            characters = FindObjectsOfType<Character>();
            gameState.Characters = new Data.Character.CharacterData[Math.Min(characters.Length, 16)];

            for (int i = 0; i < gameState.Characters.Length; i++)
            {
                gameState.Characters[i].FacingDirection = (int)characters[i].transform.lossyScale.x;
                gameState.Characters[i].DynamicBody = characters[i].GetLogicBody();
                gameState.Characters[i].Stats = characters[i].GetLogicCharacterStats(logicEngine != null ? logicEngine.FixedDeltaTime : (FixedFloat)(1f/config.TargetFPS));
                gameState.Characters[i].CurrentState = Data.Character.CharacterStateType.Fall;
                gameState.Characters[i].SpawnPosition = (Vector2)characters[i].transform.position;
                gameState.Characters[i].RemainingStocks = 3;

                gameState.Characters[i].Hurtboxes = new Data.Combat.HurtboxData[1];
                gameState.Characters[i].Hurtboxes[0] = new Data.Combat.HurtboxData { Collider = characters[i].GetHurtbox() };

                gameState.Characters[i].Damage = 50;
            }

            return gameState;
        }

        private AttackData[][] GatherAttackData()
        {
            int totalAttackTypes = Enum.GetValues(typeof(AttackType)).Length;
            AttackData[][] attacks = new AttackData[characters.Length][];

            for (int i = 0; i < characters.Length; i++)
            {
                attacks[i] = new AttackData[totalAttackTypes];
                foreach (AttackDataSO attack in characters[i].Attacks)
                {
                    AttackData attackData = attack.GetAttackData();
                    attacks[i][(int)attackData.Type] = attackData;
                }
            }
            return attacks;
        }

        private void UpdateVisuals(GameState gameState, GameState prevState, int currentTick)
        {
            for (int i = 0; i < characters.Length; i++)
            {
                characters[i].UpdateState(gameState.Characters[i]);

                if (ShowHitboxes && DebugDrawer.Instance != null)
                {
                    AttackData attack = logicEngine.Attacks[i][(int)gameState.Characters[i].AttackType];
                    DebugDrawer.Instance.DrawCharacter(gameState.Characters[i], attack);
                }

                if (prevState.Characters[i].Damage != gameState.Characters[i].Damage)
                    Core.UI.MatchEventBus.OnCharacterDamageChanged?.Invoke(i, gameState.Characters[i].Damage);

                if (prevState.Characters[i].RemainingStocks != gameState.Characters[i].RemainingStocks)
                    Core.UI.MatchEventBus.OnCharacterStocksChanged?.Invoke(i, gameState.Characters[i].RemainingStocks);
            }

            if (cameraBrain != null)
                cameraBrain.ManualUpdate();

            long framesRemaining = logicEngine.TotalMatchFrames - currentTick;
            UI.MatchEventBus.OnTimerUpdated?.Invoke(framesRemaining, config.TargetFPS);
        }

        private void EndMatch(GameState state)
        {
            int winningPlayer = -1;

            Data.Character.CharacterData characterOne = state.Characters[0];
            Data.Character.CharacterData characterTwo = state.Characters.Length > 1 ? state.Characters[1] : new Data.Character.CharacterData(); 
            
            if (characterOne.RemainingStocks != characterTwo.RemainingStocks)
            {
                if (characterOne.RemainingStocks > characterTwo.RemainingStocks)
                {
                    winningPlayer = 1;
                }
                else
                {
                    winningPlayer = 2;
                }
            }
            else if (characterOne.Damage != characterTwo.Damage)
            {
                if (characterOne.Damage > characterTwo.Damage)
                {
                    winningPlayer = 1;
                }
                else
                {
                    winningPlayer = 2;
                }
            }

            MatchResultsData.WinnerPlayer = winningPlayer;

            MatchResultsData.Player1TotalDamageDealt = characterOne.Score;
            MatchResultsData.Player2TotalDamageDealt = characterTwo.Score;
            
            SceneManager.LoadScene("ResultsScene");
        }

        private void OnDestroy()
        {
            // Clean up the thread
            isRunning = false;
            simulationThread?.Join(500); 

            #if UNITY_EDITOR
                EditorApplication.pauseStateChanged -= OnEditorPauseStateChanged;
            #endif
        }

        #if UNITY_EDITOR
        private void OnEditorPauseStateChanged(PauseState state)
        {
            if (state == PauseState.Paused)
            {
                isGamePaused = true;
                UnityEngine.Debug.Log("<color=yellow>Editor Paused:</color> Simulation Thread Frozen.");
            }
            else if (state == PauseState.Unpaused)
            {
                isGamePaused = false;
                UnityEngine.Debug.Log("<color=green>Editor Unpaused:</color> Simulation Thread Resumed.");
            }
        }
        #endif

    }
}