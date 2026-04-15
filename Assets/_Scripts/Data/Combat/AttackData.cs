
namespace Data.Combat
{
    public struct AttackData
    {
        public AttackType Type;

        public int FrameCount;

        public int TotalDurationFrames;

        public bool OverrideHurtboxes;

        public FrameData[] Frames;
    }
}