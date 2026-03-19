using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CapsuleColliderFactory : BaseColliderFactory
{
    [Header("Collider Settings")]

    public Vector2 Offset = Vector2.zero;
    
    public Vector2 Size = new Vector2(1f, 2f);

    private Vector2 scaledSize => new Vector2(Size.x * transform.lossyScale.x, Size.y * transform.lossyScale.y);

    private Vector2 scaledOffset => new Vector2(Offset.x * transform.lossyScale.x, Offset.y * transform.lossyScale.y);

    public override LogicCollider GetLogicCollider()
    {
        float radius = scaledSize.x / 2f;

        float innerHeight = Mathf.Max(0f, scaledSize.y - scaledSize.x);

        float halfInnerLength = innerHeight / 2f;

        return new LogicCollider
        {
            Type = ColliderType.Capsule,
            Position = ((Vector2)transform.position + scaledOffset).ToFixedVector2(),
            HalfInnerLength = halfInnerLength.ToFixedFloat(),
            Radius = radius.ToFixedFloat()
        };
    }

    public override void DrawCollider()
    {
        Vector3 scale = transform.lossyScale;

        float radius = scaledSize.x / 2f;

        float innerHeight = Mathf.Max(0f, scaledSize.y - scaledSize.x);

        Vector3 worldCenter = transform.position + (Vector3)scaledOffset;

        Gizmos.DrawWireCube(worldCenter, new Vector3(scaledSize.x, innerHeight, 0.1f));
        Gizmos.DrawWireSphere(worldCenter + new Vector3(0, innerHeight / 2f, 0), radius);
        Gizmos.DrawWireSphere(worldCenter - new Vector3(0, innerHeight / 2f, 0), radius);
    }

    public override void DrawBoundingBox()
    {
        Vector2 worldCenter = (Vector2)transform.position + scaledOffset;

        Gizmos.DrawWireCube(worldCenter, scaledSize);
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