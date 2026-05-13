
using UnityEngine;

namespace Core
{
    public class RollbackGameManager : BaseGameManager
    {
        private int localPlayerId = 0;

        private Networking.NetworkManager networkManager;

        protected override void Start()
        {
            base.Start();
            
            networkManager = new Networking.NetworkManager();
            networkManager.Start(NetworkConfig.LocalPort, NetworkConfig.IPAddress, NetworkConfig.RemotePort);
        }

        protected override void GatherLocalInput()
        {
            lock (inputLock)
            {
                threadInput[localPlayerId] = characters[localPlayerId].GetRawInput();
            }

            if (networkManager.IncomingPackets.TryDequeue(out Data.NetworkPacket result))
            {
                Debug.Log($"Got packet for latest frame {result.LatestExecutionFrame}; size = {result.Inputs.Length}; ({result.Inputs[0].LeftStickX}, {result.Inputs[0].LeftStickY})");
            } 
        }

        protected override void GetSimulationInput(ref RawInput[] simulationInput)
        {
            lock (inputLock)
            {
                ushort executionFrame = (ushort)(logicEngine.CurrentTick + config.InputDelay);

                RawInput localInput = threadInput[localPlayerId];

                localInput.FrameId = executionFrame;

                simulationInput[localPlayerId] = localInput;
 
                Data.NetworkPacket packet = new Data.NetworkPacket
                {
                    PlayerId = (byte)localPlayerId,
                    LatestExecutionFrame = executionFrame,
                    RawAdvantage = 0,
                    Inputs = new RawInput[Networking.PacketSerializer.REDUNDANCY_COUNT]
                };

                for (int i = 0; i < Networking.PacketSerializer.REDUNDANCY_COUNT; i++)
                {
                    int targetFrame = executionFrame - i;

                    if (targetFrame >= 0)
                    {
                        packet.Inputs[i] = logicEngine.InputBuffer[localPlayerId][targetFrame % config.BufferSize];
                    }
                }

                // Override the local input as it is not inside the logic engine yet
                packet.Inputs[0] = localInput;

                networkManager.SendPacket(packet);
            }
        }

        protected override void ProcessBackgroundTasks()
        {
            
        }
    }
}