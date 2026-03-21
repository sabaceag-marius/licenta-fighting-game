
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

    public bool IsGrounded;
}