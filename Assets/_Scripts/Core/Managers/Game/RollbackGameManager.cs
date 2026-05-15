
using UnityEngine;

namespace Core
{
    public class RollbackGameManager : BaseGameManager
    {
        private int localPlayerId;

        private Networking.NetworkManager networkManager;

        protected override void Start()
        {
            base.Start();

            localPlayerId = NetworkConfig.LocalPlayerId;
            
            networkManager = new Networking.NetworkManager();
            networkManager.Start(NetworkConfig.ActiveClient, NetworkConfig.IPAddress, NetworkConfig.RemotePort);
        }

        protected override bool ShouldTickAccumulator()
        {
            int executionFrame = logicEngine.CurrentTick + config.InputDelay;
            int trueAdvantage = logicEngine.GetTrueFrameAdvantage(executionFrame);

            // If we are more than 2 frames ahead of the opponent, freeze the logic tick!
            if (trueAdvantage > 2)
            {
                return false; 
            }
            return true;
        }

        protected override void GatherLocalInput()
        {
            lock (inputLock)
            {
                threadInput[localPlayerId] = characters[localPlayerId].GetRawInput();
            }
        }

        protected override void GetSimulationInput(ref RawInput[] simulationInput)
        {
            lock (inputLock)
            {
                ushort executionFrame = (ushort)(logicEngine.CurrentTick + config.InputDelay);

                RawInput localInput = threadInput[localPlayerId];

                localInput.FrameId = executionFrame;
                localInput.IsConfirmed = true;

                simulationInput[localPlayerId] = localInput;
 
                SendNetworkPacket(executionFrame, localInput);
            }
        }

        protected override void ProcessBackgroundTasks()
        {
            while (networkManager.IncomingPackets.TryDequeue(out Data.NetworkPacket result))
            {
                // Debug.Log($"Got packet for frame {result.LatestExecutionFrame}; RawAdvantage: {result.RawAdvantage} ");

                logicEngine.ReceiveNetworkPacket(result);
            } 

            // Its rollback time, rollback all over the place
            if (logicEngine.OldestDesyncFrame != -1)
            {
                Debug.Log($"Rollback {logicEngine.CurrentTick - logicEngine.OldestDesyncFrame} frames");
                logicEngine.ProcessRollback();
            }
        }

        private void SendNetworkPacket(ushort executionFrame, RawInput currentInput)
        {
            Data.NetworkPacket packet = new Data.NetworkPacket
                {
                    PlayerId = (byte)localPlayerId,
                    LatestExecutionFrame = executionFrame,
                    RawAdvantage = (sbyte)logicEngine.GetLocalRawAdvantage(executionFrame),
                    Inputs = new RawInput[Networking.NetworkUtils.REDUNDANCY_COUNT]
                };

                for (int i = 0; i < Networking.NetworkUtils.REDUNDANCY_COUNT; i++)
                {
                    int targetFrame = executionFrame - i;

                    if (targetFrame >= 0)
                    {
                        packet.Inputs[i] = logicEngine.InputBuffer[localPlayerId][targetFrame % config.BufferSize];
                    }
                }

                // Override the local input as it is not inside the logic engine yet
                packet.Inputs[0] = currentInput;

                networkManager.SendPacket(packet);
        }

        void OnDestroy()
        {
            networkManager?.Dispose();
        }
    }
}