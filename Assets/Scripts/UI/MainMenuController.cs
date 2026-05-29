using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Attach to a dedicated MainMenu scene object (for example: Canvas/MainMenuController).
/// Handles top-level menu navigation and panel visibility.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string shipShopSceneName = "ShipShop";

    [Header("Panels")]
    [SerializeField] private GameObject mainButtonsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Continue State")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TMP_Text continueStatusText;
    [SerializeField] private string noSaveFoundMessage = "No Save Found";

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
        PlayerProgression.ResetAllProgression();
        PlayerProgression.MarkSaveExists();
        LoadShipShop();
    }

    public void OnNewGamePressed() => OnPlayPressed();

    public void OnContinuePressed()
    {
        if (PlayerProgression.HasSaveFile())
        {
            LoadShipShop();
            return;
        }

        if (continueStatusText != null)
        {
            continueStatusText.text = noSaveFoundMessage;
            continueStatusText.gameObject.SetActive(true);
        }

        RefreshContinueState();
    }

    public void OnSettingsPressed()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(true);
        creditsPanel?.SetActive(false);
    }

    public void OnCreditsPressed()
    {
        mainButtonsPanel?.SetActive(false);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(true);
    }

    public void ShowMainButtons()
    {
        mainButtonsPanel?.SetActive(true);
        settingsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
    }

    public void OnQuitPressed()
    {
#if UNITY_EDITOR
        Debug.Log("Quit requested from Main Menu (Editor): would quit application in a build.");
#else
        Application.Quit();
#endif
    }

    private void RefreshContinueState()
    {
        if (continueButton != null)
        {
            continueButton.interactable = PlayerProgression.HasSaveFile();
        }
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
}
