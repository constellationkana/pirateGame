using System.Collections.Generic;
using UnityEngine;

public class BirdCrewController : MonoBehaviour
{
    public enum BirdCrewType
    {
        BirdBoy,
        EvilBirdBoy
    }

    [Header("Crew")]
    [SerializeField] private BirdCrewType crewType = BirdCrewType.BirdBoy;
    [SerializeField] private RunCrewManager runCrewManager;
    [SerializeField] private Transform playerTransform;

    [Header("Parrot Visuals")]
    [SerializeField] private GameObject parrotPrefab;
    [SerializeField] private float orbitRadius = 1.6f;
    [SerializeField] private float orbitSpeed = 120f;
    [SerializeField] private int parrotCount = 2;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private int baseDamage = 1;
    [SerializeField] private float baseCooldown = 1.5f;
    [SerializeField] private float firingCooldown = 1.5f;
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float homingTurnSpeed = 360f;
    [Range(0f, 1f)] [SerializeField] private float slowChance = 0.25f;
    [SerializeField] private float slowDuration = 1.5f;

    [Header("Upgrades")]
    [SerializeField] private int damageIncreasePerLevel = 1;
    [SerializeField] private float cooldownReductionPerLevel = 0.2f;
    [SerializeField] private float minimumCooldown = 0.35f;
    [SerializeField] private int maxDamageUpgradeLevel = 3;
    [SerializeField] private int maxCooldownUpgradeLevel = 3;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyDetectionMask = Physics2D.DefaultRaycastLayers;

    private readonly List<Transform> parrots = new();
    private float nextFireTime;
    private float orbitAngleOffset;
    private static Sprite birdBoyProjectileSprite;
    private static Sprite evilBirdBoyProjectileSprite;
    private static Sprite parrotPlaceholderSprite;

    public BirdCrewType CrewType => crewType;

    private string CrewId => crewType == BirdCrewType.BirdBoy ? RunCrewManager.BirdBoyCrewId : RunCrewManager.EvilBirdBoyCrewId;
    private string DamageUpgradeId => crewType == BirdCrewType.BirdBoy ? RunCrewManager.BirdBoyDamageUpgradeId : RunCrewManager.EvilBirdBoyDamageUpgradeId;
    private string CooldownUpgradeId => crewType == BirdCrewType.BirdBoy ? RunCrewManager.BirdBoyCooldownUpgradeId : RunCrewManager.EvilBirdBoyCooldownUpgradeId;

    public void SetCrewType(BirdCrewType type)
    {
        crewType = type;
        RefreshActiveState();
    }

    public int MaxDamageUpgradeLevel => Mathf.Max(1, maxDamageUpgradeLevel);
    public int MaxCooldownUpgradeLevel => Mathf.Max(1, maxCooldownUpgradeLevel);

    private void Awake()
    {
        EnsureReferences();
        firingCooldown = Mathf.Max(0.01f, firingCooldown <= 0f ? baseCooldown : firingCooldown);
    }

    private void OnEnable()
    {
        EnsureReferences();
        if (runCrewManager != null)
        {
            runCrewManager.SetBirdCrewUpgradeMaxLevels(CrewId, maxDamageUpgradeLevel, maxCooldownUpgradeLevel);
            runCrewManager.CrewStateChanged += HandleCrewStateChanged;
        }

        RefreshActiveState();
    }

    private void OnDisable()
    {
        if (runCrewManager != null)
        {
            runCrewManager.CrewStateChanged -= HandleCrewStateChanged;
        }

        ClearParrots();
    }

    private void Update()
    {
        EnsureReferences();
        if (!IsCrewActive())
        {
            ClearParrots();
            return;
        }

        EnsureParrots();
        UpdateParrotOrbit();

        if (Time.time < nextFireTime)
        {
            return;
        }

        ShipHealth target = PickRandomEnemyInRange();
        if (target == null)
        {
            return;
        }

        FireAt(target);
        nextFireTime = Time.time + GetCurrentCooldown();
    }

    private void HandleCrewStateChanged()
    {
        RefreshActiveState();
        if (IsCrewActive())
        {
            nextFireTime = Mathf.Min(nextFireTime, Time.time + GetCurrentCooldown());
        }
    }

