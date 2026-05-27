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

    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private float nextSpawnTime;

    private void Awake()
    {
        if (enemyShipPrefab == null) Debug.LogWarning("EnemyShipSpawner: Enemy ship prefab is missing.", this);
        if (playerShip == null) Debug.LogWarning("EnemyShipSpawner: PlayerShip reference is missing.", this);
        if (playerShipController == null) Debug.LogWarning("EnemyShipSpawner: PlayerShipController reference is missing.", this);
    }

    private void Update()
    {
        CleanupDestroyedEnemies();
        ResolveReferences();

        if (enemyShipPrefab == null || playerShip == null)
        {
            return;

        if (spawnOnlyWhenPlayerOnBoard && (playerShipController == null || !playerShipController.PlayerOnBoard))
            return;

        if (aliveEnemies.Count >= maxEnemiesAlive)
            return;

        if (Time.time < nextSpawnTime)
            return;

        SpawnEnemy();
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
    }

    private void SpawnEnemy()
    {
        // Pick a random direction around the player.
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        if (randomDirection.sqrMagnitude < 0.001f)
        {
            GameObject tagged = GameObject.FindWithTag("PlayerShip");
            if (tagged != null) playerShipTransform = tagged.transform;
        }

        // Pick a random distance between min/max values so we avoid spawning too close.
        float minDistance = Mathf.Max(0f, minSpawnDistanceFromPlayer);
        float maxDistance = Mathf.Max(minDistance, maxSpawnDistanceFromPlayer);
        float spawnDistance = Random.Range(minDistance, maxDistance);

        Vector3 spawnPosition = playerShipTransform.position + (Vector3)(randomDirection * spawnDistance);

        GameObject spawnedEnemy = Instantiate(enemyShipPrefab, spawnPosition, Quaternion.identity);
        aliveEnemies.Add(spawnedEnemy);

        if (logSpawns)
        {
            Debug.Log($"EnemyShipSpawner: Spawned enemy at {spawnPosition}. Alive: {aliveEnemies.Count}", this);
        }
    }

    private void CleanupDestroyedEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
                aliveEnemies.RemoveAt(i);
        }
    }
}