using UnityEngine;

public class EnemyShipAttack : MonoBehaviour
{
    [SerializeField] private ShipHealth targetShipHealth;
    [SerializeField] private Transform targetShip;
    [SerializeField] private ShipController2D playerShipController;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int damage = 1;

    private float nextAttackTime;
    private bool hasLoggedMissingRefs;

    private void Awake()
    {
        ResolveReferences(true);
    }

    private void Start()
    {
        ResolveReferences(true);
    }

    private void Update()
    {
        if (targetShipHealth == null || targetShip == null || playerShipController == null)
        {
            ResolveReferences(false);
            return;
        }

        if (!playerShipController.PlayerOnBoard)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        float distanceToTarget = Vector2.Distance(transform.position, targetShip.position);
        if (distanceToTarget > attackRange)
        {
            return;
        }

        targetShipHealth.TakeDamage(damage);
        nextAttackTime = Time.time + attackCooldown;
    }

    private void ResolveReferences(bool logWarnings)
    {
        if (targetShip == null)
        {
            GameObject taggedShip = GameObject.FindWithTag("PlayerShip");
            if (taggedShip != null)
            {
                targetShip = taggedShip.transform;
            }
            else
            {
                GameObject namedShip = GameObject.Find("PlayerShip");
                if (namedShip != null)
                {
                    targetShip = namedShip.transform;
                }
            }

            if (targetShip == null)
            {
                ShipController2D fallbackController = FindFirstObjectByType<ShipController2D>();
                if (fallbackController != null)
                {
                    targetShip = fallbackController.transform;
                }
            }
        }

        if (playerShipController == null && targetShip != null)
        {
            playerShipController = targetShip.GetComponent<ShipController2D>();
        }

        if (targetShipHealth == null && targetShip != null)
        {
            targetShipHealth = targetShip.GetComponent<ShipHealth>();
            if (targetShipHealth == null)
            {
                targetShipHealth = targetShip.GetComponentInParent<ShipHealth>();
            }
            if (targetShipHealth == null)
            {
                targetShipHealth = targetShip.GetComponentInChildren<ShipHealth>();
            }
        }

        if (!logWarnings)
        {
            return;
        }

        bool missing = targetShip == null || playerShipController == null || targetShipHealth == null;
        if (!missing)
        {
            hasLoggedMissingRefs = false;
            return;
        }

        if (hasLoggedMissingRefs)
        {
            return;
        }

        if (targetShip == null)
        {
            Debug.LogWarning("EnemyShipAttack: Could not find PlayerShip target. Assign Target Ship or tag PlayerShip.", this);
        }

        if (playerShipController == null)
        {
            Debug.LogWarning("EnemyShipAttack: Could not find ShipController2D on PlayerShip.", this);
        }

        if (targetShipHealth == null)
        {
            Debug.LogWarning("EnemyShipAttack: Could not find ShipHealth on PlayerShip.", this);
        }

        hasLoggedMissingRefs = true;
    }

    public void Initialize(Transform newTargetShip, ShipController2D newPlayerShipController, ShipHealth newTargetShipHealth)
    {
        if (newTargetShip != null)
        {
            targetShip = newTargetShip;
        }

        if (newPlayerShipController != null)
        {
            playerShipController = newPlayerShipController;
        }

        if (newTargetShipHealth != null)
        {
            targetShipHealth = newTargetShipHealth;
        }

        hasLoggedMissingRefs = false;
    }
}
