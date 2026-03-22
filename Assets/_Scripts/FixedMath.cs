global using FixedFloat = Unity.Mathematics.FixedPoint.fp;
global using FixedVector2 = Unity.Mathematics.FixedPoint.fp2;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
public static class FixedMath
{
    public static FixedFloat Abs(FixedFloat value)
    {
        return value > 0f ? value : -value;
    }
 
    public static FixedFloat ToFixedFloat(this float x)
    {
        return (FixedFloat)x;
    }

    public static FixedFloat ToFixedFloat(this double x)
    {
        return (FixedFloat)x;
    }

    public static FixedVector2 ToFixedVector2(this Vector2 v)
    {
        return new FixedVector2(v.x.ToFixedFloat(), v.y.ToFixedFloat());
    }

    public static FixedVector2 ToFixedVector2(this Vector3 v)
    {
        return new FixedVector2(ToFixedFloat(v.x), ToFixedFloat(v.y));
    }

    public static FixedFloat Min(FixedFloat a, FixedFloat b)
    {
        return a < b ? a : b;
    }

    public static FixedFloat Max(FixedFloat a, FixedFloat b)
    {
        return a > b ? a : b;
    }

    public static FixedFloat Clamp(FixedFloat x, FixedFloat a, FixedFloat b)
    {
        return Max(a, Min(b, x));
    }

    public static FixedFloat Sqrt(FixedFloat x)
    {
        return fpmath.sqrt(x);
    }

    public static FixedFloat Sign (FixedFloat x)
    {
        return fpmath.sign(x);
    }
    
    public static FixedFloat MoveTowards(this FixedFloat current, FixedFloat target, FixedFloat maxDelta)
    {
        FixedFloat distance = target - current;

        // If we are close enough to snap to the target, just return the target
        if (FixedMath.Abs(distance) <= maxDelta)
        {
            return target;
        }

        // Move towards the target using pure addition/ subtraction(Zero multiplication!)
        if (target > current)
        {
            return current + maxDelta;
        }
        else
        {
            return current - maxDelta;
        }
    }

    public static FixedVector2 MoveTowards(this FixedVector2 current, FixedVector2 target, FixedFloat maxDelta)
    {
        FixedVector2 difference = target - current;
        FixedFloat distanceSquared = (difference.x * difference.x) + (difference.y * difference.y);

        // If we are within range snap to target
        if (distanceSquared == 0f || distanceSquared <= maxDelta * maxDelta)
        {
            return target;
        }

        FixedFloat distance = FixedMath.Sqrt(distanceSquared);
        FixedVector2 direction = new FixedVector2(difference.x / distance, difference.y / distance);

        return new FixedVector2(
            current.x + (direction.x * maxDelta),
            current.y + (direction.y * maxDelta)
        );
    }

    public static FixedVector2 Normalize(this FixedVector2 vector)
    {
        return fpmath.normalize(vector);
    }
}
