using UnityEngine;

public class ShipDeathDropper : MonoBehaviour
{
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private GameObject[] dropPrefabs;
    [SerializeField] private int minDrops = 1;
    [SerializeField] private int maxDrops = 3;
    [SerializeField] private float dropScatterRadius = 1f;

    private void OnEnable()
    {
        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
        }

        if (shipHealth != null)
        {
            shipHealth.OnDeath += OnShipDeath;
        }
    }

    private void OnDisable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnDeath -= OnShipDeath;
        }
    }

    private void OnShipDeath(ShipHealth _)
    {
        if (dropPrefabs == null || dropPrefabs.Length == 0)
        {
            return;
        }

        int dropCount = Random.Range(minDrops, maxDrops + 1);
        for (int i = 0; i < dropCount; i++)
        {
            GameObject prefab = dropPrefabs[Random.Range(0, dropPrefabs.Length)];
            if (prefab == null)
            {
                continue;
            }

            Vector2 scatter = Random.insideUnitCircle * dropScatterRadius;
            Vector3 spawnPos = transform.position + new Vector3(scatter.x, scatter.y, 0f);
            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}
