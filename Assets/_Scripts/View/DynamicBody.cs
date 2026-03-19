using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseColliderFactory))]
public class DynamicBody : MonoBehaviour
{
    [Header("Dynamic body settings")]

    [SerializeField]
    private float gravity = 1;

    [SerializeField]
    private float movementSpeed = 1;

    private BaseColliderFactory colliderFactory;

    private void Awake()
    {
        colliderFactory = GetComponent<BaseColliderFactory>();
    }

    public LogicDynamicBody GetLogicBody()
    {
        return new LogicDynamicBody
        {
            Position = transform.position.ToFixedVector2(),
            Collider = colliderFactory.GetLogicCollider(),
            Gravity = (gravity / 100).ToFixedFloat(),
            MovementSpeed = (movementSpeed / 100).ToFixedFloat()
        };
    }
}
