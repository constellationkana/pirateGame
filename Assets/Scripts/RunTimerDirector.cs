using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Starts and tracks run time, timed spawner phases, boss spawning, and run event messages.
/// </summary>
public class RunTimerDirector : MonoBehaviour
{
    /// <summary>
    /// Defines when the run timer and normal spawners should begin.
    /// </summary>
    public enum RunStartMode
    {
        /// <summary>
        /// Starts after the player ship reports that the player is onboard.
        /// </summary>
        StartWhenPlayerBoards,
        /// <summary>
        /// Waits for StartRun or StartTimer to be called by UI or another script.
        /// </summary>
        StartFromButton,
        /// <summary>
        /// Starts as soon as the director initializes.
        /// </summary>
        StartImmediately
    }

    /// <summary>
    /// Configures spawner objects and components to toggle at a run-time threshold.
    /// </summary>
    [System.Serializable]
    public class TimedSpawnerEvent
    {
        /// <summary>
        /// Display/debug name for this timed spawner event.
        /// </summary>
        public string eventName;
        /// <summary>
        /// Elapsed run time required before this event triggers.
        /// </summary>
        public float triggerTimeSeconds;
        /// <summary>
        /// Spawner GameObjects enabled when this event triggers.
        /// </summary>
        public GameObject[] spawnerObjectsToEnable;
        /// <summary>
        /// Spawner components enabled when this event triggers.
        /// </summary>
        public MonoBehaviour[] spawnerComponentsToEnable;
        /// <summary>
        /// Spawner GameObjects disabled when this event triggers.
        /// </summary>
        public GameObject[] spawnerObjectsToDisable;
        /// <summary>
        /// Spawner components disabled when this event triggers.
        /// </summary>
        public MonoBehaviour[] spawnerComponentsToDisable;
        /// <summary>
        /// Message shown when this timed event triggers.
        /// </summary>
        public string eventMessage;
        /// <summary>
        /// Tracks whether this event has already fired during the current run.
        /// </summary>
        [HideInInspector] public bool triggered;
    }

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text eventMessageText;
    [SerializeField] private BossHealthBarUI bossHealthBarUI;

    [Header("Timing")]
    [SerializeField] private float secondSpawnerTime = 60f;
    [SerializeField] private float bossSpawnTime = 300f;
    [SerializeField] private RunStartMode runStartMode = RunStartMode.StartWhenPlayerBoards;
    [SerializeField] private bool timerEnabled = true;
    [SerializeField] private bool startTimerOnAwake = true;
    [SerializeField] private bool countOnlyWhenPlayerBoarded = true;
    [SerializeField] private bool logTimerDebug;
    [SerializeField] private bool logSpawnerEvents;

    [Header("Spawners")]
    [SerializeField] private TimedSpawnerEvent[] timedSpawnerEvents;
    [SerializeField] private MonoBehaviour[] normalSpawners;
    [SerializeField] private GameObject[] spawnerObjectsToEnableAtOneMinute;

    [Header("Boss")]
    [SerializeField] private GameObject bossShipPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private Transform playerShip;
    [SerializeField] private float bossSpawnDistanceFromPlayer = 12f;
    [SerializeField] private string bossDisplayName = "Dread Summoner";

    [Header("Messages")]
    [SerializeField] private float eventMessageDuration = 10f;
    [SerializeField] private string oneMinuteMessage = "Stronger enemies incoming!";
    [SerializeField] private string bossIncomingMessage = "Boss ship incoming!";

    [Header("Runtime (Read-Only)")]
    [SerializeField] private float elapsedTime;
    [SerializeField] private bool secondSpawnerActivated;
    [SerializeField] private bool bossSpawned;
    [SerializeField] private bool runStarted;
    [SerializeField] private GameObject spawnedBoss;

    private ShipController2D playerShipController;
    private Coroutine clearEventMessageCoroutine;
    private bool timerRunning;
    private float nextDebugLogTime;

    /// <summary>
    /// Gets the current elapsed run time in seconds.
    /// </summary>
    public float ElapsedTime => elapsedTime;
    /// <summary>
    /// Gets the configured run start mode.
    /// </summary>
    public RunStartMode CurrentRunStartMode => runStartMode;
    /// <summary>
    /// Gets whether the run has started.
    /// </summary>
    public bool RunStarted => runStarted;

