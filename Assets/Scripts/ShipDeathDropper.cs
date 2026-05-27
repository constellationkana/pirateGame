using UnityEngine;

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

        if (logDrops)
        {
            Debug.Log($"Dropped {woodDropCount} wood and {doubloonDropCount} doubloons from {gameObject.name}.", this);
        }
    }

    private int SpawnDrops(GameObject pickupPrefab, int minDrops, int maxDrops)
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
            Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
        }

        return dropCount;
    }
}
