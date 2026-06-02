using UnityEngine;

/// <summary>
/// Moves a bird projectile toward a target and applies configured damage or slowing effects on impact.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class BirdHomingProjectile : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float speed = 8f;
    [SerializeField] private float homingTurnSpeed = 360f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float slowChance;
    [SerializeField] private float slowDuration = 1.5f;
    [SerializeField] private bool rotateToVelocity = true;
    [SerializeField] private float rotationOffsetDegrees = -90f;

    private Rigidbody2D rb;
    private Transform target;
    private GameObject owner;
    private bool firedByPlayer = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        if (lifetime > 0f)
        {
            Destroy(gameObject, lifetime);
        }
    }

    private void FixedUpdate()
    {
        Vector2 currentVelocity = rb.linearVelocity;
        Vector2 desiredDirection = currentVelocity.sqrMagnitude > 0.001f ? currentVelocity.normalized : (Vector2)transform.up;

        if (target != null)
        {
            ShipHealth targetHealth = target.GetComponentInParent<ShipHealth>();
            if (targetHealth == null || targetHealth.IsDead)
            {
                target = null;
            }
            else
            {
                Vector2 toTarget = (Vector2)target.position - rb.position;
                if (toTarget.sqrMagnitude > 0.001f)
                {
                    Vector3 newDirection = Vector3.RotateTowards(desiredDirection, toTarget.normalized, homingTurnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
                    desiredDirection = new Vector2(newDirection.x, newDirection.y);
                }
            }
        }

        rb.linearVelocity = desiredDirection.normalized * speed;
        AlignToDirection(desiredDirection);
    }

    /// <summary>
    /// Initializes this component with runtime references and configuration values.
    /// </summary>
    /// <param name="newTarget">Target transform to assign.</param>
    /// <param name="fallbackDirection">Direction used when no target is available.</param>
    /// <param name="projectileSpeed">Projectile movement speed.</param>
    /// <param name="turnSpeed">Projectile turn speed.</param>
    /// <param name="projectileDamage">Damage dealt by the projectile.</param>
    /// <param name="projectileSlowChance">Chance that the projectile applies a slow effect.</param>
    /// <param name="projectileSlowDuration">Duration of the slow effect in seconds.</param>
    /// <param name="ownerObject">Object that owns or fired this projectile.</param>
    public void Initialize(Transform newTarget, Vector2 fallbackDirection, float projectileSpeed, float turnSpeed, int projectileDamage, float projectileSlowChance, float projectileSlowDuration, GameObject ownerObject)
    {
        target = newTarget;
        owner = ownerObject;
        speed = Mathf.Max(0f, projectileSpeed);
        homingTurnSpeed = Mathf.Max(0f, turnSpeed);
        damage = Mathf.Max(0, projectileDamage);
        slowChance = Mathf.Clamp01(projectileSlowChance);
        slowDuration = Mathf.Max(0f, projectileSlowDuration);

        Vector2 launchDirection = fallbackDirection.sqrMagnitude > 0.001f ? fallbackDirection.normalized : Vector2.up;
        rb.linearVelocity = launchDirection * speed;
        AlignToDirection(launchDirection);
    }

    /// <summary>
    /// Sets whether this projectile should be treated as player-fired.
    /// </summary>
    /// <param name="playerOwned">True when the projectile belongs to the player.</param>
    public void SetFiredByPlayer(bool playerOwned)
    {
        firedByPlayer = playerOwned;
    }

    private void AlignToDirection(Vector2 direction)
    {
        if (!rotateToVelocity || direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffsetDegrees);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        ShipHealth health = other.GetComponent<ShipHealth>();
        if (health == null)
        {
            health = other.GetComponentInParent<ShipHealth>();
        }

        if (health == null)
        {
            if (!other.isTrigger)
            {
                Destroy(gameObject);
            }

            return;
        }

        if (firedByPlayer && health.CompareTag("PlayerShip"))
        {
            return;
        }

        health.TakeDamage(damage);
        TryApplySlow(health);
        Destroy(gameObject);
    }

    private void TryApplySlow(ShipHealth health)
    {
        if (health == null || slowChance <= 0f || slowDuration <= 0f || Random.value > slowChance)
        {
            return;
        }

        SimpleEnemyShipAI enemyMovement = health.GetComponentInParent<SimpleEnemyShipAI>();
        if (enemyMovement != null)
        {
            enemyMovement.ApplySlow(slowDuration);
            return;
        }

        Debug.Log($"BirdHomingProjectile: {health.name} does not support slow yet.", this);
    }
}
