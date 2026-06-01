using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum RunSummaryType
{
    Death,
    StageComplete
}

[DisallowMultipleComponent]
public class RunSummaryController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject summaryRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text summaryText;
    [SerializeField] private TMP_Text bonusText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button shipShopButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Scene Navigation")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string shipShopSceneName = "ShipShop";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string retrySceneNameOverride = "";

    [Header("Summary Behavior")]
    [SerializeField] private bool pauseTimeOnSummary = true;
    [SerializeField] private bool awardTimeBonusOnDeath = true;
    [SerializeField] private bool showSummaryOnStageComplete = true;
    [SerializeField] private bool awardTimeBonusOnStageComplete = true;
    [SerializeField] private int stageCompleteBonusDoubloons = 0;
    [SerializeField] private string deathTitle = "Run Over";
    [SerializeField] private string stageCompleteTitle = "Stage Complete";
    [SerializeField] private float doubloonsPerMinuteSurvived = 10f;
    [SerializeField] private int minimumTimeBonus = 0;
    [SerializeField] private int maximumTimeBonus = 9999;
    [SerializeField] private bool roundBonusToWholeNumber = true;
    [SerializeField] private bool showDebugLogs = false;

    [Header("Runtime References")]
    [SerializeField] private ShipHealth playerShipHealth;
    [SerializeField] private RunTimerDirector runTimerDirector;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerLevelSystem playerLevelSystem;
    [SerializeField] private PlayerProgression playerProgression;

    private bool summaryShown;
    private bool timeBonusAwarded;
    private bool stageCompleteBonusAwarded;
    private RunSummaryType currentSummaryType = RunSummaryType.Death;

    private void Awake()
    {
        ResolveReferences();
        EnsureSummaryUiExists();
        HideSummary();
        WireButtonListeners();
    }

    private void OnEnable()
    {
        SubscribeToPlayerDeath();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayerDeath();
        UnwireButtonListeners();
    }

    public bool TryShowDeathSummary(ShipHealth deadShip)
    {
        if (summaryShown)
        {
            return true;
        }

        if (deadShip != null && playerShipHealth == null)
        {
            playerShipHealth = deadShip;
        }

        ShowSummary(RunSummaryType.Death, deadShip);
        return true;
    }

    public bool TryShowStageCompleteSummary(ShipHealth defeatedBoss)
    {
        if (!showSummaryOnStageComplete)
        {
            return false;
        }

        if (summaryShown)
        {
            return true;
        }

        ShowSummary(RunSummaryType.StageComplete, defeatedBoss);
        return true;
    }

    private void HandlePlayerShipDeath(ShipHealth deadShip)
    {
        TryShowDeathSummary(deadShip);
    }

    private void ShowSummary(RunSummaryType summaryType, ShipHealth contextShip)
    {
        summaryShown = true;
        currentSummaryType = summaryType;
        ResolveReferences();
        EnsureSummaryUiExists();

        if (runTimerDirector != null)
        {
            runTimerDirector.StopTimer();
        }

        float elapsedSeconds = GetElapsedRunSeconds();
        int collectedDoubloons = playerInventory != null ? Mathf.Max(0, playerInventory.Doubloons) : 0;
        int timeBonus = CalculateTimeBonus(elapsedSeconds, summaryType);
        int awardedTimeBonus = AwardTimeBonus(timeBonus, summaryType);
        int awardedStageBonus = AwardStageCompleteBonus(summaryType);
        int totalBonusAwarded = awardedTimeBonus + awardedStageBonus;
        int totalAwarded = collectedDoubloons + totalBonusAwarded;

        if (titleText != null)
        {
            titleText.text = GetTitle(summaryType);
        }

        if (summaryText != null)
        {
            summaryText.text = BuildSummaryText(elapsedSeconds, collectedDoubloons, awardedTimeBonus, awardedStageBonus, totalAwarded, summaryType);
        }

        if (bonusText != null)
        {
            bonusText.text = BuildBonusText(awardedTimeBonus, awardedStageBonus, totalBonusAwarded);
        }

        if (summaryRoot != null)
        {
            summaryRoot.SetActive(true);
        }

        if (showDebugLogs)
        {
            string contextShipName = contextShip != null ? contextShip.name : "Unavailable";
            Debug.Log($"RunSummaryController: Showing {summaryType} summary for {contextShipName}. Time bonus={awardedTimeBonus}, stage bonus={awardedStageBonus}, total awarded={totalAwarded}.", this);
        }

        if (pauseTimeOnSummary)
        {
            Time.timeScale = 0f;
        }
    }

    private string BuildSummaryText(float elapsedSeconds, int collectedDoubloons, int awardedTimeBonus, int awardedStageBonus, int totalAwarded, RunSummaryType summaryType)
    {
        StringBuilder builder = new();
        builder.AppendLine($"Stage: {SceneManager.GetActiveScene().name}");
        builder.AppendLine($"Result: {GetTitle(summaryType)}");
        builder.AppendLine($"Time Survived: {FormatSeconds(elapsedSeconds)}");

        if (playerLevelSystem != null)
        {
            builder.AppendLine($"Level Reached: {playerLevelSystem.CurrentLevel}");
            builder.AppendLine(playerLevelSystem.XPRequiredForNextLevel > 0
                ? $"XP: {playerLevelSystem.CurrentXP}/{playerLevelSystem.XPRequiredForNextLevel}"
                : $"XP: {playerLevelSystem.CurrentXP}");
        }
        else
        {
            builder.AppendLine("Level Reached: Unavailable");
            builder.AppendLine("XP: Unavailable");
        }

        builder.AppendLine(playerInventory != null ? $"Wood Collected: {playerInventory.Wood}" : "Wood Collected: Unavailable");
        builder.AppendLine(playerInventory != null ? $"Doubloons Collected: {collectedDoubloons}" : "Doubloons Collected: Unavailable");
        builder.AppendLine($"Time Bonus: +{awardedTimeBonus} Doubloons");
        if (summaryType == RunSummaryType.StageComplete || awardedStageBonus > 0)
        {
            builder.AppendLine($"Stage Clear Bonus: +{awardedStageBonus} Doubloons");
        }
        builder.AppendLine($"Total Awarded: {totalAwarded} Doubloons");

        return builder.ToString();
    }

    private int AwardTimeBonus(int timeBonus, RunSummaryType summaryType)
    {
        if (!ShouldAwardTimeBonus(summaryType) || timeBonus <= 0 || timeBonusAwarded)
        {
            return 0;
        }

        if (playerProgression == null && PlayerProgression.HasActiveSaveSlot)
        {
            playerProgression = PlayerProgression.Instance;
        }

        if (playerProgression == null)
        {
            Debug.LogWarning("RunSummaryController: No active save slot is available, so the time bonus was not awarded.", this);
            return 0;
        }

        timeBonusAwarded = true;
        playerProgression.AddDoubloons(timeBonus);
        PlayerProgression.SaveActiveSlot();
        return timeBonus;
    }

    private int CalculateTimeBonus(float elapsedSeconds, RunSummaryType summaryType)
    {
        if (!ShouldAwardTimeBonus(summaryType))
        {
            return 0;
        }

        float minutesSurvived = Mathf.Max(0f, elapsedSeconds) / 60f;
        float rawBonus = minutesSurvived * Mathf.Max(0f, doubloonsPerMinuteSurvived);
        int calculatedBonus = roundBonusToWholeNumber ? Mathf.FloorToInt(rawBonus) : Mathf.RoundToInt(rawBonus);
        int minBonus = Mathf.Max(0, minimumTimeBonus);
        int maxBonus = Mathf.Max(minBonus, maximumTimeBonus);
        return Mathf.Clamp(calculatedBonus, minBonus, maxBonus);
    }


    private int AwardStageCompleteBonus(RunSummaryType summaryType)
    {
        int clampedBonus = Mathf.Max(0, stageCompleteBonusDoubloons);
        if (summaryType != RunSummaryType.StageComplete || clampedBonus <= 0 || stageCompleteBonusAwarded)
        {
            return 0;
        }

        if (playerProgression == null && PlayerProgression.HasActiveSaveSlot)
        {
            playerProgression = PlayerProgression.Instance;
        }

        if (playerProgression == null)
        {
            Debug.LogWarning("RunSummaryController: No active save slot is available, so the stage clear bonus was not awarded.", this);
            return 0;
        }

        stageCompleteBonusAwarded = true;
        playerProgression.AddDoubloons(clampedBonus);
        PlayerProgression.SaveActiveSlot();
        return clampedBonus;
    }

    private bool ShouldAwardTimeBonus(RunSummaryType summaryType)
    {
        return summaryType == RunSummaryType.StageComplete ? awardTimeBonusOnStageComplete : awardTimeBonusOnDeath;
    }

    private string GetTitle(RunSummaryType summaryType)
    {
        string configuredTitle = summaryType == RunSummaryType.StageComplete ? stageCompleteTitle : deathTitle;
        if (!string.IsNullOrWhiteSpace(configuredTitle))
        {
            return configuredTitle;
        }

        return summaryType == RunSummaryType.StageComplete ? "Stage Complete" : "Run Over";
    }

    private static string BuildBonusText(int awardedTimeBonus, int awardedStageBonus, int totalBonusAwarded)
    {
        if (awardedStageBonus > 0)
        {
            return $"Time Bonus: +{awardedTimeBonus} | Stage Clear Bonus: +{awardedStageBonus} | Total Bonus: +{totalBonusAwarded} Doubloons";
        }

        return $"Time Bonus: +{awardedTimeBonus} Doubloons";
    }

    private float GetElapsedRunSeconds()
    {
        if (runTimerDirector != null)
        {
            return Mathf.Max(0f, runTimerDirector.ElapsedTime);
        }

        return Mathf.Max(0f, Time.timeSinceLevelLoad);
    }

    private void ResolveReferences()
    {
        playerShipHealth ??= FindPlayerShipHealth();
        runTimerDirector ??= FindFirstObjectByType<RunTimerDirector>();
        playerInventory ??= FindFirstObjectByType<PlayerInventory>();
        playerLevelSystem ??= FindFirstObjectByType<PlayerLevelSystem>();
        playerProgression ??= PlayerProgression.HasActiveSaveSlot ? PlayerProgression.Instance : null;
    }

    private ShipHealth FindPlayerShipHealth()
    {
        GameObject taggedPlayerShip = GameObject.FindGameObjectWithTag("PlayerShip");
        if (taggedPlayerShip != null && taggedPlayerShip.TryGetComponent(out ShipHealth taggedHealth))
        {
            return taggedHealth;
        }

        PlayerShipDefeatHandler defeatHandler = FindFirstObjectByType<PlayerShipDefeatHandler>();
        if (defeatHandler != null && defeatHandler.TryGetComponent(out ShipHealth handlerHealth))
        {
            return handlerHealth;
        }

        return FindFirstObjectByType<ShipHealth>();
    }

    private void SubscribeToPlayerDeath()
    {
        ResolveReferences();
        if (playerShipHealth != null)
        {
            playerShipHealth.OnDeath -= HandlePlayerShipDeath;
            playerShipHealth.OnDeath += HandlePlayerShipDeath;
        }
    }

    private void UnsubscribeFromPlayerDeath()
    {
        if (playerShipHealth != null)
        {
            playerShipHealth.OnDeath -= HandlePlayerShipDeath;
        }
    }

    private void WireButtonListeners()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RetryRun);
            retryButton.onClick.AddListener(RetryRun);
        }

        if (mapButton != null)
        {
            mapButton.onClick.RemoveListener(LoadMap);
            mapButton.onClick.AddListener(LoadMap);
        }

        if (shipShopButton != null)
        {
            shipShopButton.onClick.RemoveListener(LoadShipShop);
            shipShopButton.onClick.AddListener(LoadShipShop);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }
    }

    private void UnwireButtonListeners()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(RetryRun);
        }

        if (mapButton != null)
        {
            mapButton.onClick.RemoveListener(LoadMap);
        }

        if (shipShopButton != null)
        {
            shipShopButton.onClick.RemoveListener(LoadShipShop);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
        }
    }

    public void RetryRun()
    {
        string targetScene = string.IsNullOrWhiteSpace(retrySceneNameOverride)
            ? SceneManager.GetActiveScene().name
            : retrySceneNameOverride;
        LoadScene(targetScene);
    }

    public void LoadMap() => LoadScene(mapSceneName);
    public void LoadShipShop() => LoadScene(shipShopSceneName);
    public void LoadMainMenu() => LoadScene(mainMenuSceneName);

    private void LoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("RunSummaryController: Cannot load an empty scene name.", this);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private void HideSummary()
    {
        if (summaryRoot != null)
        {
            summaryRoot.SetActive(false);
        }
    }

    private void EnsureSummaryUiExists()
    {
        if (summaryRoot != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new("RunSummaryCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        GameObject panel = new("RunSummaryPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);
        summaryRoot = panel;

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(650f, 550f);
        panelRect.anchoredPosition = Vector2.zero;

        Image background = panel.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.85f);

        titleText = CreateText(panel.transform, "TitleText", GetTitle(RunSummaryType.Death), 42, TextAlignmentOptions.Center, new Vector2(0f, 205f), new Vector2(600f, 70f));
        summaryText = CreateText(panel.transform, "SummaryText", string.Empty, 26, TextAlignmentOptions.TopLeft, new Vector2(0f, 35f), new Vector2(560f, 290f));
        bonusText = CreateText(panel.transform, "BonusText", string.Empty, 30, TextAlignmentOptions.Center, new Vector2(0f, -145f), new Vector2(560f, 60f));

        retryButton = CreateButton(panel.transform, "RetryButton", "Retry", new Vector2(-240f, -220f));
        mapButton = CreateButton(panel.transform, "MapButton", "Map", new Vector2(-80f, -220f));
        shipShopButton = CreateButton(panel.transform, "ShipShopButton", "Ship Shop", new Vector2(80f, -220f));
        mainMenuButton = CreateButton(panel.transform, "MainMenuButton", "Main Menu", new Vector2(240f, -220f));
        WireButtonListeners();
    }

    private static TMP_Text CreateText(Transform parent, string objectName, string text, int fontSize, TextAlignmentOptions alignment, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = size;

        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = alignment;
        tmpText.color = Color.white;
        return tmpText;
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(140f, 50f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.85f, 0.68f, 0.32f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        TMP_Text labelText = CreateText(buttonObject.transform, "Text", label, 22, TextAlignmentOptions.Center, Vector2.zero, new Vector2(130f, 44f));
        labelText.color = Color.black;
        return button;
    }

    private static string FormatSeconds(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainingSeconds:00}";
    }
}
