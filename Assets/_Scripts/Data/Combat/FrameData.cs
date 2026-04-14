namespace Data.Combat
{
    public struct FrameData
    {
        public int HitboxCount;

        public HitboxData[] Hitboxes;

        public int HurtboxCount;

        public HurtboxData[] Hurtboxes;

        public LogicBox HurtboxesBoundingBox;
    }
}