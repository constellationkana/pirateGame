using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles pause-menu behavior and navigation inside the ship shop scene.
/// </summary>
public class ShipShopPauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private TMP_Text activeSaveText;
    [SerializeField] private TMP_Text doubloonText;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.U;
    [SerializeField] private bool pauseTime = true;

    [Header("Scene Loading")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Shop Integration")]
    [SerializeField] private ShipShopController shopController;
    [SerializeField] private bool closeShopMenusOnOpen = true;

    private bool isOpen;

    private void Awake()
    {
        if (shopController == null)
        {
            shopController = FindFirstObjectByType<ShipShopController>();
        }

        if (menuRoot == null)
        {
            CreateFallbackMenu();
        }

        SetMenuVisible(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    /// <summary>
    /// Toggles this pause or shop menu between open and closed states.
    /// </summary>
    public void ToggleMenu()
    {
        if (isOpen)
        {
            Resume();
        }
        else
        {
            OpenMenu();
        }
    }

    /// <summary>
    /// Opens this menu and applies its pause behavior.
    /// </summary>
    public void OpenMenu()
    {
        if (isOpen)
        {
            return;
        }

        if (closeShopMenusOnOpen && shopController != null)
        {
            shopController.CloseAllMenus();
        }

        isOpen = true;
        Refresh();
        SetMenuVisible(true);

        if (pauseTime)
        {
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Closes this menu and resumes gameplay time when appropriate.
    /// </summary>
    public void Resume()
    {
        isOpen = false;
        SetMenuVisible(false);

        if (pauseTime)
        {
            Time.timeScale = 1f;
        }
    }

    /// <summary>
    /// Loads the configured map scene.
    /// </summary>
    public void GoToMap()
    {
        SaveActiveSlotIfAvailable();
        LoadScene(mapSceneName, nameof(mapSceneName));
    }

    /// <summary>
    /// Loads the configured main menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        SaveActiveSlotIfAvailable();
        LoadScene(mainMenuSceneName, nameof(mainMenuSceneName));
    }

    /// <summary>
    /// Refreshes displayed UI values from the current game state.
    /// </summary>
    public void Refresh()
    {
        if (PlayerProgression.HasActiveSaveSlot)
        {
            PlayerProgression progression = PlayerProgression.Instance;
            if (activeSaveText != null)
            {
                activeSaveText.text = $"Save: {PlayerProgression.GetActiveSaveName()}";
            }

            if (doubloonText != null)
            {
                doubloonText.text = $"Doubloons: {progression.GetDoubloons()}";
            }
        }
        else
        {
            if (activeSaveText != null)
            {
                activeSaveText.text = "No Save Active";
            }

            if (doubloonText != null)
            {
                doubloonText.text = "Doubloons: 0";
            }
        }
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(visible);
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

    private void LoadScene(string sceneName, string fieldName)
    {
        if (pauseTime)
        {
            Time.timeScale = 1f;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"ShipShopPauseMenu: {fieldName} is empty.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void CreateFallbackMenu()
    {
        Canvas canvas = CreateCanvas("ShipShop Pause Canvas", 100);
        GameObject panel = CreatePanel(canvas.transform);
        menuRoot = panel;

        CreateLabel(panel.transform, "Ship Shop Menu", 34, new Vector2(0f, 145f));
        activeSaveText = CreateLabel(panel.transform, "Save: -", 22, new Vector2(0f, 92f));
        doubloonText = CreateLabel(panel.transform, "Doubloons: 0", 22, new Vector2(0f, 58f));
        CreateButton(panel.transform, "Resume", new Vector2(0f, 5f), Resume);
        CreateButton(panel.transform, "Go to Map", new Vector2(0f, -55f), GoToMap);
        CreateButton(panel.transform, "Go to Main Menu", new Vector2(0f, -115f), GoToMainMenu);

        EnsureEventSystem();
    }

    private static Canvas CreateCanvas(string name, int sortingOrder)
    {
        GameObject canvasObject = new(name);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new("ShipShopPauseMenuPanel");
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0.72f);
        return panel;
    }

    private static TMP_Text CreateLabel(Transform parent, string text, int fontSize, Vector2 anchoredPosition)
    {
        GameObject labelObject = new(text);
        labelObject.transform.SetParent(parent, false);

        RectTransform rectTransform = labelObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(520f, 48f);
        rectTransform.anchoredPosition = anchoredPosition;

        TMP_Text label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new(label);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(320f, 48f);
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.85f, 0.67f, 0.32f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        TMP_Text text = CreateLabel(buttonObject.transform, label, 22, Vector2.zero);
        RectTransform textTransform = (RectTransform)text.transform;
        textTransform.anchorMin = Vector2.zero;
        textTransform.anchorMax = Vector2.one;
        textTransform.offsetMin = Vector2.zero;
        textTransform.offsetMax = Vector2.zero;
        text.color = Color.black;
        return button;
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();
    }
}
