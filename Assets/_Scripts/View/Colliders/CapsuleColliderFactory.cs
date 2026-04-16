using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CapsuleColliderFactory : BaseColliderFactory
{
    [Header("Collider Settings")]

    public Vector2 Offset = Vector2.zero;
    
    public Vector2 Size = new Vector2(1f, 2f);

    private Vector2 scaledSize => new Vector2(Size.x * Mathf.Abs(transform.lossyScale.x), Size.y * transform.lossyScale.y);

    private Vector2 scaledOffset => new Vector2(Offset.x * Mathf.Abs(transform.lossyScale.x), Offset.y * transform.lossyScale.y);

    public override LogicCollider GetLogicCollider()
    {
        float radius = scaledSize.x / 2f;

        float innerHeight = Mathf.Max(0f, scaledSize.y - scaledSize.x);

        float halfInnerLength = innerHeight / 2f;

        return new LogicCollider
        {
            Type = ColliderType.Capsule,
            Position = ((Vector2)transform.position + scaledOffset).ToFixedVector2(),
            HalfInnerLength = halfInnerLength,
            Direction = GetDirectionVector().ToFixedVector2(),
            Radius = radius,
            Layer = Layer
        };
    }

    public override void DrawCollider()
    {
        Vector2 direction = GetDirectionVector();

        Vector3 scale = transform.lossyScale;

        float radius = scaledSize.x / 2f;

        float innerHeight = Mathf.Max(0f, scaledSize.y - scaledSize.x);

        Vector2 worldCenter = (Vector2)transform.position + scaledOffset;

        Vector2 pointA = worldCenter + (direction * innerHeight * 0.5f);
        Vector2 pointB = worldCenter - (direction * innerHeight * 0.5f);

        // Circles
        Gizmos.DrawWireSphere(pointA, radius);
        Gizmos.DrawWireSphere(pointB, radius);

        Vector2 perpendicularOffset = new Vector2(-direction.y, direction.x) * radius;

        // Walls of the capsule

        Gizmos.DrawLine(pointA + perpendicularOffset, pointB + perpendicularOffset);
        Gizmos.DrawLine(pointA - perpendicularOffset, pointB - perpendicularOffset);
    }

    public override void DrawBoundingBox()
    {
        Vector2 direction = GetDirectionVector();

        Vector2 worldCenter = (Vector2)transform.position + scaledOffset;

        float currentHalfLength = Mathf.Max(0f, scaledSize.y - scaledSize.x) * 0.5f;
        
        float radius = scaledSize.x / 2f;

        float extentX = (Mathf.Abs(direction.x) * currentHalfLength) + radius;
        float extentY = (Mathf.Abs(direction.y) * currentHalfLength) + radius;

        Gizmos.DrawWireCube(worldCenter, new Vector2(extentX * 2f, extentY * 2f));
    }

    private Vector2 GetDirectionVector()
    {
        float angleRadian = transform.eulerAngles.z * Mathf.Deg2Rad;

        return new Vector2(-Mathf.Sin(angleRadian), Mathf.Cos(angleRadian)).normalized;
    }
}

//public LogicBox GetBoundingBox(LogicCapsule capsule)
//{
//    FixedVector2 segmentCenter = new FixedVector2(
//        capsule.PointA.x,
//        (capsule.PointA.y + capsule.PointB.y) / (FixedFloat)2f
//    );

//    FixedFloat innerHeight = capsule.PointA.y - capsule.PointB.y;
//    if (innerHeight < 0) innerHeight = -innerHeight;

//    FixedFloat extentsX = capsule.Radius;
//    FixedFloat extentsY = (innerHeight / (FixedFloat)2f) + capsule.Radius;

//    return new LogicBox
//    {
//        Position = segmentCenter,
//        Extents = new FixedVector2(extentsX, extentsY)
//    };
//}