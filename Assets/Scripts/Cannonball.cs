using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Cannonball : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;

    private Rigidbody2D rb;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void Initialize(Vector2 direction, float speed, GameObject ownerObject)
    {
        owner = ownerObject;
<<<<<<< HEAD

        Vector2 normalizedDirection = direction.normalized;
        rb.linearVelocity = normalizedDirection * speed;

        // Rotate sprite to face its travel direction (assuming sprite forward is +X).
        float angle = Mathf.Atan2(normalizedDirection.y, normalizedDirection.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
=======
        rb.linearVelocity = direction.normalized * speed;
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        ShipHealth health = other.GetComponent<ShipHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
