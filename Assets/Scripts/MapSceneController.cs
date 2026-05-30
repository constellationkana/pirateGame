using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapSceneController : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainSeaSceneName = "MainSea";
    [SerializeField] private string shipShopSceneName = "ShipShop";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Fallback UI")]
    [SerializeField] private bool createFallbackMenuIfMissing = true;

    private void Start()
    {
        if (createFallbackMenuIfMissing && FindFirstObjectByType<Canvas>() == null)
        {
            CreateFallbackMapMenu();
        }
    }

    public void LoadStage1() => LoadStageMainSea();

    public void LoadStageMainSea()
    {
        LoadScene(mainSeaSceneName, nameof(mainSeaSceneName));
    }

    public void GoToShipShop()
    {
        LoadScene(shipShopSceneName, nameof(shipShopSceneName));
    }

    public void GoToMainMenu()
    {
        LoadScene(mainMenuSceneName, nameof(mainMenuSceneName));
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

        CreateLabel(panel.transform, "Map", 36, new Vector2(0f, 150f));
        CreateButton(panel.transform, "Stage 1: Main Sea", new Vector2(0f, 70f), LoadStage1);
        CreateButton(panel.transform, "Ship Shop", new Vector2(0f, 0f), GoToShipShop);
        CreateButton(panel.transform, "Main Menu", new Vector2(0f, -70f), GoToMainMenu);

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

    private static void CreateLabel(Transform parent, string text, int fontSize, Vector2 anchoredPosition)
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
    }

    private static void CreateButton(Transform parent, string label, Vector2 anchoredPosition, UnityEngine.Events.UnityAction onClick)
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

        CreateLabel(buttonObject.transform, label, 22, Vector2.zero);
    }
}
