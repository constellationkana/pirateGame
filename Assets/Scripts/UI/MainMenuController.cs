using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Attach to a dedicated MainMenu scene object (for example: Canvas/MainMenuController).
/// Handles top-level menu navigation and panel visibility.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("Scene Loading")]
    [SerializeField] private string gameplaySceneName = "MainSea";

    [Header("Panels")]
    [SerializeField] private GameObject mainButtonsPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [Header("Continue State")]
    [SerializeField] private bool hasSaveFile;
    [SerializeField] private TMP_Text continueStatusText;
    [SerializeField] private string noSaveFoundMessage = "No Save Found";

    private void Start()
    {
        ShowMainButtons();

        if (continueStatusText != null)
        {
            continueStatusText.gameObject.SetActive(false);
        }
    }

    public void OnPlayPressed()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OnContinuePressed()
    {
        if (hasSaveFile)
        {
            // Placeholder for future save system flow.
            SceneManager.LoadScene(gameplaySceneName);
            return;
        }

        if (continueStatusText != null)
        {
            continueStatusText.text = noSaveFoundMessage;
            continueStatusText.gameObject.SetActive(true);
        }
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
}
