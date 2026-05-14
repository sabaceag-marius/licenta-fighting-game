
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

namespace Core.Networking
{
    public class LocalLobbyManager : IDisposable
    {
        public enum NetworkState { Disconnected, Hosting, Connecting, Connected }
        public enum MessageType : byte { JoinRequest = 0, JoinAccept = 1, ReadyConfirm = 2 }
        
        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;
        private Thread receiveThread;

        private volatile NetworkState state = NetworkState.Disconnected;
        public NetworkState State => state;

        public event Action<CharacterType> OnOpponentFound;
        public event Action OnConnectionComplete;
        
        private CharacterType myCharacter;
        private CharacterType opponentCharacter;
        private float resendTimer = 0f;
        private const float RESEND_INTERVAL = 0.2f;

        public void StartHost(int port, CharacterType myChar)
        {
            myCharacter = myChar;
            udpClient = new UdpClient(port);
            state = NetworkState.Hosting;
            StartListening();
        }

        public void StartClient(string ip, int hostPort, int port, CharacterType myChar)
        {
            myCharacter = myChar;
            udpClient = new UdpClient(port);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(ip), hostPort);
            state = NetworkState.Connecting;
            StartListening();
        }

        private void StartListening()
        {
            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
            receiveThread.Start();
        }

        private void ReceiveLoop()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            while (state != NetworkState.Disconnected)
            {
                try
                {
                    byte[] data = udpClient.Receive(ref sender);
                    if (data.Length < 2) continue;

                    MessageType type = (MessageType)data[0];
                    CharacterType remoteChar = (CharacterType)data[1];

                    Debug.Log($"Received message of type {type}");

                    HandleMessage(type, remoteChar, sender);
                }
                catch
                { 
                    break; 
                }
            }
        }

        private void HandleMessage(MessageType type, CharacterType remoteChar, IPEndPoint sender)
        {
            if (state == NetworkState.Hosting && type == MessageType.JoinRequest)
            {
                remoteEndPoint = sender;
                opponentCharacter = remoteChar;
                OnOpponentFound?.Invoke(opponentCharacter);
                Send(MessageType.JoinAccept);
            }
            else if (state == NetworkState.Hosting && type == MessageType.ReadyConfirm)
            {
                state = NetworkState.Connected;
                OnConnectionComplete?.Invoke();
            }
            else if (state == NetworkState.Connecting && type == MessageType.JoinAccept)
            {
                opponentCharacter = remoteChar;
                OnOpponentFound?.Invoke(opponentCharacter);
                Send(MessageType.ReadyConfirm);
                state = NetworkState.Connected;
                OnConnectionComplete?.Invoke();
            }
        }

        public void Tick(float deltaTime)
        {
            if (state == NetworkState.Connecting)
            {
                resendTimer += deltaTime;
                if (resendTimer >= RESEND_INTERVAL)
                {
                    Send(MessageType.JoinRequest);
                    resendTimer = 0f;
                }
            }
        }

        private void Send(MessageType type)
        {
            byte[] packet = { (byte)type, (byte)myCharacter };
            udpClient?.Send(packet, packet.Length, remoteEndPoint);
        }

        public (UdpClient, IPEndPoint, CharacterType) GetConnectionData() 
        => (udpClient, remoteEndPoint, opponentCharacter);

        public void Dispose()
        {
            state = NetworkState.Disconnected;
            udpClient?.Close();
            receiveThread?.Join(100);
        }
    }
}