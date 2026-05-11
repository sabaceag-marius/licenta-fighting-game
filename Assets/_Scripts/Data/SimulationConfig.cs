using System;

namespace Data
{
    [Serializable]
    public struct SimulationConfig
    {
        public int TargetFPS;
        public int MinutesPerMatch;
        public int InputDelay;
        public int BufferSize;
    }
}