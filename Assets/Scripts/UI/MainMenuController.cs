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
    [SerializeField] private Transform saveSlotListParent;
    [SerializeField] private SaveSlotEntryUI saveSlotEntryPrefab;
    [SerializeField] private TMP_Text saveSelectStatusText;
    [SerializeField] private TMP_InputField renameInputField;

    private readonly List<SaveSlotEntryUI> spawnedSaveEntries = new();
    private int selectedRenameSlotId = -1;

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

    public void OnPlayPressed()
    {
        PlayerProgression.CreateNewSaveSlot();
        LoadMap();
    }

    public void OnNewGamePressed() => OnPlayPressed();

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

    public void OpenSaveSelectPanel()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
        saveSelectPanel?.SetActive(true);
        RefreshSaveSelectList();
    }

    public void CloseSaveSelectPanel()
    {
        saveSelectPanel?.SetActive(false);
        ShowMainButtons();
    }

    public void LoadSaveSlot(int slotId)
    {
        if (PlayerProgression.SetActiveSaveSlot(slotId))
        {
            LoadMap();
        }
    }

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

    public void RenameSaveSlot(int slotId)
    {
        selectedRenameSlotId = slotId;
        RenameSelectedSaveSlot();
    }

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

    public void OnSettingsPressed()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
        creditsPanel?.SetActive(false);
        saveSelectPanel?.SetActive(false);
    }

    public void OnCreditsPressed()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(true);
        saveSelectPanel?.SetActive(false);
    }

    public void ShowMainButtons()
    {
        mainButtonsPanel?.SetActive(true);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
        saveSelectPanel?.SetActive(false);
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        Debug.Log("Quit requested from Main Menu (Editor): would quit application in a build.");
#else
        Application.Quit();
#endif
    }

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
