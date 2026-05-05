using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using Data;
using static Newtonsoft.Json.JsonConvert;

public class BaseTest
{
    protected GameState gameState, previousGameState;
    protected Simulation.GameSimulation gameSimulation;
    
    protected string GetFileData(string fileName)
    {
        string path = Path.Combine(Application.dataPath, "Tests/Data", fileName);

        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }

        Assert.Fail($"Required test data file not found: {fileName}");

        return string.Empty;
    }

    [SetUp]
    public void Setup()
    {
        var gameStateData = GetFileData("initial-gamestate.json");
        var globalStats = GetFileData("global-stats.json");

        gameState = DeserializeObject<GameState>(gameStateData);
        previousGameState = DeserializeObject<GameState>(gameStateData);

        Simulation.Character.GlobalCharacterStats.ApplyData(DeserializeObject<Simulation.Character.GlobalCharacterStatsSerializable>(globalStats));

        var blastzone = new LogicCollider
        {
            Position = new FixedVector2(-2.5758814812f, -0.4582579136f),
            Extents = new FixedVector2(22.305688858f, 12.1223154068f),
            Type = ColliderType.Box,
            Layer = ColliderLayer.Blastzone,
        };

        gameSimulation = new Simulation.GameSimulation();

        gameSimulation.SetMinimumStaticColliderExtends(gameState.StaticColliders);
        gameSimulation.SetBlastzone(blastzone);
        // gameSimulation.SetAttackData(new Data.Combat.AttackData[1][]);
    }

    private void RunTick(ushort currentTick, RawInput[] characterInputs, GameState[] gameStates)
    {
        DeepCopyGameState(ref gameState, ref previousGameState);

        var input = new RawInput();

        int delayedTick = currentTick - 3;

        if (delayedTick >= 0)
        {
            input = characterInputs[delayedTick];
        }

        input.FrameId = currentTick;
        
        gameState.Characters[0].RawInput = input;

        // gameSimulation.AdvanceFrame(ref gameState, previousGameState);

        var expectedGameState = gameStates[currentTick];

        Debug.Log($"Checking tick {currentTick}");
        
        SerializeObject(previousGameState.Characters[0]).Should().Be(SerializeObject(expectedGameState.Characters[0]));
    }

    [TestCase("test")]
    public void Test(string folderName)
    {
        RawInput[] inputs = DeserializeObject<RawInput[]>(GetFileData($"{folderName}/inputs.json"));
        
        GameState[] gameStates = DeserializeObject<GameState[]>(GetFileData($"{folderName}/game-states.json"));

        for (ushort i = 0; i < 600; i++)
        {
            RunTick(i, inputs, gameStates);
        }
    }

    private void DeepCopyGameState(ref GameState source, ref GameState destination)
    {
        destination.FrameNumber = source.FrameNumber;
        destination.FrameNumber++;

        for (int i = 0; i < destination.StaticColliders.Length; i++)
        {
            destination.StaticColliders[i] = source.StaticColliders[i];
        }

        for (int i = 0; i < destination.Characters.Length; i++)
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