using UnityEngine;
using UnityEngine.UIElements;

public class CustomCircleCollider : BaseColliderFactory
{
    [Header("Hitbox Settings")]
    
    public float Radius = 0.5f;

    private float scaledRadius => Mathf.Max(Mathf.Abs(transform.lossyScale.x), Mathf.Abs(transform.lossyScale.y)) * Radius;

    public override LogicCollider GetLogicCollider()
    {
        Vector2 center = transform.position;

        return new LogicCollider
        {
            Type = ColliderType.Circle,
            Position = transform.position.ToFixedVector2(),
            Radius = scaledRadius.ToFixedFloat()
        };
    }

    public override void DrawCollider()
    {
        Gizmos.DrawWireSphere(transform.localPosition, scaledRadius);
    }

    public override void DrawBoundingBox()
    {
        Gizmos.DrawWireCube(transform.localPosition, 2 * new Vector2(scaledRadius, scaledRadius));
    }
}