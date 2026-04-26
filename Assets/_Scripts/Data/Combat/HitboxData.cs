namespace Data.Combat
{
    public struct HitboxData
    {
        // Circle collider
        public LogicCollider Collider;
        
        public int Id;
        
        public FixedFloat Damage;

        public FixedVector2 LaunchDirection;

        public FixedFloat FixedKnockback;

        public FixedFloat BaseKnockback;
    }
}