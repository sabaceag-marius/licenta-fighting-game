using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Simulation;
using Data.Combat;
using Data;
using UnityEngine.SceneManagement;
using Cinemachine;

public class DeterministicGameManager : MonoBehaviour
{
    [Header("Debug settings")]
    
    [SerializeField]
    private bool ShowHitboxes = true;

    [SerializeField]
    private DebugDrawer debugDrawer;

    [Header("Simulation Settings")]

    [SerializeField]
    private int TargetFPS = 60;

    [SerializeField]
    private int minutesPerMatch;
    
    private long totalMatchFrames;

    [Header("Rollback settings")]

    [SerializeField]
    [Tooltip("How many frames to delay local inputs to mask network latency.")]
    private int InputDelay = 3;

    [SerializeField]
    private readonly int BufferSize = 60;
    private GameState[] stateBuffer;
    private RawInput[][] inputBuffer;
    private int currentTick;
    
    private float fixedDeltaTime;
    
    private float accumulator = 0;

    private GameSimulation gameSimulation;

    private Character[] characters;

    private CinemachineBrain cameraBrain;

    void Awake()
    {
        if (Camera.main != null)
        {
            cameraBrain = Camera.main.GetComponent<CinemachineBrain>();
        }
    }

    private void Start()
    {
        totalMatchFrames = (long)(minutesPerMatch * 60 * TargetFPS);

        fixedDeltaTime = 1f / TargetFPS;

        // Create the GameSimulation class

        gameSimulation = new GameSimulation();

        stateBuffer = new GameState[BufferSize];

        InitializeStartingState();

        for (int i = 1; i < BufferSize; i++)
        {
            InitializeGameStateArrays(ref stateBuffer[i], stateBuffer[0].StaticColliderCount, stateBuffer[0].CharactersCount);
        }

        InitializeAttackData();
    }

    private void InitializeStartingState()
    {
        ref GameState gameState = ref stateBuffer[0];

        // skip the colliders that are attached to the deterministic rigidbody

        BaseColliderFactory[] colliderFactories = FindObjectsOfType<BaseColliderFactory>()
            .Where(c => c.GetComponent<Character>() == null).ToArray();
        //|| allColliders[i].GetComponentInParent<DeterministicRigidBody>() == null)

        var staticColliders = colliderFactories.Where(i => i.Layer != ColliderLayer.Blastzone).ToArray();

        gameState.StaticColliderCount = math.min(staticColliders.Length, 100);

        gameState.StaticColliders = new LogicCollider[gameState.StaticColliderCount];

        for (int i = 0; i < gameState.StaticColliderCount; i++)
        {
            LogicCollider collider = staticColliders[i].GetLogicCollider();
            
            gameState.StaticColliders[i] = collider;

            gameState.StaticColliders[i].BoundingBox = gameState.StaticColliders[i].GetBoundingBox();
        }

        gameSimulation.SetMinimumStaticColliderExtends(gameState.StaticColliders);
        
        // Blastzone
        
        LogicCollider blastzoneCollider = colliderFactories.First(i => i.Layer == ColliderLayer.Blastzone).GetLogicCollider();
        gameSimulation.SetBlastzone(blastzoneCollider.GetBoundingBox());
            
        characters = FindObjectsOfType<Character>();

        gameState.CharactersCount = math.min(characters.Length, 16);
        gameState.Characters = new Data.CharacterData[gameState.CharactersCount];

        for (int i = 0; i < gameState.CharactersCount; i++)
        {
            gameState.Characters[i].FacingDirection = (int)characters[i].transform.lossyScale.x;
            gameState.Characters[i].DynamicBody = characters[i].GetLogicBody();
            gameState.Characters[i].Stats = characters[i].GetLogicCharacterStats(fixedDeltaTime);
            gameState.Characters[i].CurrentState = Data.CharacterStateType.Fall;
            gameState.Characters[i].SpawnPosition = (Vector2)characters[i].transform.position;
            gameState.Characters[i].RemainingStocks = 3;

            var hurtbox = new Data.Combat.HurtboxData[1];
            hurtbox[0] = new Data.Combat.HurtboxData {Collider = characters[i].GetHurtbox()};
            
            gameState.Characters[i].Hurtboxes = hurtbox;

            // gameState.Characters[i].DamagePercentage = characters[i].Damage;
            gameState.Characters[i].DamagePercentage = 100;
        }

        // Input buffers

        inputBuffer = new RawInput[gameState.Characters.Length][];

        for (int i = 0; i < gameState.Characters.Length; i++)
        {
            inputBuffer[i] = new RawInput[BufferSize];
        }
    }

    private void InitializeGameStateArrays(ref GameState gameState, int staticColliderCount, int characterCount)
    {
        gameState.StaticColliderCount = math.min(staticColliderCount, 100);
        gameState.StaticColliders = new LogicCollider[gameState.StaticColliderCount];

        gameState.CharactersCount = math.min(characterCount, 100);
        gameState.Characters = new Data.CharacterData[gameState.CharactersCount];

        for (int i = 0; i < gameState.Characters.Length; i++)
        {
            gameState.Characters[i].Hurtboxes = new Data.Combat.HurtboxData[1];
        }
    }    

