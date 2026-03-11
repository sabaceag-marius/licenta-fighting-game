
using UnityEngine;

public class HurtboxData
{
    public HurtboxType Type;

    public CapsuleCollider2D Collider;
    
    public HurtboxState CurrentState = HurtboxState.Normal;
}