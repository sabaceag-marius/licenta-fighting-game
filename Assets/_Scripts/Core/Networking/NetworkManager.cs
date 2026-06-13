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

        int packetLossPercentage;
        int minPacketDelay;        
        int maxPacketDelay;

        object networkTestLock = new object();        
        #endregion

        public ConcurrentQueue<NetworkPacket> IncomingPackets = new ConcurrentQueue<NetworkPacket>();
        private ConcurrentQueue<NetworkPacket> OutgoingPackets = new ConcurrentQueue<NetworkPacket>();

        private UdpClient udpClient;
        private IPEndPoint remoteEndPoint;

        private Thread sendThread;
        private Thread receiveThread;

        private volatile bool isRunning = false;

        public volatile bool HasDisconnected = false;
        public string DisconnectReason { get; private set; } = string.Empty;

        public void Start(UdpClient udpClient, IPAddress remoteIP, int remotePort)
        {
            this.udpClient = udpClient;
            remoteEndPoint = new IPEndPoint(remoteIP, remotePort);
            
            this.udpClient.Client.ReceiveTimeout = 3000;

            isRunning = true;
            HasDisconnected = false;

            sendThread = new Thread(SendLoop) { IsBackground = true };
            receiveThread = new Thread(ReceiveLoop) { IsBackground = true };

            sendThread.Start();
            receiveThread.Start();
        }

        public void SendPacket(NetworkPacket packet)
        {
            if (!isRunning) return;
            OutgoingPackets.Enqueue(packet);
        }

        private void TriggerDisconnect(string reason, Exception ex)
        {
            // Prevent double-triggering
            if (HasDisconnected) return; 

            HasDisconnected = true;
            isRunning = false;

            DisconnectReason = $"{reason} | {ex.GetType().Name}: {ex.Message}";
        }

        private void SendLoop()
        {
            System.Random rng = new System.Random();
            byte[] outboundBuffer = new byte[NetworkUtils.PACKET_SIZE];
            System.Diagnostics.Stopwatch delayStopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (isRunning)
            {
                try
                {
                    while (OutgoingPackets.TryDequeue(out NetworkPacket packetToSend))
                    {
                        int dropChance = rng.Next(100);
                        if (dropChance <= packetLossPercentage) continue;

                        int delay = rng.Next(minPacketDelay, maxPacketDelay);

                        delayStagingQueue.Enqueue(new DelayedPacket
                        {
                            Packet = packetToSend,
                            SendTimeMs = delayStopwatch.ElapsedMilliseconds + delay
                        });
                    }

                    // Look at the oldest packet. If enough time has passed, send it!

                    while (delayStagingQueue.Count > 0 && delayStopwatch.ElapsedMilliseconds >= delayStagingQueue.Peek().SendTimeMs)
                    {
                        DelayedPacket readyPacket = delayStagingQueue.Dequeue();
                        NetworkUtils.Serialize(readyPacket.Packet, outboundBuffer);
                        
                        udpClient.Send(outboundBuffer, outboundBuffer.Length, remoteEndPoint);
                    }
                }
                catch (Exception ex)
                {
                    if (isRunning) TriggerDisconnect("Failed to send packet to opponent", ex);
                    break;
                }

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
                    NetworkPacket packet = NetworkUtils.Deserialize(receivedBytes);
                    IncomingPackets.Enqueue(packet);
                }
                catch (SocketException ex)
                {
                    if (!isRunning) break;

                    // Differentiate between a Timeout and a Crash ---
                    if (ex.SocketErrorCode == SocketError.TimedOut)
                    {
                        TriggerDisconnect("Connection timed out. No packets received for 3 seconds.", ex);
                    }
                    else
                    {
                        TriggerDisconnect("Socket connection reset or closed by remote host.", ex);
                    }
                    break;
                }
                catch (Exception ex)
                {
                    if (isRunning) TriggerDisconnect("Critical error in receive thread", ex);
                    break;
                }
            }
        }

        public void SetNetworkDebugVariables(int packetLossPercentage, int minPacketDelay, int maxPacketDelay)
        {
            lock (networkTestLock)
            {
                this.packetLossPercentage = packetLossPercentage;
                this.minPacketDelay = minPacketDelay;
                this.maxPacketDelay = maxPacketDelay;
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