    private void RefreshActiveState()
    {
        if (IsCrewActive())
        {
            EnsureParrots();
        }
        else
        {
            ClearParrots();
        }
    }

    private bool IsCrewActive()
    {
        return runCrewManager != null && runCrewManager.IsCrewActive(CrewId);
    }

    private void FireAt(ShipHealth target)
    {
        Transform firePoint = GetFirePoint();
        if (firePoint == null || target == null)
        {
            return;
        }

        GameObject projectileObject = projectilePrefab != null
            ? Instantiate(projectilePrefab, firePoint.position, Quaternion.identity)
            : CreateRuntimeProjectileObject(firePoint.position);
        BirdHomingProjectile projectile = projectileObject.GetComponent<BirdHomingProjectile>();
        if (projectile == null)
        {
            projectile = projectileObject.AddComponent<BirdHomingProjectile>();
        }

        Vector2 direction = (target.transform.position - firePoint.position).sqrMagnitude > 0.001f
            ? ((Vector2)(target.transform.position - firePoint.position)).normalized
            : Vector2.up;
        projectile.SetFiredByPlayer(true);
        projectile.Initialize(target.transform, direction, projectileSpeed, homingTurnSpeed, GetCurrentDamage(), slowChance, slowDuration, GetOwnerObject());
    }

    private ShipHealth PickRandomEnemyInRange()
    {
        Transform origin = GetFirePoint();
        if (origin == null || detectionRadius <= 0f)
        {
            return null;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin.position, detectionRadius, enemyDetectionMask);
        List<ShipHealth> candidates = new();
        foreach (Collider2D hit in hits)
        {
            ShipHealth health = hit.GetComponentInParent<ShipHealth>();
            if (IsValidEnemy(health) && !candidates.Contains(health))
            {
                candidates.Add(health);
            }
        }

        return candidates.Count == 0 ? null : candidates[Random.Range(0, candidates.Count)];
    }

    private bool IsValidEnemy(ShipHealth health)
    {
        if (health == null || health.IsDead)
        {
            return false;
        }

        if (playerTransform != null && (health.transform == playerTransform || health.transform.IsChildOf(playerTransform) || playerTransform.IsChildOf(health.transform)))
        {
            return false;
        }

        return health.GetComponentInParent<SimpleEnemyShipAI>() != null
            || health.GetComponentInParent<EnemyShipAttack>() != null
            || !health.CompareTag("PlayerShip");
    }

    private void EnsureParrots()
    {
        int desiredCount = Mathf.Max(0, parrotCount);
        while (parrots.Count < desiredCount)
        {
            Transform parent = playerTransform != null ? playerTransform : transform;
            GameObject parrotObject = parrotPrefab != null
                ? Instantiate(parrotPrefab, parent.position, Quaternion.identity, parent)
                : CreateRuntimeParrotObject(parent.position, parent);
            parrotObject.name = crewType == BirdCrewType.BirdBoy ? "Bird-Boy Parrot" : "Evil-Bird-Boy Parrot";
            parrots.Add(parrotObject.transform);
        }

        while (parrots.Count > desiredCount)
        {
            Transform parrot = parrots[parrots.Count - 1];
            parrots.RemoveAt(parrots.Count - 1);
            if (parrot != null)
            {
                Destroy(parrot.gameObject);
            }
        }
    }

