using UnityEngine;

public class ShipDeathDropper : MonoBehaviour
{
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private GameObject woodPickupPrefab;
    [SerializeField] private int minDrops = 1;
    [SerializeField] private int maxDrops = 3;
    [SerializeField] private float dropScatterRadius = 0.75f;

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
        if (woodPickupPrefab == null)
        {
            return;
        }

        int clampedMin = Mathf.Max(0, minDrops);
        int clampedMax = Mathf.Max(clampedMin, maxDrops);
        int dropCount = Random.Range(clampedMin, clampedMax + 1);

        for (int i = 0; i < dropCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropScatterRadius;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            Instantiate(woodPickupPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
