using UnityEngine;

public abstract class BaseColliderFactory : MonoBehaviour
{
    public bool IsHurtbox = false;
    
    [Header("Debug settings")]

    public bool ShowCollider = true;

    public bool ShowBoundingBox = true;

    public abstract LogicCollider GetLogicCollider();

    public abstract void DrawCollider();

    public abstract void DrawBoundingBox();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if (!ShowCollider)
            return;

        DrawCollider();

        if (!ShowBoundingBox)
            return;

        Gizmos.color = Color.red;

        DrawBoundingBox();
    }
}