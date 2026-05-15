
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Core.Networking
{
    public class MultiplayerLobbyManager : IDisposable
    {
        public enum NetworkState { Disconnected, Hosting, Connecting, Connected,
            ConnectingToServer,
            HolePunching
        }
        public enum MessageType : byte { JoinRequest = 0, JoinAccept = 1, ReadyConfirm = 2 }
        
        private UdpClient udpClient;
        private IPEndPoint lobbyServerEndPoint;
        private IPEndPoint remoteEndPoint;
        private Thread receiveThread;

        private volatile NetworkState state = NetworkState.Disconnected;
        public NetworkState State => state;

        public event Action<CharacterType> OnOpponentFound;
        public event Action OnConnectionComplete;
        
        private string lobbyCode;
        private CharacterType myCharacter;
        private CharacterType opponentCharacter;
        private bool isHost;

        private float resendTimer = 0f;
        private const float RESEND_INTERVAL = 0.2f;
        private volatile bool pendingTransition = false;


        public void StartMatchMaking(string lobbyCode, bool isHost, CharacterType characterType)
        {
            lobbyServerEndPoint = new IPEndPoint(IPAddress.Parse(NetworkingDefaults.LOBBY_SERVER_IP), NetworkingDefaults.LOBBY_SERVER_PORT);    
            this.lobbyCode = lobbyCode;
            this.isHost = isHost;
            this.myCharacter = characterType;

            IPEndPoint localIpv4 = new IPEndPoint(IPAddress.Any, 0);
            udpClient = new UdpClient(localIpv4);
            
            state = NetworkState.ConnectingToServer;

            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();
        }

        public void Tick(float deltaTime)
        {
            if (pendingTransition)
            {
                pendingTransition = false;
                OnConnectionComplete?.Invoke();
            }

            if (state == NetworkState.ConnectingToServer)
            {
                resendTimer += deltaTime;

                if (resendTimer >= RESEND_INTERVAL)
                {
                    string command = isHost ? "HOST" : "JOIN";

                    SendSignalingServer(command);

                    resendTimer = 0;
                }
            }
            else if (state == NetworkState.HolePunching)
            {
                resendTimer += deltaTime;

                if (resendTimer >= RESEND_INTERVAL)
                {
                    // Blast UDP to the opponent to punch the NAT firewall
                    SendP2P(MessageType.JoinRequest);
                    resendTimer = 0;
                }
            }
        }

        private void ReceiveLoop()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            while (state != NetworkState.Disconnected)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref sender);

                    // Connecting to the signaling server
                    if (state == NetworkState.ConnectingToServer)
                    {
                        // Expecting string from Signaling Server
                        string msg = Encoding.UTF8.GetString(data);
                        if (msg.StartsWith("MATCH"))
                        {
                            string[] parts = msg.Split('|');
                            remoteEndPoint = new IPEndPoint(IPAddress.Parse(parts[1]), int.Parse(parts[2]));
                            
                            // Start Hole Punching
                            state = NetworkState.HolePunching;
                            resendTimer = RESEND_INTERVAL; // Trigger immediate P2P ping
                        }
                    }
                    else if (state == NetworkState.HolePunching || state == NetworkState.Connected)
                    {
                        // Only process P2P packets from the opponent
                        if (!sender.Equals(remoteEndPoint) || data.Length < 2) continue;

                        MessageType type = (MessageType)data[0];
                        CharacterType remoteChar = (CharacterType)data[1];

                        HandleP2PMessage(type, remoteChar);
                    }
                }
                catch
                { 
                    // log to unity
                    break; 
                }
            }
        }

        private void HandleP2PMessage(MessageType type, CharacterType remoteChar)
        {
            // Symmetrical Handshake. Both clients act identically here.
            if (type == MessageType.JoinRequest)
            {
                opponentCharacter = remoteChar;
                OnOpponentFound?.Invoke(opponentCharacter);
                SendP2P(MessageType.JoinAccept);
            }
            else if (type == MessageType.JoinAccept)
            {
                opponentCharacter = remoteChar;
                OnOpponentFound?.Invoke(opponentCharacter);
                SendP2P(MessageType.ReadyConfirm);
                
                if (state != NetworkState.Connected)
                {
                    state = NetworkState.Connected;
                    pendingTransition = true;
                }
            }
            else if (type == MessageType.ReadyConfirm)
            {
                if (state != NetworkState.Connected)
                {
                    state = NetworkState.Connected;
                    pendingTransition = true;
                }
            }
        }

        

        private void SendSignalingServer(string command)
        {
            byte[] packet = Encoding.ASCII.GetBytes(command + "|" + lobbyCode);

            try
            {
                udpClient.Send(packet, packet.Length, lobbyServerEndPoint);
            }
            catch (Exception e)
            {
                // write to unity
            }
        }

        private void SendP2P(MessageType type)
        {
            byte[] packet = { (byte)type, (byte)myCharacter };
            
            try
            {
                udpClient.Send(packet, packet.Length, remoteEndPoint);
            }
            catch (Exception e)
            {
                // write to unity
            }
        }

        public void ReleaseSocketOwnership()
        {
            udpClient = null;
        }

        public (UdpClient, IPEndPoint, CharacterType) GetConnectionData() 
        => (udpClient, remoteEndPoint, opponentCharacter);

        public void Dispose()
        {
            SendSignalingServer("CANCEL");

            state = NetworkState.Disconnected;
            udpClient?.Close();
            receiveThread?.Join(100);
        }
    }
}