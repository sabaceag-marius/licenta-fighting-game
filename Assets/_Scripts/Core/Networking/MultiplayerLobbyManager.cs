using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using UnityEngine;

namespace Core.Networking
{
    public class MultiplayerLobbyManager : IDisposable
    {
        public enum NetworkState { Disconnected, Hosting, Connecting, Connected, ConnectingToServer, HolePunching }
        public enum MessageType : byte { JoinRequest = 0, JoinAccept = 1, ReadyConfirm = 2 }
        
        private UdpClient udpClient;
        private IPEndPoint lobbyServerEndPoint;
        
        // The Dual-Ping Candidates
        private IPEndPoint remotePublicEndPoint;
        private IPEndPoint remoteLocalEndPoint;
        
        // The Final Winner
        private IPEndPoint remoteEndPoint; 
        
        private Thread receiveThread;

        // Thread-safe queues for logging and main-thread execution
        private ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        private ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

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
            
            logQueue.Enqueue("Matchmaking thread started.");
        }

        public void Tick(float deltaTime)
        {
            // 1. Process all logs safely on the main thread
            while (logQueue.TryDequeue(out string logMsg))
            {
                Debug.Log($"[LobbyManager] {logMsg}");
            }

            // 2. Process all Unity-API events safely on the main thread
            while (mainThreadActions.TryDequeue(out Action action))
            {
                action.Invoke();
            }

            // 3. Network State Machine
            if (state == NetworkState.ConnectingToServer)
            {
                resendTimer += deltaTime;
                if (resendTimer >= RESEND_INTERVAL)
                {
                    // Get the port the OS assigned to our UDP Client and our Local LAN IP
                    int localPort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                    string localIp = GetLocalIPAddress();

                    string command = isHost ? "HOST" : "JOIN";
                    
                    // Send our Command + Local Routing Data to the Server
                    SendSignalingServer($"{command}|{localIp}|{localPort}");
                    
                    resendTimer = 0;
                }
            }
            else if (state == NetworkState.HolePunching)
            {
                resendTimer += deltaTime;
                if (resendTimer >= RESEND_INTERVAL)
                {
                    // Blast BOTH endpoints! One will fail, one will punch through the NAT/Router.
                    SendP2P(MessageType.JoinRequest, remotePublicEndPoint);
                    SendP2P(MessageType.JoinRequest, remoteLocalEndPoint);
                    
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

                    if (state == NetworkState.ConnectingToServer)
                    {
                        string msg = Encoding.UTF8.GetString(data);
                        if (msg.StartsWith("MATCH"))
                        {
                            string[] parts = msg.Split('|');
                            
                            // FORMAT EXPECTED: MATCH | PublicIP | PublicPort | LocalIP | LocalPort
                            remotePublicEndPoint = new IPEndPoint(IPAddress.Parse(parts[1]), int.Parse(parts[2]));
                            remoteLocalEndPoint = new IPEndPoint(IPAddress.Parse(parts[3]), int.Parse(parts[4]));
                            
                            logQueue.Enqueue($"Signaling successful! Targets -> Public: {remotePublicEndPoint} | Local: {remoteLocalEndPoint}");
                            
                            state = NetworkState.HolePunching;
                            resendTimer = RESEND_INTERVAL; // Trigger immediate P2P ping
                        }
                    }
                    else if (state == NetworkState.HolePunching || state == NetworkState.Connected)
                    {
                        // 1. Compare against both candidates safely using IPv4 Mapping
                        bool matchesPublic = sender.Address.MapToIPv4().Equals(remotePublicEndPoint.Address.MapToIPv4()) && sender.Port == remotePublicEndPoint.Port;
                        bool matchesLocal = sender.Address.MapToIPv4().Equals(remoteLocalEndPoint.Address.MapToIPv4()) && sender.Port == remoteLocalEndPoint.Port;

                        if (!matchesPublic && !matchesLocal)
                        {
                            continue; // Packet is from an unknown/malicious source
                        }

                        // 2. WINNER SELECTION: Lock in the first Endpoint that successfully reached us
                        if (state == NetworkState.HolePunching && remoteEndPoint == null)
                        {
                            remoteEndPoint = matchesPublic ? remotePublicEndPoint : remoteLocalEndPoint;
                            logQueue.Enqueue($"Connection established via {(matchesPublic ? "PUBLIC" : "LOCAL")} EndPoint: {remoteEndPoint}");
                        }

                        if (data.Length < 2) continue;

                        MessageType type = (MessageType)data[0];
                        CharacterType remoteChar = (CharacterType)data[1];

                        HandleP2PMessage(type, remoteChar);
                    }
                }
                catch (SocketException e)
                {
                    if (state != NetworkState.Disconnected)
                        logQueue.Enqueue($"Socket Exception: {e.Message}");
                    break; 
                }
                catch (Exception e)
                {
                    logQueue.Enqueue($"FATAL THREAD ERROR: {e.Message}\n{e.StackTrace}");
                    break;
                }
            }
        }

        private void HandleP2PMessage(MessageType type, CharacterType remoteChar)
        {
            logQueue.Enqueue($"Received P2P Message: {type} from {remoteChar}");

            if (type == MessageType.JoinRequest)
            {
                opponentCharacter = remoteChar;
                
                // Queue the event to run safely on Unity's main thread
                mainThreadActions.Enqueue(() => OnOpponentFound?.Invoke(opponentCharacter));
                
                SendP2P(MessageType.JoinAccept, remoteEndPoint);
            }
            else if (type == MessageType.JoinAccept)
            {
                opponentCharacter = remoteChar;
                mainThreadActions.Enqueue(() => OnOpponentFound?.Invoke(opponentCharacter));
                
                SendP2P(MessageType.ReadyConfirm, remoteEndPoint);
                
                if (state != NetworkState.Connected)
                {
                    state = NetworkState.Connected;
                    mainThreadActions.Enqueue(() => OnConnectionComplete?.Invoke());
                }
            }
            else if (type == MessageType.ReadyConfirm)
            {
                if (state != NetworkState.Connected)
                {
                    state = NetworkState.Connected;
                    mainThreadActions.Enqueue(() => OnConnectionComplete?.Invoke());
                }
            }
        }

        private void SendSignalingServer(string command)
        {
            byte[] packet = Encoding.ASCII.GetBytes(command + "|" + lobbyCode);
            try { udpClient.Send(packet, packet.Length, lobbyServerEndPoint); }
            catch (Exception e) { logQueue.Enqueue($"Send error (Server): {e.Message}"); }
        }

        // Updated to accept a specific target Endpoint so we can Dual-Ping
        private void SendP2P(MessageType type, IPEndPoint targetEndPoint)
        {
            if (targetEndPoint == null) return;
            
            byte[] packet = { (byte)type, (byte)myCharacter };
            try { udpClient.Send(packet, packet.Length, targetEndPoint); }
            catch (Exception e) { logQueue.Enqueue($"Send error (P2P): {e.Message}"); }
        }

        private string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork) // IPv4 only
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        public void ReleaseSocketOwnership() { udpClient = null; }

        public (UdpClient, IPEndPoint, CharacterType) GetConnectionData() => (udpClient, remoteEndPoint, opponentCharacter);

        public void Dispose()
        {
            if (state != NetworkState.Disconnected)
                SendSignalingServer("CANCEL");

            state = NetworkState.Disconnected;
            udpClient?.Close();
            receiveThread?.Join(100);
        }
    }
}