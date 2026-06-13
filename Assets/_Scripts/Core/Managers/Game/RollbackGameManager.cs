
using UnityEngine;
using UnityEngine.SceneManagement;

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
            
            SetNetworkDebugVariables(NetworkConfig.PacketLossPercentage, NetworkConfig.MinArtificialDelay, NetworkConfig.MaxArtificialDelay);

            networkManager = new Networking.NetworkManager();
            networkManager.Start(NetworkConfig.ActiveClient, NetworkConfig.IPAddress, NetworkConfig.RemotePort);
        }

        protected override bool ShouldTickAccumulator()
        {
            if (logicEngine.TotalMatchFrames - logicEngine.CurrentTick < 5)
            {
                return true;
            }
            
            int executionFrame = logicEngine.CurrentTick + config.InputDelay;
            int trueAdvantage = logicEngine.GetTrueFrameAdvantage(executionFrame);

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
                logicEngine.ReceiveNetworkPacket(result);
            } 

            // Its rollback time, rollback all over the place
            if (logicEngine.OldestDesyncFrame != -1)
            {
                Debug.Log($"Rollback {logicEngine.CurrentTick - logicEngine.OldestDesyncFrame} frames");
                logicEngine.ProcessRollback();

                var gameState = logicEngine.GetCurrentGameState();
                
                for (int i = 0; i < characters.Length; i++)
                {
                    characters[i].QueueSnapPosition(gameState.Characters[i].Position);
                }
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

        protected override void Update()
        {
            if (networkManager != null && networkManager.HasDisconnected)
            {
                Debug.LogError($"[NetworkManager Fatal Error] {networkManager.DisconnectReason}");

                SceneManager.LoadScene("DesyncScene");
                return;    
            }   

            HandleDebugInputs();

            base.Update();
        }

        private void SetNetworkDebugVariables(int minPacketDelay, int maxPacketDelay, int packetLossPercentage)
        {
            if (networkManager == null)
                return;

            networkManager.SetNetworkDebugVariables(packetLossPercentage, minPacketDelay, maxPacketDelay);

            UI.MatchEventBus.OnNetworkDebugUpdated?.Invoke(packetLossPercentage, minPacketDelay, maxPacketDelay);
        }

        private void HandleDebugInputs()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null)
                return;

            if (UnityEngine.InputSystem.Keyboard.current.digit0Key.wasPressedThisFrame)
            {
                ShowHitboxes = !ShowHitboxes;
            }

            if (UnityEngine.InputSystem.Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                SetNetworkDebugVariables(0, 0, 0);
            }

            if (UnityEngine.InputSystem.Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                SetNetworkDebugVariables(50, 60, 0);
            }

            if (UnityEngine.InputSystem.Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                SetNetworkDebugVariables(25, 50, 25);
            }

            if (UnityEngine.InputSystem.Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                SetNetworkDebugVariables(130, 160, 10);
            }

            if (UnityEngine.InputSystem.Keyboard.current.digit5Key.wasPressedThisFrame)
            {
                SetNetworkDebugVariables(150, 250, 20);
            }

            if (UnityEngine.InputSystem.Keyboard.current.qKey.wasPressedThisFrame)
            {
                SceneManager.LoadScene("MainMenuScene");
            }
        }

        void OnDestroy()
        {
            networkManager?.Dispose();
        }
    }
}