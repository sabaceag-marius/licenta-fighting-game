global using FixedFloat = Unity.Mathematics.FixedPoint.fp;
global using FixedVector2 = Unity.Mathematics.FixedPoint.fp2;
using Unity.Mathematics;
using UnityEngine;

public static class FixedMath
{
    public static FixedFloat Abs(FixedFloat value)
    {
        return value > 0 ? value : -value;
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
        return math.sqrt((double)x).ToFixedFloat();
    }
}
