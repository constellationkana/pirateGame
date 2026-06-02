using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles map scene buttons, stage lock state, and scene navigation.
/// </summary>
public class MapSceneController : MonoBehaviour
{
    [Header("Stage Scene Names")]
    [SerializeField] private string stage1SceneName = "MainSea";
    [SerializeField] private string stage2SceneName = "MainSea";
    [SerializeField] private string stage3SceneName = "MainSea";

    [Header("Navigation Scene Names")]
    [SerializeField] private string shipShopSceneName = "ShipShop";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Stage Buttons")]
    [SerializeField] private Button stage1Button;
    [SerializeField] private Button stage2Button;
    [SerializeField] private Button stage3Button;

    [Header("Stage Labels")]
    [SerializeField] private TMP_Text stage1Label;
    [SerializeField] private TMP_Text stage2Label;
    [SerializeField] private TMP_Text stage3Label;

    [Header("Fallback UI")]
    [SerializeField] private bool createFallbackMenuIfMissing = true;

    private int currentStageNumber = 1;

    private void Start()
    {
        if (createFallbackMenuIfMissing && FindFirstObjectByType<Canvas>() == null)
        {
            CreateFallbackMapMenu();
        }

        BindExistingStageReferences();
        CreateMissingStageButtons();
        WireStageButtons();
        RefreshStageButtons();
    }

    /// <summary>
    /// Loads Stage 1 when it is available.
    /// </summary>
    public void LoadStage1() => LoadStage(1, stage1SceneName, nameof(stage1SceneName));
    /// <summary>
    /// Loads Stage 2 when it is available.
    /// </summary>
    public void LoadStage2() => LoadStage(2, stage2SceneName, nameof(stage2SceneName));
    /// <summary>
    /// Loads Stage 3 when it is available.
    /// </summary>
    public void LoadStage3() => LoadStage(3, stage3SceneName, nameof(stage3SceneName));

    /// <summary>
    /// Loads the configured Stage 1/MainSea scene path when available.
    /// </summary>
    public void LoadStageMainSea() => LoadStage1();

    /// <summary>
    /// Marks the configured current stage complete for testing or UI buttons.
    /// </summary>
    public void CompleteCurrentStage() => CompleteStage(currentStageNumber);
    /// <summary>
    /// Marks Stage 1 complete for testing or UI buttons.
    /// </summary>
    public void CompleteStage1() => CompleteStage(1);
    /// <summary>
    /// Marks Stage 2 complete for testing or UI buttons.
    /// </summary>
    public void CompleteStage2() => CompleteStage(2);
    /// <summary>
    /// Marks Stage 3 complete for testing or UI buttons.
    /// </summary>
    public void CompleteStage3() => CompleteStage(3);

    /// <summary>
    /// Loads the configured ship shop scene from the map.
    /// </summary>
    public void GoToShipShop()
    {
        LoadScene(shipShopSceneName, nameof(shipShopSceneName));
    }

    /// <summary>
    /// Loads the configured main menu scene.
    /// </summary>
    public void GoToMainMenu()
    {
        LoadScene(mainMenuSceneName, nameof(mainMenuSceneName));
    }

    private void BindExistingStageReferences()
    {
        BindExistingStageReference("Stage 1", ref stage1Button, ref stage1Label);
        BindExistingStageReference("Stage 2", ref stage2Button, ref stage2Label);
        BindExistingStageReference("Stage 3", ref stage3Button, ref stage3Label);
    }

    private static void BindExistingStageReference(string objectName, ref Button button, ref TMP_Text label)
    {
        if (button == null)
        {
            GameObject buttonObject = GameObject.Find(objectName);
            if (buttonObject != null)
            {
                button = buttonObject.GetComponent<Button>();
            }
        }

        if (label == null && button != null)
        {
            label = button.GetComponentInChildren<TMP_Text>();
        }
    }

    private void CreateMissingStageButtons()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        Transform parent = stage1Button != null ? stage1Button.transform.parent : canvas.transform;
        Vector2 basePosition = stage1Button != null
            ? ((RectTransform)stage1Button.transform).anchoredPosition
            : new Vector2(0f, 100f);

        if (stage1Button == null)
        {
            stage1Button = CreateButton(parent, "Stage 1", basePosition, LoadStage1, out stage1Label);
        }

