using UnityEngine;

/// <summary>
/// Controls the Paul crew companion behavior and projectile attacks.
/// </summary>
public class PaulCrewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunCrewManager runCrewManager;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform paulFirePoint;
    [SerializeField] private GameObject cannonballPrefab;

    [Header("Base Paul Settings")]
    [SerializeField] private int paulBaseDamage = 1;
    [SerializeField] private float paulBaseFireCooldown = 1.5f;
    [SerializeField] private float paulProjectileSpeed = 12f;
    [SerializeField] private int paulProjectileCount = 1;
    [SerializeField] private float paulDetectionRadius = 8f;

    [Header("Paul Upgrade Values")]
    [SerializeField] private float fasterFiringCooldownReductionPerLevel = 0.2f;
    [SerializeField] private int strongerCannonballsDamageIncreasePerLevel = 1;
    [SerializeField] private float broadsideSpreadAngle = 18f;
    [SerializeField] private int veteranGunnerPierceCount = 1;

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyDetectionMask = Physics2D.DefaultRaycastLayers;

    private CannonShooter playerCannonShooter;
    private float nextFireTime;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        EnsureReferences();
        if (runCrewManager != null)
        {
            runCrewManager.CrewStateChanged += HandleCrewStateChanged;
        }
    }

    private void OnDisable()
    {
        if (runCrewManager != null)
        {
            runCrewManager.CrewStateChanged -= HandleCrewStateChanged;
        }
    }

    private void Update()
    {
        EnsureReferences();
        if (runCrewManager == null || !runCrewManager.IsCrewActive(RunCrewManager.PaulCrewId))
        {
            return;
        }

        if (Time.time < nextFireTime)
        {
            return;
        }

        ShipHealth target = FindNearestEnemy();
        if (target == null)
        {
            return;
        }

        FireAt(target.transform.position);
        nextFireTime = Time.time + GetCurrentFireCooldown();
    }

    private void HandleCrewStateChanged()
    {
        nextFireTime = Mathf.Min(nextFireTime, Time.time + GetCurrentFireCooldown());
    }

    private void FireAt(Vector3 targetPosition)
    {
        GameObject projectilePrefab = GetCannonballPrefab();
        Transform firePoint = paulFirePoint != null ? paulFirePoint : playerTransform;
        if (projectilePrefab == null || firePoint == null)
        {
            return;
        }

        Vector2 baseDirection = targetPosition - firePoint.position;
        if (baseDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        int projectileCount = GetCurrentProjectileCount();
        bool useSpread = HasPaulUpgrade(RunCrewManager.PaulBroadsideExpertUpgradeId) && projectileCount > 1;
        for (int i = 0; i < projectileCount; i++)
        {
            Vector2 direction = useSpread ? RotateDirection(baseDirection.normalized, GetSpreadAngle(i, projectileCount)) : baseDirection.normalized;
            SpawnProjectile(projectilePrefab, firePoint.position, direction);
        }
    }

    private void SpawnProjectile(GameObject projectilePrefab, Vector3 spawnPosition, Vector2 direction)
    {
        GameObject cannonballObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Cannonball cannonball = cannonballObject.GetComponent<Cannonball>();
        if (cannonball == null)
        {
            Debug.LogWarning("PaulCrewController: Cannonball prefab is missing a Cannonball component.", this);
            Destroy(cannonballObject);
            return;
        }

        cannonball.SetDamage(GetCurrentDamage());
        cannonball.SetFiredByPlayer(true);
        cannonball.SetPierceCount(HasPaulUpgrade(RunCrewManager.PaulVeteranGunnerUpgradeId) ? veteranGunnerPierceCount : 0);
        cannonball.Initialize(direction, paulProjectileSpeed, GetOwnerObject());
    }

    private ShipHealth FindNearestEnemy()
    {
        Transform origin = paulFirePoint != null ? paulFirePoint : playerTransform;
        if (origin == null || paulDetectionRadius <= 0f)
        {
            return null;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin.position, paulDetectionRadius, enemyDetectionMask);
        ShipHealth nearest = null;
        float nearestDistanceSqr = float.PositiveInfinity;

        foreach (Collider2D hit in hits)
        {
            ShipHealth health = hit.GetComponentInParent<ShipHealth>();
            if (!IsValidEnemy(health))
            {
                continue;
            }

            float distanceSqr = ((Vector2)health.transform.position - (Vector2)origin.position).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearest = health;
                nearestDistanceSqr = distanceSqr;
            }
        }

        return nearest;
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

    private int GetCurrentDamage()
    {
        int strongerLevels = GetPaulUpgradeLevel(RunCrewManager.PaulStrongerCannonballsUpgradeId);
        return Mathf.Max(0, paulBaseDamage + strongerLevels * strongerCannonballsDamageIncreasePerLevel);
    }

    private float GetCurrentFireCooldown()
    {
        int fasterLevels = GetPaulUpgradeLevel(RunCrewManager.PaulFasterFiringUpgradeId);
        return Mathf.Max(0.05f, paulBaseFireCooldown - fasterLevels * fasterFiringCooldownReductionPerLevel);
    }

    private int GetCurrentProjectileCount()
    {
        int count = Mathf.Max(1, paulProjectileCount);
        if (HasPaulUpgrade(RunCrewManager.PaulCannonMasterUpgradeId))
        {
            count = Mathf.Max(count, 2);
        }

        if (HasPaulUpgrade(RunCrewManager.PaulBroadsideExpertUpgradeId))
        {
            count = Mathf.Max(count, 3);
        }

        return count;
    }

    private bool HasPaulUpgrade(string upgradeId)
    {
        return GetPaulUpgradeLevel(upgradeId) > 0;
    }

    private int GetPaulUpgradeLevel(string upgradeId)
    {
        return runCrewManager == null ? 0 : runCrewManager.GetPaulUpgradeLevel(upgradeId);
    }

    private GameObject GetCannonballPrefab()
    {
        if (cannonballPrefab != null)
        {
            return cannonballPrefab;
        }

        if (playerCannonShooter != null)
        {
            cannonballPrefab = playerCannonShooter.CannonballPrefab;
        }

        return cannonballPrefab;
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

        if (playerTransform == null)
        {
            ShipController2D playerShip = FindFirstObjectByType<ShipController2D>();
            if (playerShip != null)
            {
                playerTransform = playerShip.transform;
            }
        }

        if (playerCannonShooter == null && playerTransform != null)
        {
            playerCannonShooter = playerTransform.GetComponent<CannonShooter>();
        }

        if (cannonballPrefab == null && playerCannonShooter != null)
        {
            cannonballPrefab = playerCannonShooter.CannonballPrefab;
        }
    }

    private float GetSpreadAngle(int index, int count)
    {
        if (count <= 1)
        {
            return 0f;
        }

        float centerOffset = (count - 1) * 0.5f;
        return (index - centerOffset) * broadsideSpreadAngle;
    }

    private static Vector2 RotateDirection(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(direction.x * cos - direction.y * sin, direction.x * sin + direction.y * cos).normalized;
    }
}
