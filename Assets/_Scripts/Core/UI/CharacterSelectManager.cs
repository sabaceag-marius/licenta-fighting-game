using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelectManager : MonoBehaviour
{
    [SerializeField] 
    private Transform canvasTransform;

    [SerializeField] 
    private GameObject cursorPrefab;

    [SerializeField]
    private TMP_Text startLabel;

    private List<PlayerHandler> joinedPlayers = new List<PlayerHandler>();

    private int MaxPlayerCount;

    private bool canStartMatch;


    void Awake()
    {
        MaxPlayerCount = GetComponent<PlayerInputManager>().maxPlayerCount;
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        PlayerHandler newPlayer = playerInput.GetComponent<PlayerHandler>();
        
        joinedPlayers.Add(newPlayer);

        // Spawn the visual cursor on the UI Canvas
        GameObject newCursor = Instantiate(cursorPrefab, canvasTransform);
        VirtualCursor cursorScript = newCursor.GetComponent<VirtualCursor>();
        
        cursorScript.PlayerHandler = newPlayer;
        cursorScript.PlayerIndex = playerInput.playerIndex;
        
        playerInput.onActionTriggered += context => 
        {
            if (context.action.name == "LeftStick") 
                cursorScript.OnMove(context);

            if (context.action.name == "Select")
            {
                cursorScript.OnSelect(context);

                canStartMatch = joinedPlayers.Count(p => p.SelectedCharacterPrefab != null) == MaxPlayerCount;

                startLabel.gameObject.SetActive(canStartMatch);
            }

            if (context.action.name == "Start" && canStartMatch)
            {
                canStartMatch = false;

                SceneManager.LoadScene("CombatScene");
            }
        };
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        // var player = playerInput.GetComponent<PlayerHandler>();
        
        // joinedPlayers.Remove(player);

        // canStartMatch = false;

        // startLabel.enabled = false;
    }
}