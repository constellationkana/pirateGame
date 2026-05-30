using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseProgressionMenu : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.U;

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject upgradesPanel;
    [SerializeField] private GameObject controlsPanel;

    [Header("Buttons")]
    [SerializeField] private Button upgradesButton;
    [SerializeField] private bool hideUpgradesButtonOutsideRuns = true;

    [Header("Text")]
    [SerializeField] private TMP_Text activeSaveNameText;
    [SerializeField] private TMP_Text progressionSummaryText;
    [SerializeField] private TMP_Text upgradesSummaryText;
    [SerializeField] private TMP_Text controlsSummaryText;
    [SerializeField] private TMP_Text messageText;

    [Header("Scene Loading")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string shipShopSceneName = "ShipShop";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private const string MainSeaSceneName = "MainSea";

    private bool isOpen;

    private void Start()
    {
        CloseMenuPanels();
        SetControlsText();
        UpdateUpgradeButtonAvailability();
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

        isOpen = true;
        Time.timeScale = 0f;

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        ShowMainMenuPanel();
        UpdateUpgradeButtonAvailability();
        RefreshActiveSaveText();
    }

    public void ResumeGame()
    {
        isOpen = false;
        Time.timeScale = 1f;
        CloseMenuPanels();
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
    }

    public void LoadMap()
    {
        LoadScene(mapSceneName, nameof(mapSceneName));
    }

    public void LoadShipShop()
    {
        LoadScene(shipShopSceneName, nameof(shipShopSceneName));
    }

    public void LoadMainMenu()
    {
        LoadScene(mainMenuSceneName, nameof(mainMenuSceneName));
    }

    public void SaveGame()
    {
        PlayerProgression.SaveActiveSlot();
        PlayerPrefs.Save();
        SetMessage("Game saved.");
        Debug.Log($"PauseProgressionMenu: Saved active slot '{PlayerProgression.GetActiveSaveName()}'.", this);
    }

    public void RefreshProgressionText() => RefreshUpgradesText();

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

    private void CloseMenuPanels()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }

        SetPanelActive(mainMenuPanel, false);
        SetPanelActive(upgradesPanel, false);
        SetPanelActive(controlsPanel, false);
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

    private void RefreshActiveSaveText()
    {
        if (activeSaveNameText != null)
        {
            activeSaveNameText.text = $"Active Save: {PlayerProgression.GetActiveSaveName()}";
        }
    }

    private void SetControlsText()
    {
        if (controlsSummaryText == null)
        {
            return;
        }

        controlsSummaryText.text =
            "Movement:\n" +
            "WASD = Move PlayerShip while boarded\n\n" +
            "Combat:\n" +
            "Arrow Keys = Fire cannon in cardinal directions\n" +
            "Space = Fire toward mouse\n\n" +
            "Interaction:\n" +
            "E = Board / interact\n\n" +
            "Abilities:\n" +
            "Shift = Dash if unlocked\n\n" +
            "Menus:\n" +
            "U = Pause / progression menu";
    }

    private void LoadScene(string sceneName, string fieldName)
    {
        Time.timeScale = 1f;
        isOpen = false;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"PauseProgressionMenu: {fieldName} is empty.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
        {
            messageText.text = message;
        }
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
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
