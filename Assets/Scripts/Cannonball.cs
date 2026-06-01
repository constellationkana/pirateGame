using System.Collections.Generic;
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

    [Header("Upgrade Effects")]
    [SerializeField] private float sizeMultiplier = 1f;
    [SerializeField] private bool explosive;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 1;
    [SerializeField] private LayerMask explosionLayerMask = Physics2D.DefaultRaycastLayers;

    private Rigidbody2D rb;
    private GameObject owner;
    private Vector3 baseScale;
    private bool explosionTriggered;

    private void Awake()
    {
        baseScale = transform.localScale;
        ApplySizeMultiplier();

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

    public void SetSizeMultiplier(float multiplier)
    {
        sizeMultiplier = Mathf.Max(0.1f, multiplier);
        ApplySizeMultiplier();
    }

    public void SetExplosion(bool enabled, float radius, int damageAmount)
    {
        explosive = enabled;
        explosionRadius = Mathf.Max(0f, radius);
        explosionDamage = Mathf.Max(0, damageAmount);
    }

    private void ApplySizeMultiplier()
    {
        Vector3 referenceScale = baseScale == Vector3.zero ? transform.localScale : baseScale;
        transform.localScale = referenceScale * Mathf.Max(0.1f, sizeMultiplier);
    }

    private void TriggerExplosion(ShipHealth directHitHealth)
    {
        if (!explosive || explosionTriggered || explosionRadius <= 0f || explosionDamage <= 0)
        {
            return;
        }

        explosionTriggered = true;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionLayerMask);
        HashSet<ShipHealth> damagedShips = new();
        for (int i = 0; i < hits.Length; i++)
        {
            ShipHealth health = hits[i].GetComponent<ShipHealth>();
            if (health == null)
            {
                health = hits[i].GetComponentInParent<ShipHealth>();
            }

            if (health == null || health == directHitHealth || damagedShips.Contains(health) || IsOwnerOrOwnerChild(health.gameObject))
            {
                continue;
            }

            damagedShips.Add(health);
            health.TakeDamage(explosionDamage);
        }
    }

    private bool IsOwnerOrOwnerChild(GameObject candidate)
    {
        return owner != null && (candidate == owner || candidate.transform.IsChildOf(owner.transform));
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
            TriggerExplosion(health);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger)
        {
            TriggerExplosion(null);
            Destroy(gameObject);
        }
    }
}