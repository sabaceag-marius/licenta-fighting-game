using UnityEngine;

public abstract class BaseColliderFactory : MonoBehaviour
{
    [Header("Debug settings")]

    public bool ShowBoundingBox = true;

    public abstract LogicCollider GetLogicCollider();

    public abstract void DrawCollider();

    public abstract void DrawBoundingBox();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        DrawCollider();

        if (!ShowBoundingBox)
            return;

        Gizmos.color = Color.red;

        DrawBoundingBox();
    }
}