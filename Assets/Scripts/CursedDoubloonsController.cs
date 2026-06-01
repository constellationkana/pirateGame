using System.Collections.Generic;
using UnityEngine;

public class CursedDoubloonsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject doubloonPrefab;
    [SerializeField] private Transform player;

    [Header("Orbit")]
    [SerializeField] private float orbitRadius = 2f;
    [SerializeField] private float spinSpeed = 180f;

    [Header("Cycle")]
    [SerializeField] private float activeDuration = 8f;
    [SerializeField] private float inactiveDelay = 4f;

    [Header("Upgrade Values")]
    [SerializeField] private int startingDoubloonCount = 2;
    [SerializeField] private int maxDoubloonCount = 8;
    [SerializeField] private int damage = 1;

    [Header("Tuning")]
    [SerializeField] private float enemyDamageCooldown = 0.5f;
    [SerializeField] private float durationIncreasePerUpgrade = 0.5f;
    [SerializeField] private float inactiveDelayReductionPerUpgrade = 0.25f;
    [SerializeField] private float minimumInactiveDelay = 1f;

    private readonly List<Transform> spawnedDoubloons = new();
    private readonly Dictionary<ShipHealth, float> lastDamageTimes = new();
    private int upgradeLevel;
    private int targetDoubloonCount;
    private int effectiveDamage;
    private float currentActiveDuration;
    private float currentInactiveDelay;
    private float stateTimer;
    private float orbitAngle;
    private bool isUnlockedForRun;
    private bool isCycleActive;

    public bool IsUnlockedForRun => isUnlockedForRun;
    public int UpgradeLevel => upgradeLevel;
    public int CurrentDoubloonCount => targetDoubloonCount;

    private void Awake()
    {
        if (player == null)
        {
            player = transform;
        }

        ResetForNewRun();
    }

    private void OnDisable()
    {
        HideDoubloons();
    }

    private void Update()
    {
        if (!isUnlockedForRun)
        {
            return;
        }

        TickCycle();
        if (isCycleActive)
        {
            UpdateOrbit();
        }
    }

    public void ActivateOrUpgrade()
    {
        upgradeLevel = Mathf.Max(1, upgradeLevel + 1);
        isUnlockedForRun = true;

        targetDoubloonCount = Mathf.Clamp(startingDoubloonCount + upgradeLevel - 1, 1, Mathf.Max(1, maxDoubloonCount));
        currentActiveDuration = Mathf.Max(0.1f, activeDuration + Mathf.Max(0, upgradeLevel - 1) * durationIncreasePerUpgrade);
        currentInactiveDelay = Mathf.Max(minimumInactiveDelay, inactiveDelay - Mathf.Max(0, upgradeLevel - 1) * inactiveDelayReductionPerUpgrade);

        int shipShopDamageLevel = PlayerProgression.Instance != null ? PlayerProgression.Instance.GetCursedDoubloonsDamageLevel() : 0;
        effectiveDamage = Mathf.Max(1, damage + shipShopDamageLevel);

        EnsureDoubloonCount();
        StartActiveCycle();
    }

    public void ResetForNewRun()
    {
        upgradeLevel = 0;
        targetDoubloonCount = Mathf.Clamp(startingDoubloonCount, 1, Mathf.Max(1, maxDoubloonCount));
        effectiveDamage = Mathf.Max(1, damage);
        currentActiveDuration = Mathf.Max(0.1f, activeDuration);
        currentInactiveDelay = Mathf.Max(minimumInactiveDelay, inactiveDelay);
        stateTimer = 0f;
        orbitAngle = 0f;
        isUnlockedForRun = false;
        isCycleActive = false;
        lastDamageTimes.Clear();
        DestroySpawnedDoubloons();
    }

    private void TickCycle()
    {
        stateTimer -= Time.deltaTime;
        if (stateTimer > 0f)
        {
            return;
        }

        if (isCycleActive)
        {
            StartInactiveCycle();
        }
        else
        {
            StartActiveCycle();
        }
    }

    private void StartActiveCycle()
    {
        isCycleActive = true;
        stateTimer = currentActiveDuration;
        EnsureDoubloonCount();
        ShowDoubloons();
        UpdateOrbit();
    }

    private void StartInactiveCycle()
    {
        isCycleActive = false;
        stateTimer = currentInactiveDelay;
        HideDoubloons();
        lastDamageTimes.Clear();
    }

    private void EnsureDoubloonCount()
    {
        if (doubloonPrefab == null)
        {
            return;
        }

        while (spawnedDoubloons.Count < targetDoubloonCount)
        {
            GameObject doubloon = Instantiate(doubloonPrefab, transform);
            doubloon.name = $"Cursed Doubloon {spawnedDoubloons.Count + 1}";
            ConfigureDoubloon(doubloon);
            spawnedDoubloons.Add(doubloon.transform);
        }

        for (int i = 0; i < spawnedDoubloons.Count; i++)
        {
            if (spawnedDoubloons[i] != null)
            {
                spawnedDoubloons[i].gameObject.SetActive(isCycleActive && i < targetDoubloonCount);
            }
        }
    }

    private void ConfigureDoubloon(GameObject doubloon)
    {
        ResourcePickup pickup = doubloon.GetComponent<ResourcePickup>();
        if (pickup != null)
        {
            pickup.enabled = false;
            Destroy(pickup);
        }

        CursedDoubloonHitbox hitbox = doubloon.GetComponent<CursedDoubloonHitbox>();
        if (hitbox == null)
        {
            hitbox = doubloon.AddComponent<CursedDoubloonHitbox>();
        }
        hitbox.Initialize(this);

        Collider2D[] colliders = doubloon.GetComponentsInChildren<Collider2D>(true);
        if (colliders.Length == 0)
        {
            CircleCollider2D circle = doubloon.AddComponent<CircleCollider2D>();
            circle.radius = 0.35f;
            circle.isTrigger = true;
        }
        else
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].isTrigger = true;
            }
        }

        Rigidbody2D rb = doubloon.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = doubloon.AddComponent<Rigidbody2D>();
        }
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.simulated = true;
    }

    private void ShowDoubloons()
    {
        EnsureDoubloonCount();
        for (int i = 0; i < spawnedDoubloons.Count; i++)
        {
            if (spawnedDoubloons[i] != null)
            {
                spawnedDoubloons[i].gameObject.SetActive(i < targetDoubloonCount);
            }
        }
    }

    private void HideDoubloons()
    {
        for (int i = 0; i < spawnedDoubloons.Count; i++)
        {
            if (spawnedDoubloons[i] != null)
            {
                spawnedDoubloons[i].gameObject.SetActive(false);
            }
        }
    }

    private void DestroySpawnedDoubloons()
    {
        for (int i = spawnedDoubloons.Count - 1; i >= 0; i--)
        {
            if (spawnedDoubloons[i] != null)
            {
                Destroy(spawnedDoubloons[i].gameObject);
            }
        }
        spawnedDoubloons.Clear();
    }

    private void UpdateOrbit()
    {
        if (player == null)
        {
            return;
        }

        orbitAngle += spinSpeed * Time.deltaTime;
        int activeCount = Mathf.Min(targetDoubloonCount, spawnedDoubloons.Count);
        if (activeCount <= 0)
        {
            return;
        }

        for (int i = 0; i < activeCount; i++)
        {
            Transform doubloon = spawnedDoubloons[i];
            if (doubloon == null)
            {
                continue;
            }

            float angle = orbitAngle + 360f * i / activeCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0f) * Mathf.Max(0f, orbitRadius);
            doubloon.position = player.position + offset;
            doubloon.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    public void HandleDoubloonTrigger(Collider2D other)
    {
        if (!isUnlockedForRun || !isCycleActive || other == null)
        {
            return;
        }

        Cannonball projectile = other.GetComponentInParent<Cannonball>();
        if (projectile != null && !projectile.FiredByPlayer)
        {
            Destroy(projectile.gameObject);
            return;
        }

        ShipHealth health = other.GetComponentInParent<ShipHealth>();
        if (health == null || health.transform == player || health.CompareTag("PlayerShip"))
        {
            return;
        }

        if (lastDamageTimes.TryGetValue(health, out float lastDamageTime) && Time.time - lastDamageTime < enemyDamageCooldown)
        {
            return;
        }

        lastDamageTimes[health] = Time.time;
        health.TakeDamage(effectiveDamage);
    }
}

class CursedDoubloonHitbox : MonoBehaviour
{
    private CursedDoubloonsController controller;

    public void Initialize(CursedDoubloonsController owningController)
    {
        controller = owningController;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        controller?.HandleDoubloonTrigger(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        controller?.HandleDoubloonTrigger(other);
    }
}
