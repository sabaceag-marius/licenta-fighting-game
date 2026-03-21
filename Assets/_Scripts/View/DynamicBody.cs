using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BaseColliderFactory))]
public class DynamicBody : MonoBehaviour
{
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
        };
    }
}
