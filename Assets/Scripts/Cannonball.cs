using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Cannonball : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 3f;

    [Header("Visual Alignment")]
    [SerializeField] private bool rotateToVelocity;
    [SerializeField] private float rotationOffsetDegrees;

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

        Vector2 launchDirection = direction.normalized;
        rb.linearVelocity = launchDirection * speed;
        AlignToDirection(launchDirection);
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

    public void SetDamage(int newDamage)
    {
        damage = Mathf.Max(0, newDamage);
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