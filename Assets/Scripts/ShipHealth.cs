using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;

    [Header("Resource Drops")]
    [SerializeField] private GameObject woodPickupPrefab;
    [SerializeField] private GameObject doubloonPickupPrefab;
    [SerializeField] private int woodDropAmount = 2;
    [SerializeField] private int doubloonDropAmount = 1;
    [SerializeField] private float dropSpread = 0.75f;

    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || isDead)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        SpawnDrops(woodPickupPrefab, woodDropAmount);
        SpawnDrops(doubloonPickupPrefab, doubloonDropAmount);

        Destroy(gameObject);
    }

    private void SpawnDrops(GameObject pickupPrefab, int amount)
    {
        if (pickupPrefab == null || amount <= 0)
        {
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropSpread;
            Vector3 spawnPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);

            Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
        }
    }
}
