using UnityEngine;

/// <summary>
/// Drops configured resources when a ship health component dies.
/// </summary>
public class ShipDeathDropper : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ShipHealth shipHealth;

    [Header("Wood Drops")]
    [SerializeField] private GameObject woodPickupPrefab;
    [SerializeField] private int minWoodDrops = 1;
    [SerializeField] private int maxWoodDrops = 3;

    [Header("Doubloon Drops")]
    [SerializeField] private GameObject doubloonPickupPrefab;
    [SerializeField] private int minDoubloonDrops = 1;
    [SerializeField] private int maxDoubloonDrops = 4;

    [Header("XP Drops")]
    [SerializeField] private GameObject xpPickupPrefab;
    [SerializeField] private int minXPDrops = 1;
    [SerializeField] private int maxXPDrops = 3;
    [SerializeField] private int xpAmountPerPickup = 1;

    [Header("Treasure Chest Drops")]
    [SerializeField] private GameObject treasureChestPrefab;
    [SerializeField] private int minTreasureChestDrops = 0;
    [SerializeField] private int maxTreasureChestDrops = 0;
    [Range(0f, 1f)]
    [SerializeField] private float treasureChestDropChance = 0f;

    [Header("Drop Scatter")]
    [SerializeField] private float dropScatterRadius = 0.75f;
    [SerializeField] private bool logDrops;

    private void Awake()
    {
        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
        }
    }

    private void OnEnable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnDeath -= HandleDeath;
        }
    }

    private void HandleDeath(ShipHealth health)
    {
        int woodDropCount = SpawnDrops(woodPickupPrefab, minWoodDrops, maxWoodDrops);
        int doubloonDropCount = SpawnDrops(doubloonPickupPrefab, minDoubloonDrops, maxDoubloonDrops);
        int xpDropCount = SpawnDrops(xpPickupPrefab, minXPDrops, maxXPDrops, true);
        int treasureChestDropCount = SpawnTreasureChestDrops();

        if (logDrops)
        {
            Debug.Log($"Dropped {woodDropCount} wood, {doubloonDropCount} doubloons, {xpDropCount} XP pickups, and {treasureChestDropCount} treasure chests from {gameObject.name}.", this);
        }
    }

    private int SpawnTreasureChestDrops()
    {
        if (treasureChestPrefab == null || Mathf.Clamp01(treasureChestDropChance) <= 0f || Random.value > Mathf.Clamp01(treasureChestDropChance))
        {
            return 0;
        }

        return SpawnDrops(treasureChestPrefab, minTreasureChestDrops, maxTreasureChestDrops);
    }

    private int SpawnDrops(GameObject pickupPrefab, int minDrops, int maxDrops, bool assignXPAmount = false)
    {
        if (pickupPrefab == null)
        {
            return 0;
        }

        int clampedMin = Mathf.Max(0, minDrops);
        int clampedMax = Mathf.Max(clampedMin, maxDrops);
        int dropCount = Random.Range(clampedMin, clampedMax + 1);

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropScatterRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            GameObject pickup = Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
            if (assignXPAmount)
            {
                ResourcePickup resourcePickup = pickup.GetComponent<ResourcePickup>();
                if (resourcePickup != null)
                {
                    resourcePickup.SetAmount(xpAmountPerPickup);
                }
            }
        }

        return dropCount;
    }
}
