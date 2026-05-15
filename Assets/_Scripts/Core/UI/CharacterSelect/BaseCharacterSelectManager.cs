using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public abstract class BaseCharacterSelectManager : MonoBehaviour
{
    [SerializeField]
    protected GameObject[] characterPool; 

    [SerializeField] 
    private Transform canvasTransform;
    [SerializeField] 
    private GameObject cursorPrefab;
    [SerializeField] 
    protected TMP_Text startLabel;

    [SerializeField]
    protected GameObject remotePlayerPrefab;

    protected List<PlayerHandlerBase> joinedPlayers = new List<PlayerHandlerBase>();
    private int MaxPlayerCount;
    protected bool canStartMatch;
    
    public Core.CharacterType SelectedCharacter;

    void Awake()
    {
        MaxPlayerCount = GetComponent<PlayerInputManager>().maxPlayerCount;
    }

    protected virtual void Start()
    {
        PlayerHandlerBase[] returningPlayers = FindObjectsOfType<PlayerHandlerBase>();

        foreach (PlayerHandlerBase player in returningPlayers)
        {
            // Reset their previous character choice so they have to pick again
            player.SetCharacter(null); 
            
            SetupPlayerUI(player, player.GetComponent<PlayerInput>());
        }
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerHandlerBase newPlayer = playerInput.GetComponent<PlayerHandlerBase>();
        SetupPlayerUI(newPlayer, playerInput);
    }

    private void SetupPlayerUI(PlayerHandlerBase playerHandler, PlayerInput playerInput)
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

        Debug.Log(newCursor);
        
        cursorScript.PlayerHandler = playerHandler;
        cursorScript.PlayerIndex = playerInput.playerIndex;
        
        // We initialize the cursor and let IT handle its own input listening.
        // This is much safer than subscribing to events on the persistent manager.
        cursorScript.Initialize(playerInput, this);
    }

    // Called by the VirtualCursor when a player locks in a character
    public void CheckStartCondition()
    {
        canStartMatch = joinedPlayers.Count(p => p.SelectedCharacterPrefab != null) == MaxPlayerCount;
        startLabel?.gameObject?.SetActive(canStartMatch);
    }

    // Called by the VirtualCursor when a player presses Start
    public abstract void TryStartMatch();

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        PlayerHandlerBase player = playerInput.GetComponent<PlayerHandlerBase>();
        
        if (joinedPlayers.Contains(player))
        {
            joinedPlayers.Remove(player);
        }

        CheckStartCondition();
        
        // The PlayerInputManager handles destroying the GameObject, 
        // so any cursors linked to it need to clean themselves up.
    }

    public void HandleBack()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}