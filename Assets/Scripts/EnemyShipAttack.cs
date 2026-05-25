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

    private void Awake()
    {
        if (targetShipHealth == null)
        {
            Debug.LogWarning("EnemyShipAttack: Target Ship Health reference is missing.", this);
        }

        if (targetShip == null)
        {
            Debug.LogWarning("EnemyShipAttack: Target Ship reference is missing.", this);
        }

        if (playerShipController == null)
        {
            Debug.LogWarning("EnemyShipAttack: Player Ship Controller reference is missing.", this);
        }
    }

    private void Update()
    {
        if (targetShipHealth == null || targetShip == null || playerShipController == null)
        {
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
}
