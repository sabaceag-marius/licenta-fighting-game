using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class AttackController : MonoBehaviour, IAttackController
{
    [SerializeField] private bool showHitboxes;

    [SerializeField] private LayerMask enemyLayer;

    private HashSet<GameObject> alreadyHit = new HashSet<GameObject>();

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

            //TODO: compare to parent instead of hit.GO

            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, hitbox.Radius, enemyLayer).Where(hit => !hit.transform.IsChildOf(transform)).ToArray();

            foreach (var hit in hits)
            {

                IHurtboxController enemyHurtboxController = hit.GetComponentInParent<IHurtboxController>();

                if (enemyHurtboxController != null)
                {
                    GameObject enemyRoot = hit.gameObject;

                    if (!alreadyHit.Contains(enemyRoot))
                    {
                        enemyHurtboxController.ReceiveHit(hit, hitbox);

                        alreadyHit.Add(enemyRoot);
                    }
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