    private void Awake()
    {
        InitializeRunState();
        ResolvePlayerReferences();
        RefreshTimerText();
        ResetTimedSpawnerEvents();

        if (runStarted)
        {
            EnableNormalSpawners();
        }
        else
        {
            DisableNormalSpawners();
        }

        if (eventMessageText != null)
        {
            eventMessageText.text = string.Empty;
        }

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetBoss(null, bossDisplayName);
        }
    }

    private void Update()
    {
        ResolvePlayerReferencesIfNeeded();

        if (runStartMode == RunStartMode.StartWhenPlayerBoards && startTimerOnAwake && !runStarted && playerShipController != null && playerShipController.PlayerOnBoard)
        {
            StartRun();
        }

        if (logTimerDebug && Time.unscaledTime >= nextDebugLogTime)
        {
            nextDebugLogTime = Time.unscaledTime + 1f;
            Debug.Log(
                $"RunTimerDirector debug | runStartMode={runStartMode} runStarted={runStarted} timerEnabled={timerEnabled} timerRunning={timerRunning} " +
                $"countOnlyWhenPlayerBoarded={countOnlyWhenPlayerBoarded} shipControllerFound={(playerShipController != null)} " +
                $"playerOnBoard={(playerShipController != null && playerShipController.PlayerOnBoard)} timeScale={Time.timeScale:F2} " +
                $"deltaTime={Time.deltaTime:F3} elapsed={elapsedTime:F2}",
                this);
        }

        bool shouldCount = ShouldCountRunTime();

        if (shouldCount)
        {
            elapsedTime += Time.deltaTime;
        }

        RefreshTimerText();

        if (!runStarted)
        {
            return;
        }

        CheckTimedSpawnerEvents();

        if (!bossSpawned && elapsedTime >= bossSpawnTime)
        {
            TriggerBossPhase();
        }
    }

    /// <summary>
    /// Starts run flow for this controller.
    /// </summary>
    public void StartRun()
    {
        if (runStarted)
        {
            timerRunning = true;
            return;
        }

        runStarted = true;
        timerRunning = true;
        EnableNormalSpawners();

        if (logTimerDebug)
        {
            Debug.Log("RunTimerDirector: Run started.", this);
        }
    }

    /// <summary>
    /// Compatibility wrapper that starts the run timer.
    /// </summary>
    public void StartTimer() => StartRun();
    /// <summary>
    /// Stops elapsed-time counting without clearing the run-started state.
    /// </summary>
    public void StopTimer() => timerRunning = false;

    private void InitializeRunState()
    {
        switch (runStartMode)
        {
            case RunStartMode.StartFromButton:
                runStarted = false;
                timerRunning = false;
                break;
            case RunStartMode.StartImmediately:
                runStarted = true;
                timerRunning = true;
                break;
            case RunStartMode.StartWhenPlayerBoards:
            default:
                runStarted = false;
                timerRunning = false;
                break;
        }
    }

    private bool ShouldCountRunTime()
    {
        if (!timerEnabled || !timerRunning || !runStarted)
        {
            return false;
        }

        if (countOnlyWhenPlayerBoarded && (playerShipController == null || !playerShipController.PlayerOnBoard))
        {
            return false;
        }

        return true;
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

    private void ResetTimedSpawnerEvents()
    {
        if (timedSpawnerEvents == null)
        {
            return;
        }

        for (int i = 0; i < timedSpawnerEvents.Length; i++)
        {
            if (timedSpawnerEvents[i] != null)
            {
                timedSpawnerEvents[i].triggered = false;
            }
        }
    }

    private void CheckTimedSpawnerEvents()
    {
        // Prefer configured timed events; fall back to the older one-minute spawner hook for existing scenes.
        if (timedSpawnerEvents == null || timedSpawnerEvents.Length == 0)
        {
            CheckLegacySecondSpawnerEvent();
            return;
        }

        for (int i = 0; i < timedSpawnerEvents.Length; i++)
        {
            TimedSpawnerEvent timedEvent = timedSpawnerEvents[i];
            if (timedEvent == null || timedEvent.triggered || elapsedTime < timedEvent.triggerTimeSeconds)
            {
                continue;
            }

            TriggerTimedSpawnerEvent(timedEvent);
        }
    }

    private void CheckLegacySecondSpawnerEvent()
    {
        if (!secondSpawnerActivated && elapsedTime >= secondSpawnerTime)
        {
            secondSpawnerActivated = true;
            int objectsEnabled = EnableGameObjects(spawnerObjectsToEnableAtOneMinute);
            ShowEventMessage(oneMinuteMessage);

            if (logSpawnerEvents)
            {
                Debug.Log(
                    $"RunTimerDirector legacy spawner event triggered | eventName=Legacy Second Spawner triggerTimeSeconds={secondSpawnerTime:F2} " +
                    $"elapsedTime={elapsedTime:F2} objectsEnabled={objectsEnabled} componentsEnabled=0 objectsDisabled=0 componentsDisabled=0",
                    this);
            }
        }
    }

    private void TriggerTimedSpawnerEvent(TimedSpawnerEvent timedEvent)
    {
        timedEvent.triggered = true;

        int objectsEnabled = EnableGameObjects(timedEvent.spawnerObjectsToEnable);
        int componentsEnabled = EnableComponents(timedEvent.spawnerComponentsToEnable);
        int objectsDisabled = DisableGameObjects(timedEvent.spawnerObjectsToDisable);
        int componentsDisabled = DisableComponents(timedEvent.spawnerComponentsToDisable);

        if (!string.IsNullOrEmpty(timedEvent.eventMessage))
        {
            ShowEventMessage(timedEvent.eventMessage);
        }

        if (logSpawnerEvents)
        {
            Debug.Log(
                $"RunTimerDirector spawner event triggered | eventName={timedEvent.eventName} triggerTimeSeconds={timedEvent.triggerTimeSeconds:F2} " +
                $"elapsedTime={elapsedTime:F2} objectsEnabled={objectsEnabled} componentsEnabled={componentsEnabled} " +
                $"objectsDisabled={objectsDisabled} componentsDisabled={componentsDisabled}",
                this);
        }
    }

    private int EnableGameObjects(GameObject[] gameObjects)
    {
        int changedCount = 0;

        if (gameObjects == null)
        {
            return changedCount;
        }

        for (int i = 0; i < gameObjects.Length; i++)
        {
            GameObject gameObjectToEnable = gameObjects[i];
            if (gameObjectToEnable != null)
            {
                gameObjectToEnable.SetActive(true);
                changedCount++;
            }
        }

        return changedCount;
    }

    private int EnableComponents(MonoBehaviour[] components)
    {
        int changedCount = 0;

        if (components == null)
        {
            return changedCount;
        }

        for (int i = 0; i < components.Length; i++)
        {
            MonoBehaviour component = components[i];
            if (component != null)
            {
                component.enabled = true;
                changedCount++;
            }
        }

        return changedCount;
    }

    private int DisableGameObjects(GameObject[] gameObjects)
    {
        int changedCount = 0;

        if (gameObjects == null)
        {
            return changedCount;
        }

        for (int i = 0; i < gameObjects.Length; i++)
        {
            GameObject gameObjectToDisable = gameObjects[i];
            if (gameObjectToDisable != null)
            {
                gameObjectToDisable.SetActive(false);
                changedCount++;
            }
        }

        return changedCount;
    }

    private int DisableComponents(MonoBehaviour[] components)
    {
        int changedCount = 0;

        if (components == null)
        {
            return changedCount;
        }

        for (int i = 0; i < components.Length; i++)
        {
            MonoBehaviour component = components[i];
            if (component != null)
            {
                component.enabled = false;
                changedCount++;
            }
        }

        return changedCount;
    }

    private void EnableNormalSpawners()
    {
        EnableComponents(normalSpawners);
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
        DisableComponents(normalSpawners);
    }

    private void SpawnBoss()
    {
        // Boss prefabs can vary by scene, so references are resolved defensively after instantiation.
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

        ShipHealth bossHealth = spawnedBoss.GetComponent<ShipHealth>()
                                ?? spawnedBoss.GetComponentInChildren<ShipHealth>(true);

        if (bossHealthBarUI != null)
        {
            bossHealthBarUI.SetBoss(bossHealth, bossDisplayName);
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
        if (eventMessageText == null)
        {
            return;
        }

        eventMessageText.text = message;
        eventMessageText.gameObject.SetActive(true);

        if (clearEventMessageCoroutine != null)
        {
            StopCoroutine(clearEventMessageCoroutine);
        }

        clearEventMessageCoroutine = StartCoroutine(ClearEventMessageAfterDelay(message));
    }

    private IEnumerator ClearEventMessageAfterDelay(string messageToClear)
    {
        if (eventMessageDuration > 0f)
        {
            yield return new WaitForSeconds(eventMessageDuration);
        }

        if (eventMessageText != null && eventMessageText.text == messageToClear)
        {
            eventMessageText.text = string.Empty;
            eventMessageText.gameObject.SetActive(false);
        }

        clearEventMessageCoroutine = null;
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
