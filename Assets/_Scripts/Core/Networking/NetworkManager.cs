
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Concurrent;
using Data;
using UnityEngine;
using System.Collections.Generic;

namespace Core.Networking
{
    public class NetworkManager : IDisposable
    {
        #region Testing delay

        private struct DelayedPacket
        {
            public NetworkPacket Packet;
            public long SendTimeMs;
        }

        private Queue<DelayedPacket> delayStagingQueue = new Queue<DelayedPacket>();

        #endregion

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
            System.Random rng = new System.Random();

            byte[] outboundBuffer = new byte[PacketSerializer.PACKET_SIZE];

            // Stopwatch to track the time for delay Testing
            
            System.Diagnostics.Stopwatch dekayStopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (isRunning)
            {
                // Pull everything from the game thread as soon as they are generated

                while (OutgoingPackets.TryDequeue(out NetworkPacket packetToSend))
                {
                    int dropChance = rng.Next(100);

                    if (dropChance <= NetworkConfig.PacketLossPercentage)
                        continue;

                    int delay = rng.Next(NetworkConfig.MinArtificialDelay, NetworkConfig.MaxArtificialDelay);

                    delayStagingQueue.Enqueue(new DelayedPacket
                    {
                        Packet = packetToSend,
                        SendTimeMs = dekayStopwatch.ElapsedMilliseconds + delay
                    });
                }

                // Look at the oldest packet. If enough time has passed, send it!

                while (delayStagingQueue.Count > 0 && dekayStopwatch.ElapsedMilliseconds >= delayStagingQueue.Peek().SendTimeMs)
                {
                    DelayedPacket readyPacket = delayStagingQueue.Dequeue();
                    
                    PacketSerializer.Serialize(readyPacket.Packet, outboundBuffer);
                    udpClient.Send(outboundBuffer, outboundBuffer.Length, remoteEndPoint);
                }

                // Sleep for 1ms to prevent 100% CPU core usage. 
                Thread.Sleep(1);
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