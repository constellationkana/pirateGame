using UnityEngine;

public class EnemyShipSpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField] private int maxSpawnedEnemies = 6;

    [Header("Player Reference (Optional)")]
    [SerializeField] private Transform playerShipTransform;

    private ShipController2D playerShipController;
    private ShipHealth playerShipHealth;
    private float nextSpawnTime;
    private int nextSpawnIndex;

    private void Awake()
    {
        ResolvePlayerReferences();
    }

    private void Update()
    {
        if (enemyPrefab == null)
        {
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        if (maxSpawnedEnemies > 0 && CountLiveSpawnedEnemies() >= maxSpawnedEnemies)
        {
            nextSpawnTime = Time.time + 1f;
            return;
        }

        ResolvePlayerReferences();
        SpawnEnemy();
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
    }

    private void ResolvePlayerReferences()
    {
        if (playerShipTransform == null)
        {
            GameObject taggedShip = GameObject.FindWithTag("PlayerShip");
            if (taggedShip != null)
            {
                playerShipTransform = taggedShip.transform;
            }
            else
            {
                GameObject namedShip = GameObject.Find("PlayerShip");
                if (namedShip != null)
                {
                    playerShipTransform = namedShip.transform;
                }
            }

            if (playerShipTransform == null)
            {
                ShipController2D fallbackController = FindFirstObjectByType<ShipController2D>();
                if (fallbackController != null)
                {
                    playerShipTransform = fallbackController.transform;
                }
            }
        }

        if (playerShipController == null && playerShipTransform != null)
        {
            playerShipController = playerShipTransform.GetComponent<ShipController2D>();
        }

        if (playerShipHealth == null && playerShipTransform != null)
        {
            playerShipHealth = playerShipTransform.GetComponent<ShipHealth>();
            if (playerShipHealth == null)
            {
                playerShipHealth = playerShipTransform.GetComponentInParent<ShipHealth>();
            }
        }
    }

    private void SpawnEnemy()
    {
        Vector3 spawnPosition = transform.position;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform point = spawnPoints[nextSpawnIndex % spawnPoints.Length];
            if (point != null)
            {
                spawnPosition = point.position;
            }

            nextSpawnIndex++;
        }

        GameObject spawnedEnemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        SimpleEnemyShipAI ai = spawnedEnemy.GetComponent<SimpleEnemyShipAI>();
        if (ai == null)
        {
            ai = spawnedEnemy.GetComponentInChildren<SimpleEnemyShipAI>();
        }

        if (ai != null)
        {
            ai.Initialize(playerShipTransform, playerShipController);
        }

        EnemyShipAttack attack = spawnedEnemy.GetComponent<EnemyShipAttack>();
        if (attack == null)
        {
            attack = spawnedEnemy.GetComponentInChildren<EnemyShipAttack>();
        }

        if (attack != null)
        {
            attack.Initialize(playerShipTransform, playerShipController, playerShipHealth);
        }
    }

    private int CountLiveSpawnedEnemies()
    {
        SimpleEnemyShipAI[] enemies = FindObjectsByType<SimpleEnemyShipAI>(FindObjectsSortMode.None);
        return enemies != null ? enemies.Length : 0;
    }
}
