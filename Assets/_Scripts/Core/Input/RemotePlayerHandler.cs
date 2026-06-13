using UnityEngine;

public class RemotePlayerHandler : PlayerHandlerBase
{
    // Call this manually when you instantiate the remote handler in the lobby
    public void InitializeRemote(int targetPlayerIndex)
    {
        PlayerIndex = targetPlayerIndex;
    }

    protected override void InitializeCharacterInput(GameObject spawnedCharacter)
    {
    }
}