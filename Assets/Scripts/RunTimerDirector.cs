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
    [SerializeField] private bool startTimerOnAwake = true;

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

    private bool timerRunning;

    private void Awake()
    {
        ResolvePlayerShipReference();
        timerRunning = startTimerOnAwake;
        RefreshTimerText();

        if (eventMessageText != null)
        {
            eventMessageText.text = string.Empty;
        }
    }

    private void Update()
    {
        if (!timerRunning)
        {
            return;
        }

        elapsedTime += Time.deltaTime;
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

    public void StartTimer()
    {
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

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

        ResolvePlayerShipReference();
        Vector3 spawnPosition = GetBossSpawnPosition();

        spawnedBoss = Instantiate(bossShipPrefab, spawnPosition, Quaternion.identity);

        ShipController2D playerController = playerShip != null
            ? playerShip.GetComponent<ShipController2D>()
            : FindFirstObjectByType<ShipController2D>();

        ShipHealth playerHealth = playerShip != null
            ? playerShip.GetComponent<ShipHealth>()
            : (playerController != null ? playerController.GetComponent<ShipHealth>() : null);

        if (playerHealth == null && playerController != null)
        {
            playerHealth = playerController.GetComponentInParent<ShipHealth>();
        }

        SimpleEnemyShipAI ai = spawnedBoss.GetComponent<SimpleEnemyShipAI>();
        if (ai == null)
        {
            ai = spawnedBoss.GetComponentInChildren<SimpleEnemyShipAI>(true);
        }

        if (ai != null)
        {
            ai.enabled = true;
            ai.Initialize(playerShip, playerController);
        }

        EnemyShipAttack attack = spawnedBoss.GetComponent<EnemyShipAttack>();
        if (attack == null)
        {
            attack = spawnedBoss.GetComponentInChildren<EnemyShipAttack>(true);
        }

        if (attack != null)
        {
            attack.enabled = true;
            attack.Initialize(playerShip, playerController, playerHealth);
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

    private void ResolvePlayerShipReference()
    {
        if (playerShip != null)
        {
            return;
        }

        GameObject taggedShip = GameObject.FindWithTag("PlayerShip");
        if (taggedShip != null)
        {
            playerShip = taggedShip.transform;
            return;
        }

        GameObject namedShip = GameObject.Find("PlayerShip");
        if (namedShip != null)
        {
            playerShip = namedShip.transform;
            return;
        }

        ShipController2D controller = FindFirstObjectByType<ShipController2D>();
        if (controller != null)
        {
            playerShip = controller.transform;
        }
    }
}
