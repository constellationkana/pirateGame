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
    [SerializeField] private int pierceCount;

    [Header("Explosion")]
    [SerializeField] private bool explosive;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 1;
    [SerializeField] private LayerMask explosionDamageMask = Physics2D.DefaultRaycastLayers;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionEffectLifetime = 0.5f;
    [SerializeField] private bool createRuntimePlaceholderExplosion = true;
    [SerializeField] private Color placeholderExplosionColor = new(1f, 0.45f, 0f, 0.8f);
    [SerializeField] private float placeholderExplosionScale = 1f;
    [SerializeField] private bool damageDirectHitTargetOnlyOnce = true;
    [SerializeField] private bool logExplosionDebug;

    private static Sprite placeholderExplosionSprite;

    private Rigidbody2D rb;
    private GameObject owner;
    private Vector3 baseScale;
    private bool firedByPlayer;
    private bool explosionTriggered;
    private readonly HashSet<ShipHealth> piercedTargets = new();

    public bool FiredByPlayer => firedByPlayer;

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

    public void SetFiredByPlayer(bool playerOwned)
    {
        firedByPlayer = playerOwned;
    }

    public void SetPierceCount(int count)
    {
        pierceCount = Mathf.Max(0, count);
        piercedTargets.Clear();
    }

    public void SetExplosion(bool enabled, float radius, int damageAmount)
    {
        explosive = enabled;
        explosionRadius = Mathf.Max(0f, radius);
        explosionDamage = Mathf.Max(0, damageAmount);
    }

    public void SetExplosionEffect(GameObject effectPrefab, float effectLifetime)
    {
        explosionEffectPrefab = effectPrefab;
        explosionEffectLifetime = Mathf.Max(0f, effectLifetime);
    }

    public void SetExplosionDamageMask(LayerMask damageMask)
    {
        explosionDamageMask = damageMask;
    }

    public void ConfigureExplosion(bool enabled, float radius, int damageAmount, GameObject effectPrefab, LayerMask damageMask)
    {
        ConfigureExplosion(enabled, radius, damageAmount, effectPrefab, explosionEffectLifetime, damageMask);
    }

    public void ConfigureExplosion(bool enabled, float radius, int damageAmount, GameObject effectPrefab, float effectLifetime, LayerMask damageMask)
    {
        SetExplosion(enabled, radius, damageAmount);
        SetExplosionEffect(effectPrefab, effectLifetime);
        SetExplosionDamageMask(damageMask);
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
        SpawnExplosionEffect();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionDamageMask);
        HashSet<ShipHealth> damagedShips = new();
        int damagedCount = 0;

        for (int i = 0; i < hits.Length; i++)
        {
            ShipHealth health = hits[i].GetComponent<ShipHealth>();
            if (health == null)
            {
                health = hits[i].GetComponentInParent<ShipHealth>();
            }

            if (ShouldSkipExplosionTarget(health, directHitHealth, damagedShips))
            {
                continue;
            }

            damagedShips.Add(health);
            health.TakeDamage(explosionDamage);
            damagedCount++;
        }

        if (logExplosionDebug)
        {
            Debug.Log($"Cannonball exploded at {transform.position} and hit {damagedCount} ships.", this);
        }
    }

    private bool ShouldSkipExplosionTarget(ShipHealth health, ShipHealth directHitHealth, HashSet<ShipHealth> damagedShips)
    {
        if (health == null || damagedShips.Contains(health))
        {
            return true;
        }

        if (damageDirectHitTargetOnlyOnce && health == directHitHealth)
        {
            return true;
        }

        if (IsOwnerOrOwnerChild(health.gameObject))
        {
            return true;
        }

        return firedByPlayer && health.CompareTag("PlayerShip");
    }

    private void SpawnExplosionEffect()
    {
        GameObject effect = null;
        if (explosionEffectPrefab != null)
        {
            effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        else if (createRuntimePlaceholderExplosion)
        {
            effect = CreatePlaceholderExplosionEffect();
        }

        if (effect != null && explosionEffectLifetime > 0f)
        {
            Destroy(effect, explosionEffectLifetime);
        }
    }

    private GameObject CreatePlaceholderExplosionEffect()
    {
        GameObject effect = new("ExplosionPlaceholderRuntime");
        effect.transform.position = transform.position;
        float scale = Mathf.Max(0.1f, explosionRadius * 2f * Mathf.Max(0.1f, placeholderExplosionScale));
        effect.transform.localScale = Vector3.one * scale;

        SpriteRenderer spriteRenderer = effect.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetPlaceholderExplosionSprite();
        spriteRenderer.color = placeholderExplosionColor;
        spriteRenderer.sortingOrder = 50;
        return effect;
    }

    private static Sprite GetPlaceholderExplosionSprite()
    {
        if (placeholderExplosionSprite != null)
        {
            return placeholderExplosionSprite;
        }

        const int textureSize = 32;
        Texture2D texture = new(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.48f;
        Color inner = new(1f, 0.9f, 0.15f, 0.85f);
        Color outer = new(1f, 0.25f, 0f, 0f);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float t = Mathf.Clamp01(distance / radius);
                Color color = Color.Lerp(inner, outer, t);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        placeholderExplosionSprite = Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
        return placeholderExplosionSprite;
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
            if (firedByPlayer && health.CompareTag("PlayerShip"))
            {
                return;
            }

            if (piercedTargets.Contains(health))
            {
                return;
            }

            piercedTargets.Add(health);
            health.TakeDamage(damage);
            TriggerExplosion(health);

            if (pierceCount > 0)
            {
                pierceCount--;
                return;
            }

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
