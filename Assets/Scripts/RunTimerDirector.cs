using TMPro;
using UnityEngine;

public class RunTimerDirector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text eventMessageText;

    [Header("Timing")]
    [SerializeField] private float secondSpawnerTime = 60f;
    [SerializeField] private float bossSpawnTime = 300f;
    [SerializeField] private bool timerEnabled = true;
    [SerializeField] private bool startTimerOnAwake = true;
    [SerializeField] private bool countOnlyWhenPlayerBoarded = true;
    [SerializeField] private bool logTimerDebug;

    [Header("Spawners")]
    [SerializeField] private MonoBehaviour[] normalSpawners;
    [SerializeField] private GameObject[] spawnerObjectsToEnableAtOneMinute;

    [Header("Boss")]
    [SerializeField] private GameObject bossShipPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform playerShip;
    [SerializeField] private float bossSpawnDistanceFromPlayer = 12f;

    [Header("Messages")]
    [SerializeField] private string oneMinuteMessage = "Stronger enemies incoming!";
    [SerializeField] private string bossIncomingMessage = "Boss ship incoming!";

    [Header("Runtime (Read-Only)")]
    [SerializeField] private float elapsedTime;
    [SerializeField] private bool secondSpawnerActivated;
    [SerializeField] private bool bossSpawned;
    [SerializeField] private GameObject spawnedBoss;

    private ShipController2D playerShipController;
    private bool timerRunning;
    private float nextDebugLogTime;

    private void Awake()
    {
        timerRunning = startTimerOnAwake;
        ResolvePlayerReferences();
        RefreshTimerText();

        if (eventMessageText != null)
        {
            eventMessageText.text = string.Empty;
        }
    }

    private void Update()
    {
        ResolvePlayerReferencesIfNeeded();

        if (logTimerDebug && Time.unscaledTime >= nextDebugLogTime)
        {
            nextDebugLogTime = Time.unscaledTime + 1f;
            Debug.Log(
                $"RunTimerDirector debug | timerEnabled={timerEnabled} timerRunning={timerRunning} countOnlyWhenPlayerBoarded={countOnlyWhenPlayerBoarded} " +
                $"shipControllerFound={(playerShipController != null)} playerOnBoard={(playerShipController != null && playerShipController.PlayerOnBoard)} " +
                $"timeScale={Time.timeScale:F2} deltaTime={Time.deltaTime:F3} elapsed={elapsedTime:F2}",
                this);
        }

        bool shouldCount = timerEnabled && timerRunning;

        if (shouldCount && countOnlyWhenPlayerBoarded)
        {
            if (playerShipController == null || !playerShipController.PlayerOnBoard)
            {
                shouldCount = false;
            }
        }

        if (shouldCount)
        {
            elapsedTime += Time.deltaTime;
        }

        RefreshTimerText();

        if (!secondSpawnerActivated && elapsedTime >= secondSpawnerTime)
        {
            ActivateSecondSpawnerWave();
        }

        if (!bossSpawned && elapsedTime >= bossSpawnTime)
        {
            TriggerBossPhase();
        }
    }

    public void StartTimer() => timerRunning = true;
    public void StopTimer() => timerRunning = false;

    private void RefreshTimerText()
    {
        if (timerText == null)
        {
            return;
        }

        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(elapsedTime));
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void ActivateSecondSpawnerWave()
    {
        secondSpawnerActivated = true;

        for (int i = 0; i < spawnerObjectsToEnableAtOneMinute.Length; i++)
        {
            GameObject spawnerObject = spawnerObjectsToEnableAtOneMinute[i];
            if (spawnerObject != null)
            {
                spawnerObject.SetActive(true);
            }
        }

        ShowEventMessage(oneMinuteMessage);
    }

    private void TriggerBossPhase()
    {
        bossSpawned = true;
        timerRunning = false;

        DisableNormalSpawners();
        ShowEventMessage(bossIncomingMessage);
        SpawnBoss();
    }

    private void DisableNormalSpawners()
    {
        for (int i = 0; i < normalSpawners.Length; i++)
        {
            MonoBehaviour spawner = normalSpawners[i];
            if (spawner != null)
            {
                spawner.enabled = false;
            }
        }
    }

    private void SpawnBoss()
    {
        if (bossShipPrefab == null)
        {
            Debug.LogWarning("RunTimerDirector: Boss ship prefab is not assigned.", this);
            return;
        }

        ResolvePlayerReferences();
        Vector3 spawnPosition = GetBossSpawnPosition();
        spawnedBoss = Instantiate(bossShipPrefab, spawnPosition, Quaternion.identity);

        ShipHealth playerHealth = null;
        if (playerShip != null)
        {
            playerHealth = playerShip.GetComponent<ShipHealth>() ?? playerShip.GetComponentInParent<ShipHealth>();
        }

        if (playerHealth == null && playerShipController != null)
        {
            playerHealth = playerShipController.GetComponent<ShipHealth>() ?? playerShipController.GetComponentInParent<ShipHealth>();
        }

        SimpleEnemyShipAI ai = spawnedBoss.GetComponent<SimpleEnemyShipAI>()
                               ?? spawnedBoss.GetComponentInChildren<SimpleEnemyShipAI>(true);
        if (ai != null)
        {
            ai.enabled = true;
            ai.Initialize(playerShip, playerShipController);
        }

        EnemyShipAttack attack = spawnedBoss.GetComponent<EnemyShipAttack>()
                                ?? spawnedBoss.GetComponentInChildren<EnemyShipAttack>(true);
        if (attack != null)
        {
            attack.enabled = true;
            attack.Initialize(playerShip, playerShipController, playerHealth);
        }

        BossDefeatHandler bossDefeatHandler = spawnedBoss.GetComponent<BossDefeatHandler>();
        if (bossDefeatHandler == null)
        {
            bossDefeatHandler = spawnedBoss.AddComponent<BossDefeatHandler>();
        }

        bossDefeatHandler.SetVictoryMessageText(eventMessageText);
    }

    private Vector3 GetBossSpawnPosition()
    {
        if (bossSpawnPoint != null)
        {
            return bossSpawnPoint.position;
        }

        if (playerShip != null)
        {
            return playerShip.position + (Vector3.up * bossSpawnDistanceFromPlayer);
        }

        return transform.position;
    }

    private void ShowEventMessage(string message)
    {
        if (eventMessageText != null)
        {
            eventMessageText.text = message;
            eventMessageText.gameObject.SetActive(true);
        }
    }

    private void ResolvePlayerReferencesIfNeeded()
    {
        if (playerShip == null || playerShipController == null)
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
    }
}
