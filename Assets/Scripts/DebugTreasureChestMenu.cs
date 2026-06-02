using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Developer-only runtime menu for tuning treasure chest spawn settings during a play session.
/// </summary>
[DisallowMultipleComponent]
public class DebugTreasureChestMenu : MonoBehaviour
{
    [SerializeField] private TreasureChestSpawner treasureChestSpawner;

    private GameObject panelRoot;
    private TMP_Text statusText;

    private void Awake()
    {
        ResolveSpawner();
        EnsureRuntimeUI();
        SetMenuVisible(false);
    }

    private void Update()
    {
        if (WasShiftUToggled())
        {
            ToggleMenu();
        }

        if (panelRoot != null && panelRoot.activeSelf)
        {
            ResolveSpawner();
            RefreshStatusText();
        }
    }

    /// <summary>
    /// Toggles the debug menu visibility without pausing gameplay.
    /// </summary>
    public void ToggleMenu()
    {
        EnsureRuntimeUI();
        SetMenuVisible(panelRoot == null || !panelRoot.activeSelf);
    }

    private static bool WasShiftUToggled()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.uKey.wasPressedThisFrame)
        {
            return false;
        }

        return keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
    }

    private void ResolveSpawner()
    {
        if (treasureChestSpawner == null)
        {
            treasureChestSpawner = FindFirstObjectByType<TreasureChestSpawner>();
        }
    }

    private void EnsureRuntimeUI()
    {
        if (panelRoot != null)
        {
            return;
        }

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new("Treasure Chest Debug Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        EnsureEventSystem();

        panelRoot = new GameObject("Treasure Chest Debug Panel", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(20f, -20f);
        panelRect.sizeDelta = new Vector2(360f, 460f);

        Image panelImage = panelRoot.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.82f);

        VerticalLayoutGroup layout = panelRoot.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(16, 16, 14, 14);
        layout.spacing = 8f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateText("Treasure Chest Debug", 26, FontStyles.Bold, 36f);
        statusText = CreateText(string.Empty, 16, FontStyles.Normal, 84f);

        CreateButton("Chance 0%", () => SetChance(0f));
        CreateButton("Chance 50%", () => SetChance(0.5f));
        CreateButton("Chance 100%", () => SetChance(1f));
        CreateButton("Interval 0.5s", () => SetInterval(0.5f));
        CreateButton("Interval 5s", () => SetInterval(5f));
        CreateButton("Interval 30s", () => SetInterval(30f));
        CreateButton("Max -1", () => AdjustMaxActiveChests(-1));
        CreateButton("Max +1", () => AdjustMaxActiveChests(1));
        CreateButton("Max 10", () => SetMaxActiveChests(10));
        CreateButton("Max 50", () => SetMaxActiveChests(50));
        CreateButton("Force Spawn Chest", ForceSpawnChest);
        CreateButton("Reset Defaults", ResetDefaults);
        CreateButton("Close", () => SetMenuVisible(false));

        RefreshStatusText();
    }

    private TMP_Text CreateText(string text, int fontSize, FontStyles fontStyle, float preferredHeight)
    {
        GameObject textObject = new("Debug Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(panelRoot.transform, false);

        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.fontStyle = fontStyle;
        tmpText.color = Color.white;
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.enableWordWrapping = true;

        LayoutElement layoutElement = textObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = preferredHeight;
        return tmpText;
    }

    private void CreateButton(string label, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new(label, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(panelRoot.transform, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.88f, 0.67f, 0.24f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(action);

        LayoutElement buttonLayout = buttonObject.AddComponent<LayoutElement>();
        buttonLayout.preferredHeight = 30f;

        GameObject labelObject = new("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text labelText = labelObject.GetComponent<TMP_Text>();
        labelText.text = label;
        labelText.fontSize = 18;
        labelText.color = Color.black;
        labelText.alignment = TextAlignmentOptions.Center;
    }

    private void SetChance(float value)
    {
        ResolveSpawner();
        if (treasureChestSpawner != null)
        {
            treasureChestSpawner.SpawnChance = value;
            RefreshStatusText();
        }
    }

    private void SetInterval(float value)
    {
        ResolveSpawner();
        if (treasureChestSpawner != null)
        {
            treasureChestSpawner.SpawnInterval = value;
            RefreshStatusText();
        }
    }

    private void AdjustMaxActiveChests(int delta)
    {
        ResolveSpawner();
        if (treasureChestSpawner != null)
        {
            treasureChestSpawner.MaxActiveChests += delta;
            RefreshStatusText();
        }
    }

    private void SetMaxActiveChests(int value)
    {
        ResolveSpawner();
        if (treasureChestSpawner != null)
        {
            treasureChestSpawner.MaxActiveChests = value;
            RefreshStatusText();
        }
    }

    private void ForceSpawnChest()
    {
        ResolveSpawner();
        if (treasureChestSpawner == null)
        {
            Debug.LogWarning("DebugTreasureChestMenu: No TreasureChestSpawner found in this scene.", this);
            RefreshStatusText();
            return;
        }

        treasureChestSpawner.ForceSpawnChest();
        RefreshStatusText();
    }

    private void ResetDefaults()
    {
        ResolveSpawner();
        if (treasureChestSpawner != null)
        {
            treasureChestSpawner.ResetDebugValues();
            RefreshStatusText();
        }
    }

    private void SetMenuVisible(bool visible)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(visible);
            if (visible)
            {
                RefreshStatusText();
            }
        }
    }

    private void RefreshStatusText()
    {
        if (statusText == null)
        {
            return;
        }

        if (treasureChestSpawner == null)
        {
            statusText.text = "No TreasureChestSpawner found in this scene.";
            return;
        }

        statusText.text =
            $"Spawner: {treasureChestSpawner.name}\n" +
            $"Chance: {treasureChestSpawner.SpawnChance:0.00}\n" +
            $"Interval: {treasureChestSpawner.SpawnInterval:0.0}s\n" +
            $"Max Active: {treasureChestSpawner.MaxActiveChests}\n" +
            $"Active Chests: {treasureChestSpawner.ActiveChestCount}";
    }

    private static void EnsureEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }
}
