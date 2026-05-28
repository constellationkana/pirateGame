using System.Collections.Generic;
using UnityEngine;

public class EnemyShipSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject enemyShipPrefab;
    [SerializeField] private Transform playerShipTransform;
    [SerializeField] private ShipController2D playerShipController;
    [SerializeField] private ShipHealth playerShipHealth;

    [Header("Spawning")]
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField] private int maxEnemiesAlive = 5;
    [SerializeField] private float minSpawnDistanceFromPlayer = 8f;
    [SerializeField] private float maxSpawnDistanceFromPlayer = 15f;
    [SerializeField] private bool spawnOnlyWhenPlayerOnBoard = true;
    [SerializeField] private bool logSpawns = false;
    [SerializeField] private bool logSpawnedEnemySetup = false;

    [Header("Difficulty Scaling")]
    [SerializeField] private bool scaleDifficultyOverTime = true;
    [SerializeField] private float elapsedRunTime;
    [SerializeField] private float baseSpawnInterval = 8f;
    [SerializeField] private float minSpawnInterval = 1.5f;
    [SerializeField] private float spawnIntervalDecreasePerMinute = 0.5f;
    [SerializeField] private int baseMaxEnemiesAlive = 3;
    [SerializeField] private int maxEnemiesAliveIncreasePerMinute = 1;
    [SerializeField] private int absoluteMaxEnemiesAlive = 25;

    [Header("Runtime (Read-Only)")]
    [SerializeField] private float currentSpawnInterval;
    [SerializeField] private int currentMaxEnemiesAlive;

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private float nextSpawnTime;

    private void Awake()
    {
        ResolveReferences();
        ApplyDifficultyValues(true);

        if (enemyShipPrefab == null) Debug.LogWarning("EnemyShipSpawner: Enemy ship prefab is missing.", this);
        if (playerShipTransform == null) Debug.LogWarning("EnemyShipSpawner: PlayerShip reference is missing.", this);
        if (playerShipController == null) Debug.LogWarning("EnemyShipSpawner: PlayerShipController reference is missing.", this);
    }

    private void Update()
    {
        CleanupDestroyedEnemies();
        ResolveReferences();
        UpdateDifficultyTimer();
        ApplyDifficultyValues(false);

        if (enemyShipPrefab == null || playerShipTransform == null)
        {
            return;
        }

        if (spawnOnlyWhenPlayerOnBoard && (playerShipController == null || !playerShipController.PlayerOnBoard))
        {
            return;
        }

        if (aliveEnemies.Count >= currentMaxEnemiesAlive)
        {
            return;
        }

        if (Time.time < nextSpawnTime)
        {
            return;
        }

        SpawnEnemy();
        nextSpawnTime = Time.time + Mathf.Max(0.1f, currentSpawnInterval);
    }

    private void UpdateDifficultyTimer()
    {
        if (!scaleDifficultyOverTime)
        {
            return;
        }

        elapsedRunTime += Time.deltaTime;
    }

    private void ApplyDifficultyValues(bool force)
    {
        if (!scaleDifficultyOverTime)
        {
            if (force)
            {
                currentSpawnInterval = spawnInterval;
                currentMaxEnemiesAlive = maxEnemiesAlive;
            }

            return;
        }

        float elapsedMinutes = elapsedRunTime / 60f;

        float targetSpawnInterval = Mathf.Max(
            minSpawnInterval,
            baseSpawnInterval - (spawnIntervalDecreasePerMinute * elapsedMinutes));

        int targetMaxEnemies = Mathf.Min(
            absoluteMaxEnemiesAlive,
            baseMaxEnemiesAlive + Mathf.FloorToInt(maxEnemiesAliveIncreasePerMinute * elapsedMinutes));

        currentSpawnInterval = targetSpawnInterval;
        currentMaxEnemiesAlive = targetMaxEnemies;
    }

    private void ResolveReferences()
    {
        if (playerShipTransform == null)
        {
            GameObject taggedShip = GameObject.FindWithTag("PlayerShip");
            if (taggedShip != null)
            {
                playerShipTransform = taggedShip.transform;
            }
        }

        if (playerShipTransform == null)
        {
            GameObject namedShip = GameObject.Find("PlayerShip");
            if (namedShip != null)
            {
                playerShipTransform = namedShip.transform;
            }
        }

        if (playerShipController == null)
        {
            playerShipController = FindFirstObjectByType<ShipController2D>();
            if (playerShipTransform == null && playerShipController != null)
            {
                playerShipTransform = playerShipController.transform;
            }
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
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        if (randomDirection.sqrMagnitude < 0.001f)
        {
            randomDirection = Vector2.up;
        }

        float minDistance = Mathf.Max(0f, minSpawnDistanceFromPlayer);
        float maxDistance = Mathf.Max(minDistance, maxSpawnDistanceFromPlayer);
        float spawnDistance = Random.Range(minDistance, maxDistance);

        Vector3 spawnPosition = playerShipTransform.position + (Vector3)(randomDirection * spawnDistance);

        GameObject spawnedEnemy = Instantiate(enemyShipPrefab, spawnPosition, Quaternion.identity);
        aliveEnemies.Add(spawnedEnemy);

        SimpleEnemyShipAI ai = spawnedEnemy.GetComponent<SimpleEnemyShipAI>();
        if (ai == null)
        {
            ai = spawnedEnemy.GetComponentInChildren<SimpleEnemyShipAI>(true);
        }

        if (ai != null)
        {
            ai.enabled = true;
            ai.Initialize(playerShipTransform, playerShipController);
        }

        EnemyShipAttack attack = spawnedEnemy.GetComponent<EnemyShipAttack>();
        if (attack == null)
        {
            attack = spawnedEnemy.GetComponentInChildren<EnemyShipAttack>(true);
        }

        if (attack != null)
        {
            attack.enabled = true;
            attack.Initialize(playerShipTransform, playerShipController, playerShipHealth);
        }

        if (logSpawnedEnemySetup)
        {
            bool referencesAssigned = playerShipTransform != null && playerShipController != null && playerShipHealth != null;
            Debug.Log(
                $"EnemyShipSpawner setup: spawned={spawnedEnemy.name}, aiFound={(ai != null)}, aiEnabled={(ai != null && ai.enabled)}, " +
                $"attackFound={(attack != null)}, attackEnabled={(attack != null && attack.enabled)}, refsAssigned={referencesAssigned}",
                spawnedEnemy);
        }

        if (logSpawns)
        {
            Debug.Log($"EnemyShipSpawner: Spawned enemy at {spawnPosition}. Alive: {aliveEnemies.Count}, interval={currentSpawnInterval:F2}, maxAlive={currentMaxEnemiesAlive}", this);
        }
    }

    private void CleanupDestroyedEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }
    }
}