        if (stage2Button == null)
        {
            stage2Button = CreateButton(parent, "Stage 2", basePosition + new Vector2(0f, -60f), LoadStage2, out stage2Label);
        }

        if (stage3Button == null)
        {
            stage3Button = CreateButton(parent, "Stage 3", basePosition + new Vector2(0f, -120f), LoadStage3, out stage3Label);
        }
    }

    private void WireStageButtons()
    {
        WireStageButton(stage1Button, LoadStage1);
        WireStageButton(stage2Button, LoadStage2);
        WireStageButton(stage3Button, LoadStage3);
    }

    private static void WireStageButton(Button button, UnityEngine.Events.UnityAction onClick)
    {
        if (button == null) return;
        button.onClick.RemoveListener(onClick);
        button.onClick.AddListener(onClick);
    }

    private void RefreshStageButtons()
    {
        RefreshStageButton(stage1Button, stage1Label, 1);
        RefreshStageButton(stage2Button, stage2Label, 2);
        RefreshStageButton(stage3Button, stage3Label, 3);
    }

    private void RefreshStageButton(Button button, TMP_Text label, int stageNumber)
    {
        bool isUnlocked = PlayerProgression.Instance.IsStageUnlocked(stageNumber);

        if (button != null)
        {
            button.interactable = isUnlocked;
        }

        if (label != null)
        {
            label.text = $"Stage {stageNumber} - {(isUnlocked ? "Unlocked" : "Locked")}";
        }
    }

    private void LoadStage(int stageNumber, string sceneName, string fieldName)
    {
        if (!PlayerProgression.Instance.IsStageUnlocked(stageNumber))
        {
            Debug.LogWarning($"MapSceneController: Stage {stageNumber} is locked.", this);
            RefreshStageButtons();
            return;
        }

        currentStageNumber = stageNumber;
        LoadScene(sceneName, fieldName);
    }

    private void CompleteStage(int stageNumber)
    {
        PlayerProgression.Instance.CompleteStage(stageNumber);
        PlayerPrefs.Save();
        currentStageNumber = Mathf.Max(1, stageNumber);
        RefreshStageButtons();
    }

    private void LoadScene(string sceneName, string fieldName)
    {
        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning($"MapSceneController: {fieldName} is empty.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    private void CreateFallbackMapMenu()
    {
        Canvas canvas = CreateCanvas();
        GameObject panel = CreatePanel(canvas.transform);

        CreateLabel(panel.transform, "Map", 36, new Vector2(0f, 180f));
        stage1Button = CreateButton(panel.transform, "Stage 1", new Vector2(0f, 100f), LoadStage1, out stage1Label);
        stage2Button = CreateButton(panel.transform, "Stage 2", new Vector2(0f, 40f), LoadStage2, out stage2Label);
        stage3Button = CreateButton(panel.transform, "Stage 3", new Vector2(0f, -20f), LoadStage3, out stage3Label);
        CreateButton(panel.transform, "Ship Shop", new Vector2(0f, -90f), GoToShipShop, out _);
        CreateButton(panel.transform, "Main Menu", new Vector2(0f, -150f), GoToMainMenu, out _);

        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new("Map Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static GameObject CreatePanel(Transform parent)
    {
        GameObject panel = new("Map Panel");
        panel.transform.SetParent(parent, false);
        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.05f, 0.13f, 0.22f, 0.95f);
        return panel;
    }

    private static TMP_Text CreateLabel(Transform parent, string text, int fontSize, Vector2 anchoredPosition)
    {
        GameObject labelObject = new(text);
        labelObject.transform.SetParent(parent, false);
        RectTransform rectTransform = labelObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(420f, 60f);
        rectTransform.anchoredPosition = anchoredPosition;

        TMP_Text label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        return label;
    }

    private static Button CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick, out TMP_Text labelText)
    {
        GameObject buttonObject = new(label);
        buttonObject.transform.SetParent(parent, false);
        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(300f, 48f);
        rectTransform.anchoredPosition = anchoredPosition;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.85f, 0.67f, 0.32f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        labelText = CreateLabel(buttonObject.transform, label, 22, Vector2.zero);
        return button;
    }
}
