
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public static class LogicColliderExtensions
{
    public static LogicCollider GetBoundingBox(this LogicCollider collider)
    {
        switch (collider.Type)
        {
            default:
                return new LogicCollider { Type = ColliderType.Box };

            case ColliderType.Box:
                return collider.GetBoundingBoxBoxCollider();

            case ColliderType.Circle:
                return collider.GetBoundingBoxCircleCollider();

            case ColliderType.Capsule:
                return collider.GetBoundingBoxCapsuleCollider();
        }
    }

    private static LogicCollider GetBoundingBoxBoxCollider(this LogicCollider boxCollider)
    {
        return new LogicCollider
        {
            Position = boxCollider.Position,
            Extents = boxCollider.Extents,
            Type = ColliderType.Box
        };
    }

    private static LogicCollider GetBoundingBoxCircleCollider(this LogicCollider circleCollider)
    {
        return new LogicCollider
        {
            Position = circleCollider.Position,
            Extents = new FixedVector2(circleCollider.Radius, circleCollider.Radius),
            Type = ColliderType.Box
        };
    }

    private static LogicCollider GetBoundingBoxCapsuleCollider(this LogicCollider capsuleCollider)
    {
        FixedFloat extentX = (FixedMath.Abs(capsuleCollider.Direction.x) * capsuleCollider.HalfInnerLength) + capsuleCollider.Radius;
        FixedFloat extentY = (FixedMath.Abs(capsuleCollider.Direction.y) * capsuleCollider.HalfInnerLength) + capsuleCollider.Radius;

        return new LogicCollider
        {
            Position = capsuleCollider.Position,
            Extents = new FixedVector2(extentX, extentY),
            Type = ColliderType.Box
        };
    }
}