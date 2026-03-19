using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class DeterministicGameManager : MonoBehaviour
{
    [Header("Simulation Settings")]

    [SerializeField]
    private int TargetFPS = 60;

    [SerializeField]
    private InputManager inputManager;

    public int CurrentTick { get; private set; }
    
    private float fixedDeltaTime;
    
    private float accumulator = 0;

    private GameState gameState;

    private GameSimulation gameSimulation;

    private DynamicBody[] dynamicBodies;

    void Start()
    {
        fixedDeltaTime = 1f / TargetFPS;

        // Create the GameSimulation class

        gameSimulation = new GameSimulation();

        gameState = new GameState();

        InitializeStartingState();
    }

    private void InitializeStartingState()
    {
        // skip the colliders that are attached to the deterministic rigidbody

        BaseColliderFactory[] colliderFactories = FindObjectsOfType<BaseColliderFactory>()
            .Where(c => c.GetComponent<DynamicBody>() == null).ToArray();
        //|| allColliders[i].GetComponentInParent<DeterministicRigidBody>() == null)

        gameState.StaticColliderCount = math.min(colliderFactories.Length, 100);

        gameState.StaticColliders = new LogicCollider[gameState.StaticColliderCount];

        for (int i = 0; i < gameState.StaticColliderCount; i++)
        {
            gameState.StaticColliders[i] = colliderFactories[i].GetLogicCollider();

            gameState.StaticColliders[i].BoundingBox = gameState.StaticColliders[i].GetBoundingBox();
        }

        dynamicBodies = FindObjectsOfType<DynamicBody>();

        gameState.DynamicBodiesCount = math.min(dynamicBodies.Length, 100);

        gameState.DynamicBodies = new LogicDynamicBody[gameState.DynamicBodiesCount];

        for (int i = 0; i < gameState.DynamicBodiesCount; i++)
        {
            gameState.DynamicBodies[i] = dynamicBodies[i].GetLogicBody();
        }
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
        // store the game data in the buffer, get the input etc

        gameSimulation.AdvanceFrame(ref gameState, inputManager.CurrentFrameInput);

        CurrentTick++;
    }

    private void UpdateVisuals()
    {
        for (int i = 0; i < dynamicBodies.Length; i++)
        {
            var dynamicBody = dynamicBodies[i];

            var logicDynamicBody = gameState.DynamicBodies[i];

            var position = dynamicBody.transform.position;

            dynamicBody.transform.position = new Vector2((float)logicDynamicBody.Position.x, (float)logicDynamicBody.Position.y);
        }
    }
}
