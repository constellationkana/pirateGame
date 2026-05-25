using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;
    
    [Header("Drops")]
    [SerializeField] private GameObject woodPickupPrefab;
    [SerializeField] private GameObject doubloonPickupPrefab;
    [SerializeField] private int woodDropAmount = 1;
    [SerializeField] private int doubloonDropAmount = 1;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            SpawnDrops();
            Destroy(gameObject);
        }
    }

    private void SpawnDrops()
    {
        Vector3 spawnPosition = transform.position;

        if (woodPickupPrefab != null && woodDropAmount > 0)
        {
            for (int i = 0; i < woodDropAmount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-0.6f, 0.6f), 0f);
                Instantiate(woodPickupPrefab, spawnPosition + offset, Quaternion.identity);
            }
        }

        if (doubloonPickupPrefab != null && doubloonDropAmount > 0)
        {
            for (int i = 0; i < doubloonDropAmount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.6f, 0.6f), Random.Range(-0.6f, 0.6f), 0f);
                Instantiate(doubloonPickupPrefab, spawnPosition + offset, Quaternion.identity);
            }
        }
    }
}
