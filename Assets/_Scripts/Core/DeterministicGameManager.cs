using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Simulation;
using Data.Combat;
using Data;

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

    [Header("Rollback settings")]

    [SerializeField]
    private readonly int BufferSize = 60;
    private GameState[] stateBuffer;

    public int CurrentTick { get; private set; }
    
    private float fixedDeltaTime;
    
    private float accumulator = 0;

    private GameSimulation gameSimulation;

    private Character[] characters;

    void Start()
    {
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

        gameState.StaticColliderCount = math.min(colliderFactories.Length, 100);

        gameState.StaticColliders = new LogicCollider[gameState.StaticColliderCount];

        for (int i = 0; i < gameState.StaticColliderCount; i++)
        {
            gameState.StaticColliders[i] = colliderFactories[i].GetLogicCollider();

            gameState.StaticColliders[i].BoundingBox = gameState.StaticColliders[i].GetBoundingBox();
        }
        
        characters = FindObjectsOfType<Character>();

        gameState.CharactersCount = math.min(characters.Length, 100);
        gameState.Characters = new Data.CharacterData[gameState.CharactersCount];

        for (int i = 0; i < gameState.CharactersCount; i++)
        {
            gameState.Characters[i].FacingDirection = (int)characters[i].transform.lossyScale.x;
            gameState.Characters[i].DynamicBody = characters[i].GetLogicBody();
            gameState.Characters[i].Stats = characters[i].GetLogicCharacterStats(fixedDeltaTime);
            gameState.Characters[i].CurrentState = Data.CharacterStateType.Fall;

            var hurtbox = new Data.Combat.HurtboxData[1];
            hurtbox[0] = new Data.Combat.HurtboxData {Collider = characters[i].GetHurtbox()};
            
            gameState.Characters[i].Hurtboxes = hurtbox;

            gameState.Characters[i].HitTargets = new bool[gameState.Characters.Length];
        }
    }

    private void InitializeGameStateArrays(ref GameState gameState, int staticColliderCount, int characterCount)
    {
        gameState.StaticColliderCount = math.min(staticColliderCount, 100);
        gameState.StaticColliders = new LogicCollider[gameState.StaticColliderCount];

        gameState.CharactersCount = math.min(characterCount, 100);
        gameState.Characters = new Data.CharacterData[gameState.CharactersCount];
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

    void Update()
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
        int previousIndex = CurrentTick % BufferSize;
        int currentIndex = (CurrentTick + 1) % BufferSize;

        ref GameState previousState = ref stateBuffer[previousIndex];
        ref GameState currentState = ref stateBuffer[currentIndex];

        DeepCopyGameState(ref previousState, ref currentState);

        for (int i = 0; i < characters.Length; i++)
        {
            Character character = characters[i];

            RawInput input = character.GetRawInput();

            input.FrameId = (ushort)CurrentTick;

            currentState.Characters[i].RawInput = input;

            //TODO: Remove this in actual game
            // currentState.Characters[i].Stats = characters[i].GetLogicCharacterStats(fixedDeltaTime);
        }

        // store the game data in the buffer, get the input etc

        gameSimulation.AdvanceFrame(ref currentState, previousState);

        CurrentTick++;
    }

    private void UpdateVisuals()
    {
        GameState gameState = stateBuffer[CurrentTick % BufferSize];

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
        }
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
            destination.Characters[i] = source.Characters[i];
        }
    }
}
