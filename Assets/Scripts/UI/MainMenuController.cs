using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to a dedicated MainMenu scene object (for example: Canvas/MainMenuController).
/// Handles top-level menu navigation, save-slot selection, and panel visibility.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string shipShopSceneName = "ShipShop";

    [Header("Panels")]
    [SerializeField] private GameObject mainButtonsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject saveSelectPanel;

    [Header("Continue State")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueStatusText;
    [SerializeField] private string noSaveFoundMessage = "No Save Found";

    [Header("Save Select")]
    [SerializeField] private ScrollRect saveSlotScrollRect;
    [SerializeField] private Transform saveSlotListParent;
    [SerializeField] private SaveSlotEntryUI saveSlotEntryPrefab;
    [SerializeField] private TMP_Text saveSelectStatusText;
    [SerializeField] private TMP_InputField renameInputField;

    private readonly List<SaveSlotEntryUI> spawnedSaveEntries = new();
    private int selectedRenameSlotId = -1;
    private Coroutine resetSaveSlotScrollCoroutine;

    private void Start()
    {
        ShowMainButtons();
        RefreshContinueState();

        if (continueStatusText != null)
        {
            continueStatusText.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        RefreshContinueState();
    }

    /// <summary>
    /// Creates a new save slot and loads the map scene.
    /// </summary>
    public void OnPlayPressed()
    {
        PlayerProgression.CreateNewSaveSlot();
        LoadMap();
    }

    /// <summary>
    /// Compatibility entry point for new-game buttons; starts a new save.
    /// </summary>
    public void OnNewGamePressed() => OnPlayPressed();

    /// <summary>
    /// Opens save selection when saves exist, or displays the no-save message.
    /// </summary>
    public void OnContinuePressed()
    {
        if (!PlayerProgression.HasSaveFile())
        {
            ShowNoSaveFoundMessage();
            RefreshContinueState();
            return;
        }

        OpenSaveSelectPanel();
    }

    /// <summary>
    /// Shows the save-select panel and refreshes its list.
    /// </summary>
    public void OpenSaveSelectPanel()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
        saveSelectPanel?.SetActive(true);
        RefreshSaveSelectList();
    }

    /// <summary>
    /// Hides the save-select panel and returns to the main buttons.
    /// </summary>
    public void CloseSaveSelectPanel()
    {
        saveSelectPanel?.SetActive(false);
        ShowMainButtons();
    }

    /// <summary>
    /// Loads a selected save slot and opens the map when successful.
    /// </summary>
    /// <param name="slotId">Save-slot identifier.</param>
    public void LoadSaveSlot(int slotId)
    {
        if (PlayerProgression.SetActiveSaveSlot(slotId))
        {
            LoadMap();
        }
    }

    /// <summary>
    /// Selects a save slot for renaming and focuses the rename input field.
    /// </summary>
    /// <param name="slotId">Save-slot identifier.</param>
    public void BeginRenameSaveSlot(int slotId)
    {
        selectedRenameSlotId = slotId;
        if (renameInputField != null)
        {
            renameInputField.text = string.Empty;
            renameInputField.Select();
        }

        if (saveSelectStatusText != null)
        {
            saveSelectStatusText.text = "Enter a new save name, then press Rename Selected.";
        }
    }

    /// <summary>
    /// Renames the currently selected save slot using the rename input field.
    /// </summary>
    public void RenameSelectedSaveSlot()
    {
        if (selectedRenameSlotId < 0)
        {
            if (saveSelectStatusText != null)
            {
                saveSelectStatusText.text = "Select a save to rename first.";
            }
            return;
        }

        string newName = renameInputField != null ? renameInputField.text : string.Empty;
        if (PlayerProgression.RenameSaveSlot(selectedRenameSlotId, newName))
        {
            selectedRenameSlotId = -1;
            if (renameInputField != null)
            {
                renameInputField.text = string.Empty;
            }
            RefreshSaveSelectList();
        }
    }

    /// <summary>
    /// Renames an existing save slot.
    /// </summary>
    /// <param name="slotId">Save-slot identifier.</param>
    public void RenameSaveSlot(int slotId)
    {
        selectedRenameSlotId = slotId;
        RenameSelectedSaveSlot();
    }

    /// <summary>
    /// Deletes an existing save slot and selects another slot when needed.
    /// </summary>
    /// <param name="slotId">Save-slot identifier.</param>
    public void DeleteSaveSlot(int slotId)
    {
        if (PlayerProgression.DeleteSaveSlot(slotId))
        {
            if (selectedRenameSlotId == slotId)
            {
                selectedRenameSlotId = -1;
            }
            RefreshContinueState();
            RefreshSaveSelectList();
        }
    }

    /// <summary>
    /// Shows the settings panel and hides other menu panels.
    /// </summary>
    public void OnSettingsPressed()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
        creditsPanel?.SetActive(false);
        saveSelectPanel?.SetActive(false);
    }

    /// <summary>
    /// Shows the credits panel and hides other menu panels.
    /// </summary>
    public void OnCreditsPressed()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(true);
        saveSelectPanel?.SetActive(false);
    }

    /// <summary>
    /// Shows the main button panel and hides secondary panels.
    /// </summary>
    public void ShowMainButtons()
    {
        mainButtonsPanel?.SetActive(true);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
        saveSelectPanel?.SetActive(false);
    }

    /// <summary>
    /// Quits the application in builds or logs the quit request in the editor.
    /// </summary>
    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        Debug.Log("Quit requested from Main Menu (Editor): would quit application in a build.");
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Rebuilds the save-select UI list from saved progression summaries.
    /// </summary>
    public void RefreshSaveSelectList()
    {
        ClearSpawnedSaveEntries();

        List<PlayerProgression.SaveSlotSummary> saves = PlayerProgression.GetAllSaveSlotSummaries();
        bool hasSaves = saves.Count > 0;

        if (saveSelectStatusText != null)
        {
            saveSelectStatusText.text = hasSaves ? string.Empty : noSaveFoundMessage;
        }

        if (!hasSaves)
        {
            ShowNoSaveFoundMessage();
            return;
        }

        if (saveSlotListParent == null || saveSlotEntryPrefab == null)
        {
            Debug.LogWarning("MainMenuController: Assign saveSlotListParent and saveSlotEntryPrefab to show save files in the Save Select panel.", this);
            return;
        }

        foreach (PlayerProgression.SaveSlotSummary save in saves)
        {
            SaveSlotEntryUI entry = Instantiate(saveSlotEntryPrefab, saveSlotListParent);
            entry.Configure(save, LoadSaveSlot, BeginRenameSaveSlot, DeleteSaveSlot);
            spawnedSaveEntries.Add(entry);
        }

        ResetSaveSlotScrollPosition();
    }

    private void ResetSaveSlotScrollPosition()
    {
        if (resetSaveSlotScrollCoroutine != null)
        {
            StopCoroutine(resetSaveSlotScrollCoroutine);
        }

        RebuildSaveSlotListLayout();
        SetSaveSlotScrollToTop();
        resetSaveSlotScrollCoroutine = StartCoroutine(ResetSaveSlotScrollPositionNextFrame());
    }

    private IEnumerator ResetSaveSlotScrollPositionNextFrame()
    {
        yield return null;

        RebuildSaveSlotListLayout();
        SetSaveSlotScrollToTop();
        resetSaveSlotScrollCoroutine = null;
    }

    private void RebuildSaveSlotListLayout()
    {
        if (saveSlotListParent is RectTransform saveListContent)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(saveListContent);
        }
    }

    private void SetSaveSlotScrollToTop()
    {
        if (saveSlotScrollRect != null)
        {
            saveSlotScrollRect.verticalNormalizedPosition = 1f;
        }
    }

    private void RefreshContinueState()
    {
        if (continueButton != null)
        {
            continueButton.interactable = PlayerProgression.HasSaveFile();
        }
    }

    private void LoadMap()
    {
        if (string.IsNullOrWhiteSpace(mapSceneName))
        {
            Debug.LogWarning("MainMenuController: mapSceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(mapSceneName);
    }

    private void LoadShipShop()
    {
        if (string.IsNullOrWhiteSpace(shipShopSceneName))
        {
            Debug.LogWarning("MainMenuController: shipShopSceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(shipShopSceneName);
    }

    private void ShowNoSaveFoundMessage()
    {
        if (continueStatusText != null)
        {
            continueStatusText.text = noSaveFoundMessage;
            continueStatusText.gameObject.SetActive(true);
        }

        if (saveSelectStatusText != null)
        {
            saveSelectStatusText.text = noSaveFoundMessage;
        }
    }

    private void ClearSpawnedSaveEntries()
    {
        for (int i = 0; i < spawnedSaveEntries.Count; i++)
        {
            if (spawnedSaveEntries[i] != null)
            {
                Destroy(spawnedSaveEntries[i].gameObject);
            }
        }

        spawnedSaveEntries.Clear();
    }
}
