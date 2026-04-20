using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHandler : MonoBehaviour
{
    public int PlayerIndex { get; private set;}
    
    public GameObject? SelectedCharacterPrefab { get; private set; }
    
    private PlayerInput playerInput;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        playerInput = GetComponent<PlayerInput>();
        PlayerIndex = playerInput.playerIndex;
    }

    // Called during the Character Select screen
    public void SetCharacter(GameObject? characterPrefab)
    {
        SelectedCharacterPrefab = characterPrefab;
    }

    // Called by the Game Manager in the Combat scene
    public void SpawnCharacter(Transform spawnPoint)
    {
        if (SelectedCharacterPrefab == null)
            return;

        // Spawn the character
        GameObject character = Instantiate(SelectedCharacterPrefab, spawnPoint.position, spawnPoint.rotation);

        character.transform.localScale = new Vector3(
            spawnPoint.localScale.x,
            transform.localScale.y,
            transform.localScale.z
        );

        // Switch the controller from UI mode to Player mode TODO: Remove magic string
        playerInput.SwitchCurrentActionMap("Player");
        
        // Assign the input
        character.GetComponent<InputController>().Initialize(playerInput);
    }
}