using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class UIResultsManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI p1DamageText;
    public TextMeshProUGUI p2DamageText;

    private PlayerInput[] activePlayers;
    
    private bool isTransitioning = false;

    void Start()
    {
        // Read the global data that was saved in the previous scene
        int winner = MatchResultsData.WinnerPlayer;

        if (winner == 1)
        {
            winnerText.text = "PLAYER 1 WINS!";
            winnerText.color = Color.red;
        }
        else if (winner == 2)
        {
            winnerText.text = "PLAYER 2 WINS!";
            winnerText.color = Color.blue;
        }
        else
        {
            winnerText.text = "DRAW!";
            winnerText.color = Color.gray;
        }

        p1DamageText.text = $"P1 Score: {MatchResultsData.Player1TotalDamageDealt:0}";
        p2DamageText.text = $"P2 Score: {MatchResultsData.Player2TotalDamageDealt:0}";

        activePlayers = FindObjectsOfType<PlayerInput>();

        foreach (PlayerInput player in activePlayers)
        {
            player.SwitchCurrentActionMap("UI Overlay");
        }
    }

    void Update()
    {
        if (isTransitioning || activePlayers == null) return;

        foreach (PlayerInput player in activePlayers)
        {
            if (player.actions["Start"].WasPressedThisFrame())
            {
                OnCharacterSelectPressed();
                break;
            }
        }
    }

    public void OnCharacterSelectPressed()
    {
        MatchResultsData.ResetData();

        SceneManager.LoadScene("MainMenuScene");
    }
}