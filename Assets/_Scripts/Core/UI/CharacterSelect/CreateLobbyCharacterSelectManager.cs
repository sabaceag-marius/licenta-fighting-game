
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core;
using Core.Networking;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateLobbyCharacterSelectManager : BaseCharacterSelectManager
{
    [SerializeField]
    private TMP_InputField minDelayInput;

    [SerializeField]
    private TMP_InputField maxDelayInput;

    [SerializeField]
    private TMP_InputField packetLossInput;

    [SerializeField]
    private TMP_Text lobbyLabel;

    private volatile bool changeScene = false;
    private MultiplayerLobbyManager lobby = new MultiplayerLobbyManager();

    private string lobbyCode;

    protected override void Start()
    {
        base.Start();

        var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var stringChars = new char[6];

        for (int i = 0; i < stringChars.Length; i++)
        {
            stringChars[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        }

        lobbyCode = new String(stringChars);

        lobbyLabel.SetText(lobbyCode);
    }
    void Update()
    {
        lobby.Tick(Time.deltaTime);

        if (changeScene)
        {
            RemotePlayerHandler remotePlayerHandler = Instantiate(remotePlayerPrefab).GetComponent<RemotePlayerHandler>();

            remotePlayerHandler.InitializeRemote(1);

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

            lobby.OnOpponentFound += HandleOpponentFound;
            lobby.OnConnectionComplete += StartMatch;
            
            // lobby.StartHost(lobbyCode, SelectedCharacter);
            lobby.StartMatchMaking(lobbyCode, true, SelectedCharacter);
        }
    }

    private void HandleOpponentFound(CharacterType opponentCharacter)
    {
        Debug.Log("Player joined!");
    }

    private void StartMatch()
    {
        // startLabel.SetText("Found connection!");

        joinedPlayers[0].PlayerIndex = 0;

        (UdpClient, IPEndPoint, CharacterType) connectionData = lobby.GetConnectionData();

        NetworkConfig.ActiveClient = connectionData.Item1;

        NetworkConfig.IPAddress = connectionData.Item2.Address;
        NetworkConfig.RemotePort = connectionData.Item2.Port;
        NetworkConfig.LocalPlayerId = 0;

        lobby.ReleaseSocketOwnership();

        changeScene = true;
    }

    void OnDestroy()
    {
        lobby.OnOpponentFound -= HandleOpponentFound;
        lobby.OnConnectionComplete -= StartMatch;

        lobby.Dispose();
    }
}