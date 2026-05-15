
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core;
using Core.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JoinLobbyCharacterSelectManager : BaseCharacterSelectManager
{
    [SerializeField]
    private TMP_InputField minDelayInput;

    [SerializeField]
    private TMP_InputField maxDelayInput;

    [SerializeField]
    private TMP_InputField packetLossInput;
    
    private volatile bool changeScene = false;
    private LocalLobbyManager lobby = new LocalLobbyManager();

    protected virtual void Update()
    {
        lobby.Tick(Time.deltaTime);

        if (changeScene)
        {
            RemotePlayerHandler remotePlayerHandler = Instantiate(remotePlayerPrefab).GetComponent<RemotePlayerHandler>();

            remotePlayerHandler.InitializeRemote(0);

            CharacterType characterType = lobby.GetConnectionData().Item3;

            if ((int) characterType < characterPool.Length)
            {
                remotePlayerHandler.SetCharacter(characterPool[(int)characterType]);
            }

            int.TryParse(minDelayInput?.text, out int minDelay);
            int.TryParse(maxDelayInput?.text, out int maxDelay);
            int.TryParse(packetLossInput?.text, out int packetLoss);

            NetworkConfig.MinArtificialDelay = minDelay;
            NetworkConfig.MaxArtificialDelay = maxDelay;
            NetworkConfig.PacketLossPercentage = packetLoss;

            SceneManager.LoadScene("MultiplayerCombatScene");
        }
    }

    public override void TryStartMatch()
    {
        if (canStartMatch)
        {
            startLabel.SetText("Waiting for connection...");

            canStartMatch = false;

            lobby.OnConnectionComplete += StartMatch;
        
            // Start searching/sending requests
            lobby.StartClient(NetworkingDefaults.LOCALHOST, NetworkingDefaults.HOST_PORT, NetworkingDefaults.CLIENT_PORT, SelectedCharacter);
        }
    }

    private void StartMatch()
    {
        // startLabel.SetText("Found connection!");

        joinedPlayers[0].PlayerIndex = 1;

        NetworkConfig.IPAddress = NetworkingDefaults.LOCALHOST;
        NetworkConfig.LocalPort = NetworkingDefaults.CLIENT_PORT;
        NetworkConfig.RemotePort = NetworkingDefaults.HOST_PORT;
        NetworkConfig.LocalPlayerId = 1;

        changeScene = true;
    }

    void OnDestroy()
    {
        lobby.OnConnectionComplete -= StartMatch;

        lobby.Dispose();
    }
}