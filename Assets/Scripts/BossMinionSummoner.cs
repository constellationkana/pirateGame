using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BossMinionSummoner : MonoBehaviour
{
    [Header("Summoning")]
    [SerializeField] private GameObject[] minionPrefabs;
    [SerializeField] private Transform[] summonPoints;
    [SerializeField] private Transform playerShip;
    [SerializeField] private float summonInterval = 8f;
    [SerializeField] private int minionsPerWave = 2;
    [SerializeField] private int maxAliveMinions = 5;
    [SerializeField] private float summonRadius = 5f;
    [SerializeField] private bool summonOnlyWhenPlayerBoarded = true;
    [SerializeField] private bool logSummons = false;

    [Header("Runtime (Read-Only)")]
    [SerializeField] private int aliveMinionCount;

    private readonly List<GameObject> aliveMinions = new List<GameObject>();
    private ShipController2D playerShipController;
    private ShipHealth playerShipHealth;
    private float nextSummonTime;

    private void Awake()
    {
        ResolvePlayerReferences();
        nextSummonTime = Time.time + Mathf.Max(0.1f, summonInterval);
    }

    public void Initialize(Transform newPlayerShip, ShipController2D newPlayerShipController, ShipHealth newPlayerShipHealth)
    {
        playerShip = newPlayerShip;
        playerShipController = newPlayerShipController;
        playerShipHealth = newPlayerShipHealth;
    }

    private void Update()
    {
        CleanupDeadMinions();
        ResolvePlayerReferencesIfNeeded();

        if (minionPrefabs == null || minionPrefabs.Length == 0)
        {
            return;
        }

        if (summonOnlyWhenPlayerBoarded && (playerShipController == null || !playerShipController.PlayerOnBoard))
        {
            return;
        }

        if (aliveMinions.Count >= Mathf.Max(0, maxAliveMinions))
        {
            return;
        }

        if (Time.time < nextSummonTime)
        {
            return;
        }

        SummonWave();
        nextSummonTime = Time.time + Mathf.Max(0.1f, summonInterval);
    }

    private void SummonWave()
    {
        int limit = Mathf.Max(0, maxAliveMinions);
        int waveCount = Mathf.Max(0, minionsPerWave);

        for (int i = 0; i < waveCount; i++)
        {
            if (aliveMinions.Count >= limit)
            {
                break;
            }

            GameObject prefab = PickMinionPrefab();
            if (prefab == null)
            {
                continue;
            }

            Vector3 spawnPosition = GetSummonPosition(i);
            GameObject minion = Instantiate(prefab, spawnPosition, Quaternion.identity);
            aliveMinions.Add(minion);
            InitializeMinion(minion);

            if (logSummons)
            {
                Debug.Log($"BossMinionSummoner: Spawned minion {minion.name} at {spawnPosition}.", this);
            }
        }

        aliveMinionCount = aliveMinions.Count;
    }

    private GameObject PickMinionPrefab()
    {
        if (minionPrefabs == null || minionPrefabs.Length == 0)
        {
            return null;
        }

        int index = Random.Range(0, minionPrefabs.Length);
        return minionPrefabs[index];
    }

    private Vector3 GetSummonPosition(int waveIndex)
    {
        if (summonPoints != null && summonPoints.Length > 0)
        {
            Transform point = summonPoints[waveIndex % summonPoints.Length];
            if (point != null)
            {
                return point.position;
            }
        }

        Vector2 offset2D = Random.insideUnitCircle;
        if (offset2D.sqrMagnitude < 0.001f)
        {
            offset2D = Vector2.up;
        }

        Vector3 offset = new Vector3(offset2D.x, offset2D.y, 0f).normalized * Mathf.Max(0.1f, summonRadius);
        return transform.position + offset;
    }

    private void InitializeMinion(GameObject minion)
    {
        SimpleEnemyShipAI ai = minion.GetComponent<SimpleEnemyShipAI>();
        if (ai == null)
        {
            ai = minion.GetComponentInChildren<SimpleEnemyShipAI>(true);
        }

        if (ai != null)
        {
            ai.enabled = true;
            ai.Initialize(playerShip, playerShipController);
        }

        EnemyShipAttack attack = minion.GetComponent<EnemyShipAttack>();
        if (attack == null)
        {
            attack = minion.GetComponentInChildren<EnemyShipAttack>(true);
        }

        if (attack != null)
        {
            attack.enabled = true;
            attack.Initialize(playerShip, playerShipController, playerShipHealth);
        }
    }

    private void CleanupDeadMinions()
    {
        for (int i = aliveMinions.Count - 1; i >= 0; i--)
        {
            if (aliveMinions[i] == null)
            {
                aliveMinions.RemoveAt(i);
            }
        }

        aliveMinionCount = aliveMinions.Count;
    }

    private void ResolvePlayerReferencesIfNeeded()
    {
        if (playerShip == null || playerShipController == null || playerShipHealth == null)
        {
            ResolvePlayerReferences();
        }
    }

    private void ResolvePlayerReferences()
    {
        if (playerShip == null)
        {
            GameObject taggedShip = GameObject.FindWithTag("PlayerShip");
            if (taggedShip != null)
            {
                playerShip = taggedShip.transform;
            }
        }

        if (playerShip == null)
        {
            GameObject namedShip = GameObject.Find("PlayerShip");
            if (namedShip != null)
            {
                playerShip = namedShip.transform;
            }
        }

        if (playerShipController == null && playerShip != null)
        {
            playerShipController = playerShip.GetComponent<ShipController2D>();
            if (playerShipController == null)
            {
                playerShipController = playerShip.GetComponentInParent<ShipController2D>();
            }
        }

        if (playerShipController == null)
        {
            playerShipController = FindFirstObjectByType<ShipController2D>();
            if (playerShip == null && playerShipController != null)
            {
                playerShip = playerShipController.transform;
            }
        }

        if (playerShipHealth == null && playerShip != null)
        {
            playerShipHealth = playerShip.GetComponent<ShipHealth>();
            if (playerShipHealth == null)
            {
                playerShipHealth = playerShip.GetComponentInParent<ShipHealth>();
            }
        }

        if (playerShipHealth == null && playerShipController != null)
        {
            playerShipHealth = playerShipController.GetComponent<ShipHealth>();
            if (playerShipHealth == null)
            {
                playerShipHealth = playerShipController.GetComponentInParent<ShipHealth>();
            }
        }
    }
}
