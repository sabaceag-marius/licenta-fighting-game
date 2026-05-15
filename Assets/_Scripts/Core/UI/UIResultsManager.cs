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

        // Display the stats
        p1DamageText.text = $"P1 Score: {MatchResultsData.Player1TotalDamageDealt:0}";
        p2DamageText.text = $"P2 Score: {MatchResultsData.Player2TotalDamageDealt:0}";

        activePlayers = FindObjectsOfType<PlayerInput>();

        foreach (PlayerInput player in activePlayers)
        {
            // Force their controllers back into UI mode so they stop swinging/moving
            player.SwitchCurrentActionMap("UI Overlay");
        }
    }

    void Update()
    {
        // Don't listen for inputs if we are already loading a scene
        if (isTransitioning || activePlayers == null) return;

        // --- 3. LISTEN FOR START BUTTON ---
        // Let ANY player press Start to go back to the character select screen
        foreach (PlayerInput player in activePlayers)
        {
            // Ensure "Start" matches the exact name of the action in your Input Asset
            if (player.actions["Start"].WasPressedThisFrame())
            {
                OnCharacterSelectPressed();
                break; // Exit the loop instantly so we don't trigger it twice
            }
        }
    }

    // Link this to a "Character Select" button in your UI
    public void OnCharacterSelectPressed()
    {
        MatchResultsData.ResetData();

        SceneManager.LoadScene("MainMenuScene");
    }
}