using System;

namespace Data
{
    [Serializable]
    public struct SimulationConfig
    {
        public int TargetFPS;
        public int MinutesPerMatch;
        public ushort InputDelay;
        public int BufferSize;
    }
}