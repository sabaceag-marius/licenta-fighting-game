
public enum ColliderType
{
    Box,
    Circle,
    Capsule
}

public struct LogicCollider
{
    // Add layer stuff here

    public ColliderType Type;

    public bool IsTrigger;

    public LogicBox BoundingBox;

    public FixedVector2 Position;

    /// <summary>
    /// [CircleCollider]: Radius of the circle; 
    /// [CapsuleCollider]: The thickness of the capsule
    /// </summary>

    public FixedFloat Radius;

    /// <summary>
    /// [CapsuleCollider]: The distance from the center to the top / bottom of the capsule
    /// </summary>
    
    public FixedFloat HalfInnerLength;

    /// <summary>
    /// [BoxCollider]: Half-width (x) and Half-height (y)
    /// </summary>
    
    public FixedVector2 Extents;

    /// <summary>
    /// [CapsuleCollider]: Rotation vector. Represents where Vector2.Up is for this collider
    /// </summary>
    
    public FixedVector2 Direction;


    // BoxCollider specifics

    public FixedFloat Top => Position.y + Extents.y;

    public FixedFloat Bottom => Position.y - Extents.y;

    public FixedFloat Right => Position.x + Extents.x;

    public FixedFloat Left => Position.x - Extents.x;
}