using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackController : MonoBehaviour, IAttackController
{
    [SerializeField] private bool showHitboxes;

    [SerializeField] private LayerMask enemyLayer;
    
    //TODO: change this to hurtboxes
    private HashSet<Collider2D> alreadyHit = new HashSet<Collider2D>();

    private List<HitboxData> activeHitboxes;


    private ICharacterManager characterManager;

    private void Awake()
    {
        characterManager = GetComponent<ICharacterManager>();
    }
     
    public void GenerateHitboxes(List<HitboxData> hitboxes)
    {
        activeHitboxes = hitboxes;

        foreach (var hitbox in hitboxes.OrderBy(hitbox => hitbox.Id))
        {
            Vector2 worldPosition = (Vector2)transform.position + hitbox.Center;

            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, hitbox.Radius, enemyLayer).Where(hit => hit.gameObject != gameObject).ToArray();

            foreach (var hit in hits)
            {
                if (!alreadyHit.Contains(hit))
                {
                    alreadyHit.Add(hit);
                    Debug.Log($"Hit {hit.name} with Hitbox ID: {hitbox.Id}!");

                    // TODO: Tell the enemy they took damage
                }
            }
        }
    }

    public void ClearHitTargets()
    {
        alreadyHit.Clear();

        activeHitboxes = null;
    }

    private void OnDrawGizmos()
    {
        if (!showHitboxes || activeHitboxes == null)
            return;

        Gizmos.color = Color.red;

        foreach (var hitbox in activeHitboxes)
        {
            Vector2 worldPos = (Vector2)transform.position + new Vector2(-characterManager.FacingDirection, 0) * hitbox.Center;
            Gizmos.DrawWireSphere(worldPos, hitbox.Radius);
        }
    }
}

