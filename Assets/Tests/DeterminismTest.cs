using System;
using System.IO;
using FluentAssertions;
using NUnit.Framework;
using UnityEngine;
using Data;
using static Newtonsoft.Json.JsonConvert;

public class DeterminismTest
{
    private string gameStateData;
    protected GameState initialGameState;
    protected Simulation.GameSimulation gameSimulation;

    protected Core.GameLogicEngine gameLogicEngine;
    
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

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        gameStateData = GetFileData("Global/initial-gamestate.json");
        string globalStats = GetFileData("Global/global-stats.json");

        Simulation.Character.GlobalCharacterStats.ApplyData(DeserializeObject<Simulation.Character.GlobalCharacterStatsSerializable>(globalStats));
    }

    [SetUp]
    public void Setup()
    {
        initialGameState = DeserializeObject<GameState>(gameStateData);
        
        var blastzone = new LogicCollider
        {
            Position = new FixedVector2(-2.5758814812f, -0.4582579136f),
            Extents = new FixedVector2(22.305688858f, 12.1223154068f),
            Type = ColliderType.Box,
            Layer = ColliderLayer.Blastzone,
        };

        SimulationConfig config = new SimulationConfig { TargetFPS = 60, MinutesPerMatch = 3, InputDelay = 3, BufferSize = 60 };

        gameLogicEngine = new Core.GameLogicEngine();

        gameLogicEngine.Initialize(config, initialGameState, new Data.Combat.AttackData[1][], blastzone);
    }

    private void RunTick(RawInput[] input, GameState expectedGameState)
    {
        SerializeObject(gameLogicEngine.GetCurrentGameState().Characters[0]).Should().Be(SerializeObject(expectedGameState.Characters[0]));

        // gameLogicEngine.GetCurrentGameState().Characters[0].Should().Be(expectedGameState.Characters[0]);

        gameLogicEngine.RunSingleTick(input);
    }

    [TestCase("walk-test")]
    [TestCase("respawn-invincibility-test")]
    [TestCase("jump-fall-test")]
    [TestCase("air-dodge-test")]
    [TestCase("dash-dash-test")]
    [TestCase("dash-run-jump-test")]
    [TestCase("platform-collision-logic-test")]
    public void Test(string folderName)
    {
        RawInput[] inputs = DeserializeObject<RawInput[]>(GetFileData($"{folderName}/inputs.json"));
        
        GameState[] gameStates = DeserializeObject<GameState[]>(GetFileData($"{folderName}/game-states.json"));

        RawInput[] tickInput = new RawInput[1];

        for (ushort i = 0; i < 600; i++)
        {
            tickInput[0] = inputs[i];

            RunTick(tickInput, gameStates[i]);
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