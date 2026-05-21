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

    /// <summary>
    /// Owner is the ship that fired this projectile. It will not be damaged by this cannonball.
    /// </summary>
    public void Initialize(Vector2 direction, float speed, GameObject ownerObject)
    {
        owner = ownerObject;
        rb.linearVelocity = direction.normalized * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ShipHealth health = other.GetComponentInParent<ShipHealth>();

        if (health != null && owner != null && health.transform.IsChildOf(owner.transform))
        {
            return;
        }

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
