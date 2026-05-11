
using System.Collections.Generic;
using UnityEngine;

public class HurtboxController : MonoBehaviour, IHurtboxController
{
    private Collider2D defaultHurtbox;

    private Dictionary<Collider2D, HurtboxData> colliderDictionary = new Dictionary<Collider2D, HurtboxData>();
    
    private Dictionary<HurtboxType, HurtboxData> hurtboxTypeDictionary = new Dictionary<HurtboxType, HurtboxData>();


    private ICharacterManager characterManager;

    private void Awake()
    {
        characterManager = GetComponent<ICharacterManager>();

        Hurtbox[] hurtboxes = GetComponentsInChildren<Hurtbox>(true);

        foreach (var hurtbox in hurtboxes)
        {
            CapsuleCollider2D collider = hurtbox.GetComponent<CapsuleCollider2D>();

            if (hurtbox.Type == HurtboxType.Body)
            {
                defaultHurtbox = collider;

                continue;
            }

            HurtboxData hurtboxData = new HurtboxData
            {
                Type = hurtbox.Type,
                Collider = collider,
                CurrentState = HurtboxState.Normal
            };

            colliderDictionary[collider] = hurtboxData;
            hurtboxTypeDictionary[hurtbox.Type] = hurtboxData;

            collider.isTrigger = true;
        }
    }

    public void SetDefaultHiurtbox()
    {
        if (defaultHurtbox == null)
        {
            Debug.LogError("Default hurtbox is null");
        }
        else
        {
            defaultHurtbox.enabled = true;
        }

        foreach (var hurtboxData in hurtboxTypeDictionary.Values)
        {
            hurtboxData.Collider.enabled = false;
            hurtboxData.CurrentState = HurtboxState.Normal;
        }    
    }

    public void SetFramedataHurtboxes(List<HurtboxOverrideData> hurtboxes, int facingDirection)
    {
        if (defaultHurtbox == null)
        {
            Debug.LogError("Default hurtbox is null");
        }
        else
        {
            defaultHurtbox.enabled = false;
        }

        foreach (var hurtboxData in hurtboxTypeDictionary.Values)
        {
            hurtboxData.Collider.enabled = false;
        }

        if (hurtboxes == null || hurtboxes.Count == 0) 
            return;

        foreach (var hurtboxOverride in hurtboxes)
        {
            if (hurtboxTypeDictionary.TryGetValue(hurtboxOverride.Type, out HurtboxData hurtboxData))
            {
                hurtboxData.Collider.enabled = true;
                hurtboxData.CurrentState = hurtboxOverride.State;
            }

            Vector2 finalCenter = hurtboxOverride.Center;
            float finalAngle = hurtboxOverride.Angle;

            hurtboxData.Collider.transform.localPosition = finalCenter;
            hurtboxData.Collider.transform.localRotation = Quaternion.Euler(0, 0, finalAngle);
            hurtboxData.Collider.size = hurtboxOverride.Size;
        }
    }

    public void ReceiveHit(Collider2D hitCollider, HitboxData hitbox)
    {
        HurtboxState state = HurtboxState.Normal;
        HurtboxType hitType = HurtboxType.Body; // Fallback

        // Find out exactly what we hit
        if (colliderDictionary.TryGetValue(hitCollider, out HurtboxData limb))
        {
            state = limb.CurrentState;
            hitType = limb.Type;
        }
        else if (hitCollider == defaultHurtbox)
        {
            //OBSOLETETODO: change for dodges, rolls etc
            state = HurtboxState.Normal;
        }
        else
        {
            return; // We hit something unexpected, abort
        }

        // Process based on state
        if (state == HurtboxState.Intangible) return;

        if (state == HurtboxState.Invincible)
        {
            //Debug.Log($"Attack clanked! {hitType} is Invincible.");
            // Trigger block spark / hitlag here, but no damage.
            return;
        }

        //Debug.Log($"{hitType} took damage from Hitbox ID: {hitbox.Id}!");
    }

    [Header("Debug")]
    [SerializeField] private bool showHurtboxes = true;

    private void OnDrawGizmos()
    {
        if (!showHurtboxes) return;

        // 1. Draw Default Movement Hurtbox
        if (defaultHurtbox != null && defaultHurtbox.enabled)
        {
            DrawColliderGizmo(defaultHurtbox, HurtboxState.Normal);
        }

        // 2. Draw Attack Limbs
        // If playing, use the dictionary to get the live state and colors
        if (Application.isPlaying && hurtboxTypeDictionary != null)
        {
            foreach (var kvp in hurtboxTypeDictionary)
            {
                if (kvp.Value.Collider != null && kvp.Value.Collider.enabled)
                {
                    DrawColliderGizmo(kvp.Value.Collider, kvp.Value.CurrentState);
                }
            }
        }
        // If in the editor (not playing), just draw the components directly
        else if (!Application.isPlaying)
        {
            Hurtbox[] tags = GetComponentsInChildren<Hurtbox>();
            foreach (var tag in tags)
            {
                CapsuleCollider2D col = tag.GetComponent<CapsuleCollider2D>();
                if (col != null && col.enabled)
                {
                    DrawColliderGizmo(col, HurtboxState.Normal);
                }
            }
        }
    }

    private void DrawColliderGizmo(Collider2D col, HurtboxState state)
    {
        // Set Color based on state
        Gizmos.color = state switch
        {
            HurtboxState.Invincible => Color.cyan,
            HurtboxState.Intangible => Color.blue,
            _ => Color.yellow // Normal State
        };

        // Save the default matrix
        Matrix4x4 oldMatrix = Gizmos.matrix;

        // This is the magic line! It aligns the Gizmo drawing space EXACTLY 
        // with the GameObject's world position, rotation, and scale.
        Gizmos.matrix = col.transform.localToWorldMatrix;

        if (col is CapsuleCollider2D cap)
        {
            float radius = cap.size.x / 2f;
            float cylinderHeight = Mathf.Max(0, cap.size.y - (radius * 2));

            // Because our matrix is now in the Collider's local space, 
            // we just draw at the collider's exact local offset!
            Vector3 center = cap.offset;

            // Draw center box (the straight part of the capsule)
            if (cylinderHeight > 0)
            {
                Gizmos.DrawWireCube(center, new Vector3(cap.size.x, cylinderHeight, 0.1f));
            }

            // Draw top and bottom spheres
            Gizmos.DrawWireSphere(center + new Vector3(0, cylinderHeight / 2f, 0), radius);
            Gizmos.DrawWireSphere(center + new Vector3(0, -cylinderHeight / 2f, 0), radius);
        }
        else if (col is BoxCollider2D box)
        {
            // Box colliders are even easier
            Gizmos.DrawWireCube(box.offset, box.size);
        }

        // Restore the matrix so we don't mess up other Gizmos in the scene
        Gizmos.matrix = oldMatrix;
    }
}