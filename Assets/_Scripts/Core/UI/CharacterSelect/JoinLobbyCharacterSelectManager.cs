
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
    private TMP_InputField lobbyCodeInput;

    [SerializeField]
    private TMP_InputField minDelayInput;

    [SerializeField]
    private TMP_InputField maxDelayInput;

    [SerializeField]
    private TMP_InputField packetLossInput;
    
    private volatile bool changeScene = false;
    private MultiplayerLobbyManager lobby = new MultiplayerLobbyManager();

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
        if (string.IsNullOrWhiteSpace(lobbyCodeInput?.text))
            return;

        if (canStartMatch)
        {
            startLabel.SetText("Waiting for connection...");

            canStartMatch = false;

            lobby.OnConnectionComplete += StartMatch;

            lobby.StartMatchMaking(lobbyCodeInput.text, false, SelectedCharacter);
        }
    }

    private void StartMatch()
    {
        // startLabel.SetText("Found connection!");

        joinedPlayers[0].PlayerIndex = 1;

        (UdpClient, IPEndPoint, CharacterType) connectionData = lobby.GetConnectionData();

        NetworkConfig.ActiveClient = connectionData.Item1;

        NetworkConfig.IPAddress = connectionData.Item2.Address;
        NetworkConfig.RemotePort = connectionData.Item2.Port;
        NetworkConfig.LocalPlayerId = 1;

        lobby.ReleaseSocketOwnership();

        changeScene = true;
    }

    void OnDestroy()
    {
        lobby.OnConnectionComplete -= StartMatch;

        lobby.Dispose();
    }
}