    private void UpdateParrotOrbit()
    {
        if (playerTransform == null || parrots.Count == 0)
        {
            return;
        }

        orbitAngleOffset += orbitSpeed * Time.deltaTime;
        float spacing = 360f / parrots.Count;
        for (int i = 0; i < parrots.Count; i++)
        {
            Transform parrot = parrots[i];
            if (parrot == null)
            {
                continue;
            }

            float angle = (orbitAngleOffset + spacing * i) * Mathf.Deg2Rad;
            Vector3 offset = new(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            parrot.position = playerTransform.position + offset * orbitRadius;

            Vector3 tangent = new(-Mathf.Sin(angle), Mathf.Cos(angle), 0f);
            if (tangent.sqrMagnitude > 0.001f)
            {
                parrot.up = tangent.normalized;
            }
        }
    }

    private void ClearParrots()
    {
        for (int i = 0; i < parrots.Count; i++)
        {
            if (parrots[i] != null)
            {
                Destroy(parrots[i].gameObject);
            }
        }

        parrots.Clear();
    }

    private Transform GetFirePoint()
    {
        if (parrots.Count > 0)
        {
            int index = Mathf.Abs(Mathf.FloorToInt(Time.time / Mathf.Max(0.01f, GetCurrentCooldown()))) % parrots.Count;
            if (parrots[index] != null)
            {
                return parrots[index];
            }
        }

        return playerTransform != null ? playerTransform : transform;
    }

    private int GetCurrentDamage()
    {
        int level = runCrewManager == null ? 0 : runCrewManager.GetBirdCrewUpgradeLevel(DamageUpgradeId);
        return Mathf.Max(0, baseDamage + level * damageIncreasePerLevel);
    }

    private float GetCurrentCooldown()
    {
        int level = runCrewManager == null ? 0 : runCrewManager.GetBirdCrewUpgradeLevel(CooldownUpgradeId);
        float startingCooldown = firingCooldown > 0f ? firingCooldown : baseCooldown;
        return Mathf.Max(minimumCooldown, startingCooldown - level * cooldownReductionPerLevel);
    }

    private GameObject GetOwnerObject()
    {
        return playerTransform != null ? playerTransform.gameObject : gameObject;
    }

    private void EnsureReferences()
    {
        if (runCrewManager == null)
        {
            runCrewManager = FindFirstObjectByType<RunCrewManager>();
        }

        if (runCrewManager != null)
        {
            runCrewManager.SetBirdCrewUpgradeMaxLevels(CrewId, maxDamageUpgradeLevel, maxCooldownUpgradeLevel);
        }

        if (playerTransform == null)
        {
            ShipController2D shipController = FindFirstObjectByType<ShipController2D>();
            if (shipController != null)
            {
                playerTransform = shipController.transform;
            }
        }
    }

    private GameObject CreateRuntimeProjectileObject(Vector3 position)
    {
        GameObject projectile = new(crewType == BirdCrewType.BirdBoy ? "Egg Missile" : "Poop Missile");
        projectile.transform.position = position;
        SpriteRenderer spriteRenderer = projectile.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = crewType == BirdCrewType.BirdBoy ? GetBirdBoyProjectileSprite() : GetEvilBirdBoyProjectileSprite();
        spriteRenderer.sortingOrder = 20;
        CircleCollider2D collider = projectile.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.16f;
        projectile.AddComponent<Rigidbody2D>();
        projectile.AddComponent<BirdHomingProjectile>();
        return projectile;
    }

    private GameObject CreateRuntimeParrotObject(Vector3 position, Transform parent)
    {
        GameObject parrot = new("RuntimeParrot");
        parrot.transform.SetParent(parent, false);
        parrot.transform.position = position;
        SpriteRenderer spriteRenderer = parrot.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetParrotPlaceholderSprite();
        spriteRenderer.color = crewType == BirdCrewType.BirdBoy ? new Color(0.2f, 0.8f, 0.25f, 1f) : new Color(0.35f, 0.1f, 0.45f, 1f);
        spriteRenderer.sortingOrder = 15;
        return parrot;
    }

    private static Sprite GetBirdBoyProjectileSprite()
    {
        return birdBoyProjectileSprite != null ? birdBoyProjectileSprite : birdBoyProjectileSprite = CreateCircleSprite(new Color(1f, 0.95f, 0.75f, 1f));
    }

    private static Sprite GetEvilBirdBoyProjectileSprite()
    {
        return evilBirdBoyProjectileSprite != null ? evilBirdBoyProjectileSprite : evilBirdBoyProjectileSprite = CreateCircleSprite(new Color(0.33f, 0.18f, 0.06f, 1f));
    }

    private static Sprite GetParrotPlaceholderSprite()
    {
        return parrotPlaceholderSprite != null ? parrotPlaceholderSprite : parrotPlaceholderSprite = CreateCircleSprite(Color.white);
    }

    private static Sprite CreateCircleSprite(Color color)
    {
        const int textureSize = 32;
        Texture2D texture = new(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        Vector2 center = new((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.45f;
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(1f - Mathf.InverseLerp(radius * 0.8f, radius, distance));
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
    }
}
