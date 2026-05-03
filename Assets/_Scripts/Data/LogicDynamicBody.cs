
public struct LogicDynamicBody
{
    public FixedVector2 Position
    {
        get => Collider.Position;

        set
        {
            Collider.Position = value;
        }
    }

    public LogicCollider Collider;

    public FixedVector2 Velocity;

    public FixedVector2 ExternalVelocity;

    public bool IsGrounded;
    
    public bool HitFloor;

    public bool HitCeiling;
    
    public bool HitWall;

    public FixedVector2 ExternalVelocityAtImpact;
}