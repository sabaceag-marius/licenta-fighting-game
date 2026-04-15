using UnityEngine;
using Data.Combat;

public class DebugDrawer : MonoBehaviour
{
    [Header("Colors & Materials")]
    [Tooltip("Create an Unlit/Color material, set Render Mode to Transparent.")]

    [SerializeField]
    private Material HitboxMaterial;   // Recommended: Red (Alpha 0.4)

    [SerializeField]
    private Material HurtboxMaterial;  // Recommended: Green (Alpha 0.4)

    [SerializeField]
    private Material BoundingBoxMaterial; // Recommended: Yellow (Alpha 0.2)

    // Cached procedural meshes
    private Mesh boxMesh;
    private Mesh circleMesh;

    private void Awake()
    {
        // Generate primitive meshes via code so we don't need asset references
        boxMesh = CreateQuadMesh();
        circleMesh = CreateCircleMesh(24); // 24 segments for a smooth circle
    }

    /// <summary>
    /// Call this every frame from your View/Visuals script, passing the current player position.
    /// </summary>
    public void DrawAttackState(Data.Combat.AttackData attack, int currentFrame, Vector2 playerPosition, int facingDirection)
    {
        // Safety check to ensure we don't read out of bounds
        if (currentFrame < 0 || currentFrame >= attack.FrameCount) return;

        FrameData frameData = attack.Frames[currentFrame];

        // 1. Draw the Broad Phase Bounding Box
        if (frameData.HurtboxCount > 0)
        {
            DrawAABB(frameData.HurtboxesBoundingBox, playerPosition, facingDirection, BoundingBoxMaterial);
        }

        // 2. Draw Hurtboxes (Capsules)
        for (int i = 0; i < frameData.HurtboxCount; i++)
        {
            DrawCapsule(frameData.Hurtboxes[i].Collider, playerPosition, facingDirection, HurtboxMaterial);
        }

        // 3. Draw Hitboxes (Circles)
        for (int i = 0; i < frameData.HitboxCount; i++)
        {
            DrawCircle(frameData.Hitboxes[i].Collider, playerPosition, facingDirection, HitboxMaterial);
        }
    }

    #region Drawing Math
    
    private void DrawCircle(LogicCollider circle, Vector2 playerPos, int facingDir, Material mat)
    {
        // Convert fixed-point to standard floats for Unity rendering
        float radius = (float)circle.Radius;
        Vector2 localPos = new Vector2((float)circle.Position.x * facingDir, (float)circle.Position.y);
        Vector3 worldPos = playerPos + localPos;

        // Scale is diameter (Radius * 2)
        Vector3 scale = new Vector3(radius * 2f, radius * 2f, 1f);
        Matrix4x4 matrix = Matrix4x4.TRS(worldPos, Quaternion.identity, scale);

        Graphics.DrawMesh(circleMesh, matrix, mat, 0);
    }

    private void DrawAABB(LogicBox box, Vector2 playerPos, int facingDir, Material mat)
    {
        Vector2 localPos = new Vector2((float)box.Position.x * facingDir, (float)box.Position.y);
        Vector3 worldPos = playerPos + localPos;

        // Extents are half-size, so multiply by 2 for full scale
        Vector3 scale = new Vector3((float)box.Extents.x * 2f, (float)box.Extents.y * 2f, 1f);
        Matrix4x4 matrix = Matrix4x4.TRS(worldPos, Quaternion.identity, scale);

        Graphics.DrawMesh(boxMesh, matrix, mat, 0);
    }

    private void DrawCapsule(LogicCollider capsule, Vector2 playerPos, int facingDir, Material mat)
    {
        // Convert fixed-point to standard floats for Unity rendering
        float radius = (float)capsule.Radius;
        float halfLength = (float)capsule.HalfInnerLength;
        
        // Calculate the absolute world position of the capsule center
        Vector2 localPos = new Vector2((float)capsule.Position.x * facingDir, (float)capsule.Position.y);
        Vector3 worldPos = playerPos + localPos;

        // Apply facing direction to the direction vector itself for correct mirroring
        Vector2 dir = new Vector2((float)capsule.Direction.x * facingDir, (float)capsule.Direction.y);
        
        // Use Atan2 on the mirrored direction vector to get the direct Euler angle
        // Our quad's length is along X, so Atan2 is the perfect tool with no offset needed.
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // A Capsule is visually drawn as 3 intersecting shapes: 
        // 1. A rectangle for the inner body
        // We scale the Quad's X axis by the bone length, and Y axis by the diameter (Radius * 2)
        Vector3 bodyScale = new Vector3(halfLength * 2f, radius * 2f, 1f);
        Matrix4x4 bodyMatrix = Matrix4x4.TRS(worldPos, rotation, bodyScale);
        Graphics.DrawMesh(boxMesh, bodyMatrix, mat, 0);

        // 2. A circle at the top tip of the bone
        // Simple positioning: Center + DirectionVector * HalfLength
        Vector3 topPos = worldPos + (new Vector3(dir.x, dir.y, 0f) * halfLength);
        Matrix4x4 topMatrix = Matrix4x4.TRS(topPos, Quaternion.identity, new Vector3(radius * 2f, radius * 2f, 1f));
        Graphics.DrawMesh(circleMesh, topMatrix, mat, 0);

        // 3. A circle at the bottom tip of the bone
        // Center - DirectionVector * HalfLength
        Vector3 bottomPos = worldPos - (new Vector3(dir.x, dir.y, 0f) * halfLength);
        Matrix4x4 bottomMatrix = Matrix4x4.TRS(bottomPos, Quaternion.identity, new Vector3(radius * 2f, radius * 2f, 1f));
        Graphics.DrawMesh(circleMesh, bottomMatrix, mat, 0);
    }
    #endregion

    #region Procedural Mesh Generation

    private Mesh CreateQuadMesh()
    {
        Mesh m = new Mesh { name = "DebugQuad" };
        m.vertices = new Vector3[]
        {
            // Define vertices with the long axis along X, width along Y.
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0),
            new Vector3(0.5f, 0.5f, 0)
        };
        m.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        return m;
    }

    private Mesh CreateCircleMesh(int segments)
    {
        Mesh m = new Mesh { name = "DebugCircle" };
        Vector3[] vertices = new Vector3[segments + 1];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // Center vertex
        float angleStep = 360f / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            // Vertices are at 0.5f radius so a scale of 1 equals 1 unit diameter
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 0.5f, Mathf.Sin(angle) * 0.5f, 0);

            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = (i + 1) == segments ? 1 : i + 2;
        }

        m.vertices = vertices;
        m.triangles = triangles;
        return m;
    }

    #endregion
}