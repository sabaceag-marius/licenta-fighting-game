
namespace Data
{
    public struct NetworkPacket
    {
        public byte PlayerId;

        public ushort LatestExecutionFrame;

        public sbyte RawAdvantage;

        public RawInput[] Inputs;
    }
}