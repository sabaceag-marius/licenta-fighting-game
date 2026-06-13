using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class LocalPlayerHandler : PlayerHandlerBase
{
    private PlayerInput playerInput;

    protected override void Awake()
    {
        base.Awake();
        playerInput = GetComponent<PlayerInput>();
        PlayerIndex = playerInput.playerIndex;
    }

    protected override void InitializeCharacterInput(GameObject spawnedCharacter)
    {
        // Switch the controller from UI mode to Player mode
        playerInput.SwitchCurrentActionMap("Player");
        
        // Assign the local hardware input
        var inputController = spawnedCharacter.GetComponent<InputController>();

        if (inputController != null)
        {
            inputController.Initialize(playerInput);
        }
    }
}