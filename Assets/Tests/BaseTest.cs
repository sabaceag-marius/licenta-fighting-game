using System.IO;
using NUnit.Framework;
using UnityEngine;
using static Newtonsoft.Json.JsonConvert;

public class BaseTest
{
    protected GameState gameState, previousGameState;

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

        gameState = DeserializeObject<GameState>(gameStateData);
        previousGameState = DeserializeObject<GameState>(gameStateData);
    }

    [Test]
    public void Test()
    {
        
    }
}