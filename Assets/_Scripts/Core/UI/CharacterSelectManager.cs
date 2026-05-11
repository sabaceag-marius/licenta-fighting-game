using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [SerializeField] private Transform canvasTransform;
    [SerializeField] private GameObject cursorPrefab;
    [SerializeField] private TMP_Text startLabel;

    private List<PlayerHandler> joinedPlayers = new List<PlayerHandler>();
    private int MaxPlayerCount;
    private bool canStartMatch;

    void Awake()
    {
        MaxPlayerCount = GetComponent<PlayerInputManager>().maxPlayerCount;
    }

    void Start()
    {
        PlayerHandler[] returningPlayers = FindObjectsOfType<PlayerHandler>();

        foreach (PlayerHandler player in returningPlayers)
        {
            // Reset their previous character choice so they have to pick again
            player.SetCharacter(null); 
            
            SetupPlayerUI(player, player.GetComponent<PlayerInput>());
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerHandler newPlayer = playerInput.GetComponent<PlayerHandler>();
        SetupPlayerUI(newPlayer, playerInput);
    }

    private void SetupPlayerUI(PlayerHandler playerHandler, PlayerInput playerInput)
    {
        if (!joinedPlayers.Contains(playerHandler))
        {
            joinedPlayers.Add(playerHandler);
        }

        // Switch them back to UI mode
        playerInput.SwitchCurrentActionMap("UI Overlay");

        // Spawn the visual cursor
        GameObject newCursor = Instantiate(cursorPrefab, canvasTransform);
        VirtualCursor cursorScript = newCursor.GetComponent<VirtualCursor>();
        
        cursorScript.PlayerHandler = playerHandler;
        cursorScript.PlayerIndex = playerInput.playerIndex;
        
        // We initialize the cursor and let IT handle its own input listening.
        // This is much safer than subscribing to events on the persistent manager.
        cursorScript.Initialize(playerInput, this);
    }

    // Called by the VirtualCursor when a player locks in a character
    public void CheckStartCondition()
    {
        canStartMatch = true;
        // canStartMatch = joinedPlayers.Count(p => p.SelectedCharacterPrefab != null) == MaxPlayerCount;
        startLabel?.gameObject.SetActive(canStartMatch);
    }

    // Called by the VirtualCursor when a player presses Start
    public void TryStartMatch()
    {
        if (canStartMatch)
        {
            canStartMatch = false;
            SceneManager.LoadScene("CombatScene");
        }
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        PlayerHandler player = playerInput.GetComponent<PlayerHandler>();
        
        if (joinedPlayers.Contains(player))
        {
            joinedPlayers.Remove(player);
        }

        CheckStartCondition();
        
        // The PlayerInputManager handles destroying the GameObject, 
        // so any cursors linked to it need to clean themselves up.
    }
}