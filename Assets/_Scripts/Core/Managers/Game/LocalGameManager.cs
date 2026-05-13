
namespace Core
{
    public class LocalGameManager : BaseGameManager
    {
        protected override void GatherLocalInput()
        {
            lock (inputLock)
            {
                for (int i = 0; i < characters.Length; i++)
                {
                    threadInput[i] = characters[i].GetRawInput();
                    
                    // For changing the stats while playing
                    // logicEngine.StateBuffer[logicEngine.CurrentTick % config.BufferSize].Characters[i].Stats = characters[i].GetLogicCharacterStats(logicEngine.FixedDeltaTime);
                }
            } 
        }

        protected override void GetSimulationInput(ref RawInput[] simulationInput)
        {
            lock (inputLock)
            {
                for (int i = 0; i < characters.Length; i++)
                {
                    threadInput[i].FrameId = (ushort)(logicEngine.CurrentTick + config.InputDelay);
                    simulationInput[i] = threadInput[i];
                }
            }
        }
    }
}