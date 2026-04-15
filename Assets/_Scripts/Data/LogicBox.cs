
public struct LogicBox
{
    public FixedVector2 Position;

    // Half-width (x) and Half-height (y)
    public FixedVector2 Extents; 

    public FixedFloat Top => Position.y + Extents.y;
    
    public FixedFloat Bottom => Position.y - Extents.y;
    
    public FixedFloat Right => Position.x + Extents.x;
    
    public FixedFloat Left => Position.x - Extents.x;
}