    private void InitializeAttackData()
    {
        int totalAttackTypes = System.Enum.GetValues(typeof(Data.Combat.AttackType)).Length;

        AttackData[][] attacks = new Data.Combat.AttackData[characters.Count()][];

        for (int i = 0; i < characters.Count(); i++)
        {
            attacks[i] = new Data.Combat.AttackData[totalAttackTypes];

            foreach (AttackDataSO attack in characters[i].Attacks)
            {
                AttackData attackData = attack.GetAttackData();

                attacks[i][(int)attackData.Type] = attackData;
            }
        }

        gameSimulation.InitializeAttackData(attacks);
    }

    private void Update()
    {
        accumulator += Time.deltaTime;

        while (accumulator >= fixedDeltaTime)
        {
            RunSingleTick();

            accumulator -= fixedDeltaTime;
        }

        UpdateVisuals();
    }

    private void RunSingleTick()
    {        
        int previousIndex = currentTick % BufferSize;
        int currentIndex = (currentTick + 1) % BufferSize;

        ref GameState previousState = ref stateBuffer[previousIndex];
        ref GameState currentState = ref stateBuffer[currentIndex];

        DeepCopyGameState(ref previousState, ref currentState);

        for (int i = 0; i < characters.Length; i++)
        {
            Character character = characters[i];

            RawInput input = character.GetRawInput();

            RawInput currentHardwareInput = character.GetRawInput();
            inputBuffer[i][currentTick % BufferSize] = currentHardwareInput;

            RawInput simulationInput = new RawInput();

            int delayedTick = currentTick - InputDelay;

            if (delayedTick >= 0)
            {
                simulationInput = inputBuffer[i][delayedTick % BufferSize];
            }

            simulationInput.FrameId = (ushort)currentTick;
            
            simulationInput.FrameId = (ushort)currentTick;
            currentState.Characters[i].RawInput = simulationInput;

            // input.FrameId = (ushort)currentTick;
            // currentState.Characters[i].RawInput = input;

            //TODO: Remove this in actual game
            currentState.Characters[i].Stats = characters[i].GetLogicCharacterStats(fixedDeltaTime);
        }

        // store the game data in the buffer, get the input etc

        gameSimulation.AdvanceFrame(ref currentState, previousState);

        currentTick++;

        bool matchEnded = false;

        for (int i = 0; i < currentState.Characters.Length; i++)
        {
            if (currentState.Characters[i].RemainingStocks == 0)
            {
                matchEnded = true;
                break;
            }
        }

        if (currentTick == totalMatchFrames || matchEnded)
        {
            EndMatch(currentState);
        }
    }

    private void UpdateVisuals()
    {
        GameState prevState = stateBuffer[(currentTick -1 + BufferSize) % BufferSize];
        GameState gameState = stateBuffer[currentTick % BufferSize];

        for (int i = 0; i < characters.Length; i++)
        {
            Character character = characters[i];
            CharacterData characterData = gameState.Characters[i];

            character.UpdateState(characterData);

            if (ShowHitboxes && debugDrawer != null)
            {
                Data.Combat.AttackData attack = gameSimulation.GetCharacterAttack(i, characterData.AttackType);

                debugDrawer.DrawCharacter(characterData, attack);
            }

            CharacterData prevCharacterData = prevState.Characters[i];

            if (prevCharacterData.DamagePercentage != characterData.DamagePercentage)
            {
                UIMatchManager.Instance.UpdateCharacterDamage(i, characterData.DamagePercentage);
            }

            if (prevCharacterData.RemainingStocks != characterData.RemainingStocks)
            {
                UIMatchManager.Instance.UpdateCharacterStocks(i, characterData.RemainingStocks);
            }
        }

        // Update timer
        long framesRemaining = totalMatchFrames - currentTick;

        UIMatchManager.Instance.UpdateTimer(framesRemaining, TargetFPS);

        // Manually update the camera

        if (cameraBrain != null)
        {
            cameraBrain.ManualUpdate();
        }
    }

    private void EndMatch(GameState state)
    {
        int winningPlayer = -1;

        CharacterData characterOne = state.Characters[0];
        CharacterData characterTwo = state.Characters[1]; 
        
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
        else if (characterOne.DamagePercentage != characterTwo.DamagePercentage)
        {
            if (characterOne.DamagePercentage > characterTwo.DamagePercentage)
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

    private void DeepCopyGameState(ref GameState source, ref GameState destination)
    {
        destination.FrameNumber = source.FrameNumber;
        destination.FrameNumber++;

        destination.StaticColliderCount = source.StaticColliderCount;
        destination.CharactersCount = source.CharactersCount;

        for (int i = 0; i < destination.StaticColliderCount; i++)
        {
            destination.StaticColliders[i] = source.StaticColliders[i];
        }

        for (int i = 0; i < destination.CharactersCount; i++)
        {
            // We save the hurtbox for the character before copying it because
            // otherwise the hurtbox array of the character will have the pointer
            // to the array of the source AND will leave this array unused, whill
            // will lead to the GarbageCollector triggering

            var preAllocatedHurtboxes = destination.Characters[i].Hurtboxes;

            destination.Characters[i] = source.Characters[i];

            destination.Characters[i].Hurtboxes = preAllocatedHurtboxes;

            destination.Characters[i].Hurtboxes[0] = source.Characters[i].Hurtboxes[0];
        }
    }
}
