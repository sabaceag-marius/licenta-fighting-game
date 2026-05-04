// global using FixedFloat = Unity.Mathematics.FixedPoint.fp;
// global using FixedVector2 = Unity.Mathematics.FixedPoint.fp2;
using UnityEngine;
using Unity.Mathematics.FixedPoint;
using System;
using Newtonsoft.Json;

[JsonConverter(typeof(FixedFloatConverter))]
public struct FixedFloat
{
    public fp rawValue = 0;

    public FixedFloat(fp value) { rawValue = value; }
    public FixedFloat(float value) { rawValue = (fp)value; }

    public static implicit operator FixedFloat(float f) => new FixedFloat((fp)f);

    public static implicit operator FixedFloat(int i) => new FixedFloat((float)i);

    public static implicit operator float(FixedFloat f) => (float)f.rawValue;

    public static implicit operator FixedFloat(fp f) => new FixedFloat(f);

    public static implicit operator fp(FixedFloat f) => f.rawValue;

    public static FixedFloat operator +(FixedFloat a, FixedFloat b) => a.rawValue + b.rawValue;
    public static FixedFloat operator -(FixedFloat a, FixedFloat b) => a.rawValue - b.rawValue;
    public static FixedFloat operator *(FixedFloat a, FixedFloat b) => a.rawValue * b.rawValue;
    public static FixedFloat operator /(FixedFloat a, FixedFloat b) => a.rawValue / b.rawValue;

    public override string ToString() => $"({rawValue})";
}

public struct FixedVector2
{
    public FixedFloat x;
    public FixedFloat y;

    public static FixedVector2 zero => new FixedVector2(fp.zero);

    public FixedVector2(FixedFloat x, FixedFloat y) 
    { 
        this.x = x; 
        this.y = y; 
    }

    public FixedVector2(fp2 value) 
    { 
        this.x = value.x; 
        this.y = value.y; 
    }

    public FixedVector2(float x, float y) 
    { 
        this.x = (fp)x; 
        this.y = (fp)y; 
    }
    
    public static implicit operator FixedVector2(Vector2 v) => new FixedVector2(v.x, v.y);
    public static implicit operator Vector2(FixedVector2 v) => new Vector2((float)v.x, (float)v.y);

    public static implicit operator fp2(FixedVector2 v) => new fp2(v.x.rawValue, v.y.rawValue);
    public static implicit operator FixedVector2(fp2 v) => new FixedVector2(v);

    // Vector addition and subtraction
    public static FixedVector2 operator +(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.x + b.x, a.y + b.y);
    public static FixedVector2 operator -(FixedVector2 a, FixedVector2 b) => new FixedVector2(a.x - b.x, a.y - b.y);
    public static FixedVector2 operator -(FixedVector2 a) => new FixedVector2(-a.x, -a.y);

    // Scalar multiplication and division (Vector * Float)
    public static FixedVector2 operator *(FixedVector2 a, FixedFloat b) => new FixedVector2(a.x * b, a.y * b);
    public static FixedVector2 operator *(FixedFloat a, FixedVector2 b) => new FixedVector2(a * b.x, a * b.y);
    public static FixedVector2 operator /(FixedVector2 a, FixedFloat b) => new FixedVector2(a.x / b, a.y / b);

    // Equality
    public static bool operator ==(FixedVector2 a, FixedVector2 b) => a.x == b.x && a.y == b.y;
    public static bool operator !=(FixedVector2 a, FixedVector2 b) => !(a == b);

    // Override Equals and GetHashCode (Required when overloading == and !=)
    public override bool Equals(object obj) => obj is FixedVector2 other && this == other;
    public override int GetHashCode() => (int)fpmath.hash(this);

    public override string ToString() => $"({x.rawValue}f, {y.rawValue}f)";
}

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

    public static int Min(int a, int b)
    {
        return a < b ? a : b;
    }

    public static int Max(int a, int b)
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
        if (distanceSquared == 0 || distanceSquared <= maxDelta * maxDelta)
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
        if (vector == FixedVector2.zero)
            return FixedVector2.zero;

        return fpmath.normalize(vector);
    }

    public static FixedVector2 GetGlobalPosition(FixedVector2 localPosition, FixedVector2 position, int facingDirection)
    {
        return position + new FixedVector2(localPosition.x * facingDirection, localPosition.y);
    }

    public static FixedVector2 GetDirectionVector(FixedFloat angleDegrees)
    {
        FixedFloat angleRadian = angleDegrees * (FixedFloat)(fpmath.PI / 180);
        
        return new FixedVector2(fpmath.cos(angleRadian), fpmath.sin(angleRadian)).Normalize();
        // return new FixedVector2(-fpmath.sin(angleRadian), fpmath.cos(angleRadian)).Normalize();
    }

    public static int CeilToInt(FixedFloat x)
    {
        return (int) x;
    }
}

public class FixedFloatConverter : JsonConverter<FixedFloat>
{
    public override void WriteJson(JsonWriter writer, FixedFloat value, JsonSerializer serializer)
    {
        // Write ONLY the raw deterministic long. 
        // This changes your JSON from {"rawValue":{"RawValue":1011086}} to simply 1011086
        writer.WriteValue(value.rawValue.RawValue);
    }

    public override FixedFloat ReadJson(JsonReader reader, Type objectType, FixedFloat existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        long extractedRawValue = 0;

        extractedRawValue = (long)reader.Value;

        return new FixedFloat(fp.FromRaw(extractedRawValue));
    }
}