using UnityEngine;
using Data.Combat;

public class DebugDrawer : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField]
    private bool ShowHurtboxBoundingBox;

    [Header("Colors & Materials")]

    [SerializeField]
    private Material HitboxMaterial;

    [SerializeField]
    private Material HurtboxMaterial;

    [SerializeField]
    private Material IntangibleHurtboxMaterial;

    [SerializeField]
    private Material BoundingBoxMaterial;

    // Cached procedural meshes
    private Mesh boxMesh;
    private Mesh circleMesh;

    private RenderParams hitboxParams;
    private RenderParams hurtboxParams;
    private RenderParams intangibleHurtboxParams;
    private RenderParams boundingBoxParams;

    private void Awake()
    {
        // Generate primitive meshes via code so we don't need asset references
        boxMesh = CreateQuadMesh();
        circleMesh = CreateCircleMesh(24); // 24 segments for a smooth circle

        // Cache RenderParams

        hitboxParams = new RenderParams(HitboxMaterial);
        hurtboxParams = new RenderParams(HurtboxMaterial);
        intangibleHurtboxParams = new RenderParams(IntangibleHurtboxMaterial);
        boundingBoxParams = new RenderParams(BoundingBoxMaterial);
    }

    public void DrawCharacter(Data.CharacterData characterData, Data.Combat.AttackData attack)
    {
        if (characterData.CurrentState == Data.CharacterStateType.Attack)
        {
            DrawAttackState(attack, characterData.CurrentAttackFrame, characterData.Position, -characterData.FacingDirection);
        }
        else
        {
            DrawCharacterHurtbox(characterData.Hurtboxes[0], characterData.Position, characterData.FacingDirection);
        }
    }

    /// <summary>
    /// Call this every frame from your View/Visuals script, passing the current player position.
    /// </summary>
    public void DrawAttackState(Data.Combat.AttackData attack, int currentFrame, Vector2 playerPosition, int facingDirection)
    {
        // Safety check to ensure we don't read out of bounds
        if (currentFrame < 0 || currentFrame >= attack.FrameCount) return;

        FrameData frameData = attack.Frames[currentFrame];

        // Hurtboxes BoundingBox

        if (ShowHurtboxBoundingBox)
        {
            if (attack.OverrideHurtboxes && frameData.HurtboxCount > 0)
            {
                DrawAABB(frameData.HurtboxesBoundingBox, playerPosition, facingDirection, boundingBoxParams);
            }
        }

        // Draw Hurtboxes
        
        if (attack.OverrideHurtboxes && frameData.HurtboxCount > 0)
        {
            for (int i = 0; i < frameData.HurtboxCount; i++)
            {
                Data.Combat.HurtboxData hurtbox = frameData.Hurtboxes[i];

                DrawCapsule(hurtbox.Collider, playerPosition, facingDirection, hurtbox.State == Data.Combat.HurtboxState.Intangible ? intangibleHurtboxParams : hurtboxParams);
            }
        }
        else
        {
            //TODO: ADD NORMAL HURTBOX
        }

        for (int i = 0; i < frameData.HitboxCount; i++)
        {
            DrawCircle(frameData.Hitboxes[i].Collider, playerPosition, facingDirection, hitboxParams);
        }
    }

    private void DrawCharacterHurtbox(Data.Combat.HurtboxData hurtbox, Vector2 playerPosition, int facingDirection)
    {
        if (ShowHurtboxBoundingBox)
        {
                DrawAABB(hurtbox.Collider.BoundingBox, playerPosition, facingDirection, boundingBoxParams);
        }

        DrawCapsule(hurtbox.Collider, playerPosition, facingDirection, hurtbox.State == Data.Combat.HurtboxState.Intangible ? intangibleHurtboxParams : hurtboxParams);
    }

    #region Drawing Math
    
    private void DrawCircle(LogicCollider circle, Vector2 playerPos, int facingDir, RenderParams rp)
    {
        float radius = (float)circle.Radius;

        Vector3 worldPos = (Vector2)FixedMath.GetGlobalPosition(circle.Position, playerPos, facingDir);

        Vector3 scale = new Vector3(radius * 2f, radius * 2f, 1f);
        Matrix4x4 matrix = Matrix4x4.TRS(worldPos, Quaternion.identity, scale);

        // Modern API: Passes the cached RenderParams struct directly to the GPU
        Graphics.RenderMesh(rp, circleMesh, 0, matrix);
    }

    private void DrawAABB(LogicBox box, Vector2 playerPos, int facingDir, RenderParams rp)
    {
        Vector3 worldPos = (Vector2)FixedMath.GetGlobalPosition(box.Position, playerPos, facingDir);

        Vector3 scale = new Vector3((float)box.Extents.x * 2f, (float)box.Extents.y * 2f, 1f);
        Matrix4x4 matrix = Matrix4x4.TRS(worldPos, Quaternion.identity, scale);

        Graphics.RenderMesh(rp, boxMesh, 0, matrix);
    }

    private void DrawCapsule(LogicCollider capsule, Vector2 playerPos, int facingDir, RenderParams rp)
    {
        float radius = (float)capsule.Radius;
        float halfLength = (float)capsule.HalfInnerLength;
        
        Vector3 worldPos = (Vector2)FixedMath.GetGlobalPosition(capsule.Position, playerPos, facingDir);

        Vector2 dir = new Vector2((float)capsule.Direction.x * facingDir, (float)capsule.Direction.y);
        
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0, 0, angle);

        // 1. A rectangle for the inner body
        Vector3 bodyScale = new Vector3(halfLength * 2f, radius * 2f, 1f);
        Matrix4x4 bodyMatrix = Matrix4x4.TRS(worldPos, rotation, bodyScale);
        Graphics.RenderMesh(rp, boxMesh, 0, bodyMatrix);

        // 2. A circle at the top tip of the bone
        Vector3 topPos = worldPos + (new Vector3(dir.x, dir.y, 0f) * halfLength);
        Matrix4x4 topMatrix = Matrix4x4.TRS(topPos, Quaternion.identity, new Vector3(radius * 2f, radius * 2f, 1f));
        Graphics.RenderMesh(rp, circleMesh, 0, topMatrix);

        // 3. A circle at the bottom tip of the bone
        Vector3 bottomPos = worldPos - (new Vector3(dir.x, dir.y, 0f) * halfLength);
        Matrix4x4 bottomMatrix = Matrix4x4.TRS(bottomPos, Quaternion.identity, new Vector3(radius * 2f, radius * 2f, 1f));
        Graphics.RenderMesh(rp, circleMesh, 0, bottomMatrix);
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