
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public static class LogicColliderExtensions
{
    public static LogicBox GetBoundingBox(this LogicCollider collider)
    {
        switch (collider.Type)
        {
            default:
                return new LogicBox { };

            case ColliderType.Box:
                return collider.GetBoundingBoxBoxCollider();

            case ColliderType.Circle:
                return collider.GetBoundingBoxCircleCollider();

            case ColliderType.Capsule:
                return collider.GetBoundingBoxCapsuleCollider();

        }
    }

    private static LogicBox GetBoundingBoxBoxCollider(this LogicCollider boxCollider)
    {
        return new LogicBox
        {
            Position = boxCollider.Position,
            Extents = boxCollider.Extents
        };
    }

    private static LogicBox GetBoundingBoxCircleCollider(this LogicCollider circleCollider)
    {
        return new LogicBox
        {
            Position = circleCollider.Position,
            Extents = new FixedVector2(2f * circleCollider.Radius, 2f * circleCollider.Radius)
        };
    }

    private static LogicBox GetBoundingBoxCapsuleCollider(this LogicCollider capsuleCollider)
    {
        FixedFloat extentX = capsuleCollider.Radius;

        // 3. The Y extent (half-height) is the inner bone plus the foam padding
        FixedFloat extentY = capsuleCollider.HalfInnerLength + capsuleCollider.Radius;

        return new LogicBox
        {
            Position = capsuleCollider.Position,
            Extents = new FixedVector2(extentX, extentY)
        };
    }
}