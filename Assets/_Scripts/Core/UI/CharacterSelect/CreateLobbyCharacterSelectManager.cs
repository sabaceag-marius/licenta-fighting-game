
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Codice.Client.BaseCommands.Merge.Xml;
using Core;
using Core.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreateLobbyCharacterSelectManager : BaseCharacterSelectManager
{
    private volatile bool changeScene = false;
    private LocalLobbyManager lobby = new LocalLobbyManager();

    protected virtual void Update()
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
            
            lobby.StartHost(NetworkingDefaults.HOST_PORT, SelectedCharacter);
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

        NetworkConfig.IPAddress = NetworkingDefaults.LOCALHOST;
        NetworkConfig.LocalPort = NetworkingDefaults.HOST_PORT;
        NetworkConfig.RemotePort = NetworkingDefaults.CLIENT_PORT;
        NetworkConfig.LocalPlayerId = 0;

        changeScene = true;
    }

    void OnDestroy()
    {
        lobby.OnOpponentFound -= HandleOpponentFound;
        lobby.OnConnectionComplete -= StartMatch;

        lobby.Dispose();
    }
}