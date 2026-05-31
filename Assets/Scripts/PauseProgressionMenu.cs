using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseProgressionMenu : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.U;
    [SerializeField] private bool pauseTime = true;

    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button shipShopButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button upgradesButton;
    [SerializeField] private bool hideUpgradesButtonOutsideRuns = true;

    [Header("Text")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text activeSaveNameText;
    [SerializeField] private TMP_Text runStatsText;
    [SerializeField] private TMP_Text progressionSummaryText;
    [SerializeField] private TMP_Text upgradesSummaryText;
    [SerializeField] private TMP_Text controlsText;
    [SerializeField] private TMP_Text controlsSummaryText;
    [SerializeField] private TMP_Text messageText;

    [Header("Scene Loading")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string shipShopSceneName = "ShipShop";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Upgrade Choice Safety")]
    [SerializeField] private bool closeUpgradeChoicePanelOnOpen = false;

    [Header("ShipShop-Style Polish")]
    [SerializeField] private bool applyShipShopStyleOnStart = true;
    [SerializeField] private Color panelBackgroundColor = new Color(0f, 0f, 0f, 0.72f);
    [SerializeField] private Color buttonColor = new Color(0.85f, 0.67f, 0.32f, 1f);
    [SerializeField] private Color buttonTextColor = Color.black;
    [SerializeField] private Color bodyTextColor = Color.white;

    [Header("Debug")]
    [SerializeField] private bool logPauseDebug = false;

    private const string MainSeaSceneName = "MainSea";

    private bool isOpen;

    private GameObject MenuRoot => menuRoot != null ? menuRoot : menuPanel;
    private TMP_Text RunStatsTargetText => runStatsText != null ? runStatsText : progressionSummaryText;
    private TMP_Text ControlsTargetText => controlsText != null ? controlsText : controlsSummaryText;

    private void Awake()
    {
        WireButtonListeners();

        if (applyShipShopStyleOnStart)
        {
            ApplyShipShopStyle();
        }
    }

    private void OnEnable()
    {
        WireButtonListeners();
    }

    private void Start()
    {
        CloseMenuPanels();
        SetTitleText();
        SetControlsText();
        RefreshMenuText();
        UpdateUpgradeButtonAvailability();
    }

    private void OnDisable()
    {
        UnwireButtonListeners();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (isOpen)
        {
            ResumeGame();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (isOpen)
        {
            return;
        }

        if (IsAnotherPauseBlockingMenuOpen())
        {
            if (logPauseDebug)
            {
                Debug.LogWarning("PauseProgressionMenu: Another pause-style UI appears to be active, so the pause menu was not opened.", this);
            }

            return;
        }

        isOpen = true;
        SetMenuVisible(true);
        ShowMainMenuPanel();
        UpdateUpgradeButtonAvailability();
        RefreshMenuText();

        if (pauseTime)
        {
            Time.timeScale = 0f;
        }
    }

    public void ResumeGame()
    {
        isOpen = false;
        CloseMenuPanels();
        RestoreGameplayTimeScale();
    }

    public void OpenUpgradesPanel()
    {
        if (!IsCurrentRunUpgradePanelAvailable())
        {
            SetMessage("Current-run upgrades are only available during an active run.");
            return;
        }

        ShowSubpanel(upgradesPanel);
        RefreshUpgradesText();
    }

    public void OpenControlsPanel()
    {
        ShowSubpanel(controlsPanel);
        SetControlsText();
    }

    public void BackToMainMenuPanel()
    {
        ShowMainMenuPanel();
        UpdateUpgradeButtonAvailability();
        RefreshMenuText();
    }

    public void LoadMap()
    {
        SaveActiveSlotIfAvailable();
        LoadScene(mapSceneName, nameof(mapSceneName));
    }

    public void LoadShipShop()
    {
        SaveActiveSlotIfAvailable();
        LoadScene(shipShopSceneName, nameof(shipShopSceneName));
    }

    public void LoadMainMenu()
    {
        SaveActiveSlotIfAvailable();
        LoadScene(mainMenuSceneName, nameof(mainMenuSceneName));
    }

    public void SaveGame()
    {
        PlayerProgression.SaveActiveSlot();
        PlayerPrefs.Save();
        SetMessage("Game saved.");
        Debug.Log($"PauseProgressionMenu: Saved active slot '{PlayerProgression.GetActiveSaveName()}'.", this);
    }

    public void RefreshProgressionText()
    {
        RefreshRunStatsText();
        RefreshUpgradesText();
    }

    public void RefreshUpgradesText()
    {
        RefreshActiveSaveText();

        TMP_Text targetText = upgradesSummaryText != null ? upgradesSummaryText : progressionSummaryText;
        if (targetText == null)
        {
            return;
        }

        if (!IsCurrentRunUpgradePanelAvailable())
        {
            targetText.text = "Current-run upgrades are only available in MainSea during an active run.";
            return;
        }

        UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (upgradeManager == null || upgradeManager.CurrentRunUpgradeLevels.Count == 0)
        {
            targetText.text = "Current-run upgrades:\nNone yet.";
            return;
        }

        StringBuilder builder = new();
        builder.AppendLine("Current-run upgrades:");

        foreach (KeyValuePair<string, int> upgradeLevel in upgradeManager.CurrentRunUpgradeLevels)
        {
            string displayName = NormalizeUpgradeDisplayName(upgradeManager.GetCurrentRunUpgradeDisplayName(upgradeLevel.Key));
            builder.AppendLine($"{displayName}: Lv {upgradeLevel.Value}");
        }

        targetText.text = builder.ToString();
    }

    private void WireButtonListeners()
    {
        EnsureButtonListener(resumeButton, ResumeGame, nameof(ResumeGame));
        EnsureButtonListener(mapButton, LoadMap, nameof(LoadMap));
        EnsureButtonListener(shipShopButton, LoadShipShop, nameof(LoadShipShop));
        EnsureButtonListener(mainMenuButton, LoadMainMenu, nameof(LoadMainMenu));
    }

    private void UnwireButtonListeners()
    {
        RemoveButtonListener(resumeButton, ResumeGame);
        RemoveButtonListener(mapButton, LoadMap);
        RemoveButtonListener(shipShopButton, LoadShipShop);
        RemoveButtonListener(mainMenuButton, LoadMainMenu);
    }

    private void EnsureButtonListener(Button button, UnityAction action, string methodName)
    {
        if (button == null || action == null)
        {
            if (logPauseDebug && button == null)
            {
                Debug.Log($"PauseProgressionMenu: Optional button for {methodName} is not assigned.", this);
            }

            return;
        }

        button.onClick.RemoveListener(action);

        if (!HasPersistentListener(button, methodName))
        {
            button.onClick.AddListener(action);
        }
    }

    private void RemoveButtonListener(Button button, UnityAction action)
    {
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
    }

    private bool HasPersistentListener(Button button, string methodName)
    {
        if (button == null || string.IsNullOrWhiteSpace(methodName))
        {
            return false;
        }

        int listenerCount = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < listenerCount; i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }

    private void CloseMenuPanels()
    {
        SetMenuVisible(false);
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(upgradesPanel, false);
        SetPanelActive(controlsPanel, false);
    }

    private void SetMenuVisible(bool visible)
    {
        GameObject root = MenuRoot;
        if (root != null)
        {
            root.SetActive(visible);
        }
    }

    private void ShowMainMenuPanel()
    {
        SetPanelActive(mainMenuPanel, true);
        SetPanelActive(upgradesPanel, false);
        SetPanelActive(controlsPanel, false);
    }

    private void ShowSubpanel(GameObject panel)
    {
        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(upgradesPanel, false);
        SetPanelActive(controlsPanel, false);
        SetPanelActive(panel, true);
    }

    private void UpdateUpgradeButtonAvailability()
    {
        if (upgradesButton == null)
        {
            return;
        }

        bool available = IsCurrentRunUpgradePanelAvailable();
        upgradesButton.interactable = available;

        if (hideUpgradesButtonOutsideRuns)
        {
            upgradesButton.gameObject.SetActive(available);
        }
    }

    private bool IsCurrentRunUpgradePanelAvailable()
    {
        return SceneManager.GetActiveScene().name == MainSeaSceneName && FindFirstObjectByType<UpgradeManager>() != null;
    }

    private bool IsAnotherPauseBlockingMenuOpen()
    {
        if (!pauseTime || Time.timeScale > 0f)
        {
            return false;
        }

        if (closeUpgradeChoicePanelOnOpen && logPauseDebug)
        {
            Debug.LogWarning("PauseProgressionMenu: closeUpgradeChoicePanelOnOpen is enabled, but UpgradeChoiceUI has no public close API. Leaving the existing paused UI open.", this);
        }

        return true;
    }

    private void RefreshMenuText()
    {
        SetTitleText();
        RefreshActiveSaveText();
        RefreshRunStatsText();
        SetControlsText();
        SetMessage(string.Empty);
    }

    private void SetTitleText()
    {
        if (titleText != null)
        {
            titleText.text = "Captain's Log";
        }
    }

    private void RefreshActiveSaveText()
    {
        if (activeSaveNameText == null)
        {
            return;
        }

        activeSaveNameText.text = PlayerProgression.HasActiveSaveSlot
            ? $"Save: {PlayerProgression.GetActiveSaveName()}"
            : "No Save Active";
    }

    private void RefreshRunStatsText()
    {
        TMP_Text targetText = RunStatsTargetText;
        if (targetText == null)
        {
            return;
        }

        PlayerLevelSystem levelSystem = FindFirstObjectByType<PlayerLevelSystem>();
        PlayerInventory inventory = FindFirstObjectByType<PlayerInventory>();
        PlayerProgression progression = PlayerProgression.HasActiveSaveSlot ? PlayerProgression.Instance : null;

        int level = levelSystem != null ? levelSystem.CurrentLevel : 1;
        int currentXP = levelSystem != null ? levelSystem.CurrentXP : 0;
        int nextXP = levelSystem != null ? levelSystem.XPRequiredForNextLevel : 0;
        int wood = inventory != null ? inventory.Wood : 0;
        int doubloons = inventory != null ? inventory.Doubloons : progression != null ? progression.GetDoubloons() : 0;
        bool dashUnlocked = progression != null && progression.IsDashUnlocked();

        StringBuilder builder = new();
        builder.AppendLine($"Level: {level}");
        builder.AppendLine(nextXP > 0 ? $"XP: {currentXP}/{nextXP}" : $"XP: {currentXP}");
        builder.AppendLine($"Wood: {wood}");
        builder.AppendLine($"Doubloons: {doubloons}");
        builder.AppendLine($"Stage: {SceneManager.GetActiveScene().name}");
        builder.AppendLine($"Run Time: {FormatSeconds(Time.timeSinceLevelLoad)}");
        builder.AppendLine($"Dash: {(dashUnlocked ? "Unlocked" : "Locked")}");

        targetText.text = builder.ToString();
    }

    private void SetControlsText()
    {
        TMP_Text targetText = ControlsTargetText;
        if (targetText == null)
        {
            return;
        }

        targetText.text =
            "Controls:\n" +
            "WASD = move while boarded\n" +
            "Arrow keys = fire cannons\n" +
            "Space = fire toward mouse\n" +
            "E = board / interact\n" +
            "U = pause\n" +
            "Shift = dash if unlocked";
    }

    private void LoadScene(string sceneName, string fieldName)
    {
        RestoreGameplayTimeScale();
        isOpen = false;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"PauseProgressionMenu: {fieldName} is empty.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void RestoreGameplayTimeScale()
    {
        if (pauseTime)
        {
            Time.timeScale = 1f;
        }
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    private void ApplyShipShopStyle()
    {
        Image panelImage = MenuRoot != null ? MenuRoot.GetComponent<Image>() : null;
        if (panelImage != null)
        {
            panelImage.color = panelBackgroundColor;
        }

        ApplyButtonStyle(resumeButton);
        ApplyButtonStyle(mapButton);
        ApplyButtonStyle(shipShopButton);
        ApplyButtonStyle(mainMenuButton);

        ApplyTextColor(titleText);
        ApplyTextColor(activeSaveNameText);
        ApplyTextColor(runStatsText);
        ApplyTextColor(progressionSummaryText);
        ApplyTextColor(upgradesSummaryText);
        ApplyTextColor(controlsText);
        ApplyTextColor(controlsSummaryText);
        ApplyTextColor(messageText);
    }

    private void ApplyButtonStyle(Button button)
    {
        if (button == null)
        {
            return;
        }

        Image image = button.GetComponent<Image>();
        if (image != null)
        {
            image.color = buttonColor;
        }

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>(true);
        if (buttonText != null)
        {
            buttonText.color = buttonTextColor;
        }
    }

    private void ApplyTextColor(TMP_Text text)
    {
        if (text != null)
        {
            text.color = bodyTextColor;
        }
    }

    private static void SaveActiveSlotIfAvailable()
    {
        if (PlayerProgression.HasActiveSaveSlot)
        {
            PlayerProgression.SaveActiveSlot();
            PlayerPrefs.Save();
        }
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }

    private static string FormatSeconds(float seconds)
    {
        int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
        int minutes = totalSeconds / 60;
        int remainderSeconds = totalSeconds % 60;
        return $"{minutes:00}:{remainderSeconds:00}";
    }

    private static string NormalizeUpgradeDisplayName(string displayName)
    {
        return displayName switch
        {
            "Health Upgrade" => "Health",
            "Ship Speed" => "Speed",
            "Cannonball Damage" => "Cannon Damage",
            _ => displayName
        };
    }
}
