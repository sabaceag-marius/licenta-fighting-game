
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using Data;
using UnityEngine;

namespace Core.Networking
{
    public class NetworkManager : IDisposable
    {
        public ConcurrentQueue<NetworkPacket> IncomingPackets = new ConcurrentQueue<NetworkPacket>();
        private ConcurrentQueue<NetworkPacket> OutgoingPackets = new ConcurrentQueue<NetworkPacket>();

        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;

        private Thread sendThread;
        private Thread receiveThread;

        private volatile bool isRunning = false;

        public void Start(int localPort, string remoteIP, int remotePort)
        {
            Debug.Log($"Starting connection localhost:{localPort}");
            
            udpClient = new UdpClient(localPort);
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteIP), remotePort);
            isRunning = true;

            sendThread = new Thread(SendLoop)
            {
                IsBackground = true
            };

            receiveThread = new Thread(ReceiveLoop)
            {
                IsBackground = true
            };

            sendThread.Start();
            receiveThread.Start();
        }

        public void SendPacket(NetworkPacket packet)
        {
            OutgoingPackets.Enqueue(packet);
        }

        private void SendLoop()
        {
            byte[] outboundBuffer = new byte[PacketSerializer.PACKET_SIZE];

            while (isRunning)
            {
                if (OutgoingPackets.TryDequeue(out NetworkPacket packetToSend))
                {
                    PacketSerializer.Serialize(packetToSend, outboundBuffer);

                    udpClient.Send(outboundBuffer, outboundBuffer.Length, remoteEndPoint);
                }
                else
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void ReceiveLoop()
        {
            IPEndPoint senderEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (isRunning)
            {
                try
                {
                    byte[] receivedBytes = udpClient.Receive(ref senderEndPoint);

                    NetworkPacket packet = PacketSerializer.Deserialize(receivedBytes);

                    IncomingPackets.Enqueue(packet);
                }
                catch (SocketException)
                {
                    if (!isRunning)
                        break;
                }
            }
        }

        public void Dispose()
        {
            isRunning = false;
            udpClient?.Close();

            sendThread?.Join(500);
            receiveThread?.Join(500);
        }
    }
}