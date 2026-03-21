using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxColliderFactory : BaseColliderFactory
{
    [Header("Hitbox Settings")]

    public Vector2 Size = new Vector2(1, 1);

    private Vector2 scaledSize => new Vector2(Size.x * Mathf.Abs(transform.lossyScale.x), Size.y * Mathf.Abs(transform.lossyScale.y));

    public override LogicCollider GetLogicCollider()
    {
        return new LogicCollider
        {
            Type = ColliderType.Box,
            Position = transform.position.ToFixedVector2(),
            Extents = scaledSize.ToFixedVector2() / 2f
        };
    }

    public override void DrawCollider()
    {
        Gizmos.DrawWireCube(transform.position, scaledSize);
    }

    public override void DrawBoundingBox()
    {
        Gizmos.DrawWireCube(transform.position, scaledSize);
    }
}
