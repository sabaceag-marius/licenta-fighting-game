using UnityEngine;

public class RemotePlayerHandler : PlayerHandlerBase
{
    public void InitializeRemote(int targetPlayerIndex)
    {
        PlayerIndex = targetPlayerIndex;
    }

    protected override void InitializeCharacterInput(GameObject spawnedCharacter)
    {
    }
}