using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Displays treasure chest reward options and invokes callbacks when the player chooses one.
/// </summary>
public class TreasureChestChoiceUI : MonoBehaviour
{
    [Serializable]
    private class ChoiceWidgets
    {
        public Button button;
        public TMP_Text nameText;
        public TMP_Text descriptionText;
        public TMP_Text typeText;
    }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private ChoiceWidgets[] choices = new ChoiceWidgets[5];
    [SerializeField] private string title = "Treasure Chest";

    private static readonly Color PanelColor = new(0.11f, 0.07f, 0.035f, 0.94f);
    private static readonly Color GoldColor = new(0.93f, 0.68f, 0.22f, 1f);
    private static readonly Color ButtonColor = new(0.82f, 0.56f, 0.16f, 1f);
    private static readonly Color ButtonHighlightColor = new(1f, 0.76f, 0.28f, 1f);
    private static readonly Color ButtonPressedColor = new(0.58f, 0.34f, 0.08f, 1f);
    private static readonly Color ButtonDisabledColor = new(0.38f, 0.34f, 0.28f, 0.65f);
    private static readonly Color ButtonTextColor = new(0.12f, 0.07f, 0.025f, 1f);

    private const int MaxChestSelections = 3;

    private readonly HashSet<int> selectedChoiceIndexes = new();
    private Action<TreasureChestChoice> onChoiceSelected;
    private Action onSelectionComplete;
    private int requiredSelectionCount;
    private int selectedChoiceCount;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        EnsureRuntimeUI();
        ApplyTreasureChestStyle();
        HidePanel(false);
    }

    /// <summary>
    /// Shows reward options and reports the selected choice.
    /// </summary>
    /// <param name="options">Reward options to display.</param>
    /// <param name="onChosen">Callback invoked with the selected choice.</param>
    public void ShowChoices(List<TreasureChestChoice> options, Action<TreasureChestChoice> onChosen)
    {
        ShowChoices(options, onChosen, null);
    }

    /// <summary>
    /// Shows reward options and reports the selected choice.
    /// </summary>
    /// <param name="options">Reward options to display.</param>
    /// <param name="onChosen">Callback invoked with the selected choice.</param>
    /// <param name="onComplete">Callback invoked after a choice is applied.</param>
    public void ShowChoices(List<TreasureChestChoice> options, Action<TreasureChestChoice> onChosen, Action onComplete)
    {
        if (options == null || options.Count == 0)
        {
            return;
        }

        EnsureRuntimeUI();
        onChoiceSelected = onChosen;
        onSelectionComplete = onComplete;
        selectedChoiceIndexes.Clear();
        selectedChoiceCount = 0;
        requiredSelectionCount = Mathf.Min(MaxChestSelections, options.Count);
        UpdateTitleText();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        for (int i = 0; i < choices.Length; i++)
        {
            ChoiceWidgets widgets = choices[i];
            if (widgets == null || widgets.button == null)
            {
                continue;
            }

            widgets.button.onClick.RemoveAllListeners();
            if (i >= options.Count)
            {
                widgets.button.gameObject.SetActive(false);
                continue;
            }

            int choiceIndex = i;
            TreasureChestChoice option = options[i];
            widgets.button.gameObject.SetActive(true);
            widgets.button.interactable = true;
            widgets.button.onClick.AddListener(() => SelectOption(choiceIndex, option));

            if (widgets.nameText != null)
            {
                widgets.nameText.text = FormatChoiceName(option);
            }

            if (widgets.descriptionText != null)
            {
                widgets.descriptionText.text = FormatChoiceDescription(option);
            }

            if (widgets.typeText != null)
            {
                widgets.typeText.text = FormatChoiceType(option.Type);
            }

            SetChoiceSelected(widgets, false);
        }
    }

    private void SelectOption(int choiceIndex, TreasureChestChoice option)
    {
        if (selectedChoiceIndexes.Contains(choiceIndex) || selectedChoiceCount >= requiredSelectionCount)
        {
            return;
        }

        selectedChoiceIndexes.Add(choiceIndex);
        selectedChoiceCount++;
        onChoiceSelected?.Invoke(option);

        if (choices != null && choiceIndex >= 0 && choiceIndex < choices.Length)
        {
            SetChoiceSelected(choices[choiceIndex], true);
        }

        UpdateTitleText();

        if (selectedChoiceCount >= requiredSelectionCount)
        {
            Action completed = onSelectionComplete;
            onSelectionComplete = null;
            HidePanel(true);
            completed?.Invoke();
        }
    }

    private void HidePanel(bool restoreTimeScale)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        selectedChoiceIndexes.Clear();
        selectedChoiceCount = 0;
        requiredSelectionCount = 0;

        if (restoreTimeScale)
        {
            Time.timeScale = Mathf.Approximately(previousTimeScale, 0f) ? 1f : previousTimeScale;
        }
    }

    private void EnsureRuntimeUI()
    {
        if (HasConfiguredChoices())
        {
            ApplyTreasureChestStyle();
            return;
        }

        EnsureEventSystem();

        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new("Treasure Chest Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject root = new("Treasure Chest Choice Panel");
        root.transform.SetParent(canvas.transform, false);
        panelRoot = root;

        Image background = root.AddComponent<Image>();
        background.color = PanelColor;
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.anchoredPosition = Vector2.zero;
        rootRect.sizeDelta = new Vector2(700f, 520f);

        VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(50, 50, 30, 34);
        layout.spacing = 10f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text titleText = CreateText("Title", root.transform, "Choose Your Plunder", 42, TextAlignmentOptions.Center);
        LayoutElement titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 64f;

        choices = new ChoiceWidgets[5];
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i] = CreateChoiceWidgets(root.transform, i + 1);
        }
    }

    private bool HasConfiguredChoices()
    {
        if (panelRoot == null || choices == null || choices.Length < 5)
        {
            return false;
        }

        for (int i = 0; i < 5; i++)
        {
            if (choices[i] == null || choices[i].button == null)
            {
                return false;
            }
        }

        return true;
    }

    private ChoiceWidgets CreateChoiceWidgets(Transform parent, int index)
    {
        GameObject buttonObject = new($"Treasure Choice {index}");
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = ButtonColor;

        Button button = buttonObject.AddComponent<Button>();
        ApplyButtonColors(button);

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 72f;

        VerticalLayoutGroup layout = buttonObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(22, 22, 6, 6);
        layout.spacing = 0f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text typeText = CreateText("Type", buttonObject.transform, "Upgrade", 14, TextAlignmentOptions.Center);
        TMP_Text nameText = CreateText("Name", buttonObject.transform, "Choice", 22, TextAlignmentOptions.Center);
        TMP_Text descriptionText = CreateText("Description", buttonObject.transform, "Description", 16, TextAlignmentOptions.Center);
        StyleChoiceText(typeText, 14, ButtonTextColor, FontStyles.UpperCase);
        StyleChoiceText(nameText, 22, ButtonTextColor, FontStyles.Bold);
        StyleChoiceText(descriptionText, 16, ButtonTextColor, FontStyles.Normal);

        return new ChoiceWidgets
        {
            button = button,
            nameText = nameText,
            descriptionText = descriptionText,
            typeText = typeText
        };
    }

    private static TMP_Text CreateText(string objectName, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
    {
        GameObject textObject = new(objectName);
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.alignment = alignment;
        tmpText.color = Color.white;
        tmpText.enableWordWrapping = true;
        return tmpText;
    }

    private void ApplyTreasureChestStyle()
    {
        if (panelRoot == null)
        {
            return;
        }

        RectTransform panelRect = panelRoot.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(700f, 520f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
        }

        if (panelRoot.TryGetComponent(out Image panelImage))
        {
            panelImage.color = PanelColor;
        }

        StyleDecorativeAccent();
        StyleTitleText();

        if (choices == null)
        {
            return;
        }

        for (int i = 0; i < choices.Length; i++)
        {
            StyleChoiceWidget(choices[i], i);
        }
    }

    private void StyleDecorativeAccent()
    {
        foreach (Image image in panelRoot.GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject == panelRoot || image.GetComponent<Button>() != null)
            {
                continue;
            }

            image.color = new Color(GoldColor.r, GoldColor.g, GoldColor.b, 0.35f);
            image.raycastTarget = false;
            if (image.transform is RectTransform rectTransform)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 1f);
                rectTransform.anchorMax = new Vector2(0.5f, 1f);
                rectTransform.anchoredPosition = new Vector2(0f, -76f);
                rectTransform.sizeDelta = new Vector2(600f, 4f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
            }

            return;
        }
    }

    private void StyleTitleText()
    {
        TMP_Text titleText = null;
        foreach (TMP_Text textElement in panelRoot.GetComponentsInChildren<TMP_Text>(true))
        {
            if (textElement.gameObject.name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                titleText = textElement;
                break;
            }
        }

        if (titleText == null)
        {
            return;
        }

        titleText.text = GetTitleText();
        titleText.fontSize = 42f;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = GoldColor;
        titleText.enableWordWrapping = false;

        if (titleText.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = new Vector2(0f, -42f);
            rectTransform.sizeDelta = new Vector2(640f, 58f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    private void StyleChoiceWidget(ChoiceWidgets widgets, int index)
    {
        if (widgets == null || widgets.button == null)
        {
            return;
        }

        RectTransform buttonRect = widgets.button.GetComponent<RectTransform>();
        if (buttonRect != null)
        {
            buttonRect.anchorMin = new Vector2(0.5f, 1f);
            buttonRect.anchorMax = new Vector2(0.5f, 1f);
            buttonRect.anchoredPosition = new Vector2(0f, -115f - (index * 78f));
            buttonRect.sizeDelta = new Vector2(560f, 70f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
        }

        if (widgets.button.TryGetComponent(out Image buttonImage))
        {
            buttonImage.color = ButtonColor;
        }

        ApplyButtonColors(widgets.button);
        PositionChoiceText(widgets.typeText, 14f, new Vector2(0f, 21f), new Vector2(520f, 18f), FontStyles.UpperCase);
        PositionChoiceText(widgets.nameText, 22f, new Vector2(0f, 3f), new Vector2(520f, 26f), FontStyles.Bold);
        PositionChoiceText(widgets.descriptionText, 16f, new Vector2(0f, -22f), new Vector2(520f, 22f), FontStyles.Normal);
    }

    private static void PositionChoiceText(TMP_Text text, float fontSize, Vector2 anchoredPosition, Vector2 sizeDelta, FontStyles fontStyle)
    {
        if (text == null)
        {
            return;
        }

        StyleChoiceText(text, fontSize, ButtonTextColor, fontStyle);

        if (text.transform is RectTransform rectTransform)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = sizeDelta;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
        }
    }

    private static void StyleChoiceText(TMP_Text text, float fontSize, Color color, FontStyles fontStyle)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAlignmentOptions.Center;
        text.color = color;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
    }

    private void SetChoiceSelected(ChoiceWidgets widgets, bool isSelected)
    {
        if (widgets == null || widgets.button == null)
        {
            return;
        }

        widgets.button.interactable = !isSelected;

        if (widgets.button.TryGetComponent(out Image buttonImage))
        {
            buttonImage.color = isSelected ? ButtonDisabledColor : ButtonColor;
        }

        if (isSelected)
        {
            if (widgets.typeText != null)
            {
                widgets.typeText.text = $"Selected {widgets.typeText.text}";
            }

            if (widgets.nameText != null && !widgets.nameText.text.StartsWith("✓ ", StringComparison.Ordinal))
            {
                widgets.nameText.text = $"✓ {widgets.nameText.text}";
            }
        }
    }

    private void UpdateTitleText()
    {
        if (panelRoot == null)
        {
            return;
        }

        foreach (TMP_Text textElement in panelRoot.GetComponentsInChildren<TMP_Text>(true))
        {
            if (textElement.gameObject.name.IndexOf("Title", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                textElement.text = GetTitleText();
                return;
            }
        }
    }

    private string GetTitleText()
    {
        string baseTitle = string.IsNullOrWhiteSpace(title) ? "Choose Your Plunder" : title;
        return requiredSelectionCount > 0 ? $"{baseTitle} ({selectedChoiceCount}/{requiredSelectionCount})" : baseTitle;
    }

    private static void ApplyButtonColors(Button button)
    {
        if (button == null)
        {
            return;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = ButtonColor;
        colors.highlightedColor = ButtonHighlightColor;
        colors.pressedColor = ButtonPressedColor;
        colors.selectedColor = ButtonHighlightColor;
        colors.disabledColor = ButtonDisabledColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private static string FormatChoiceName(TreasureChestChoice option)
    {
        if (option == null)
        {
            return string.Empty;
        }

        string name = string.IsNullOrWhiteSpace(option.Name) ? FormatChoiceType(option.Type) : option.Name.Trim();
        return name switch
        {
            "Health Upgrade" => "Increase Max Health",
            "Cannonball Damage" => "Increase Cannon Damage",
            "Ship Speed" => "Increase Ship Movement Speed",
            "Magnet Radius Upgrade" => "Increase Pickup Magnet Radius",
            "Cannonball Speed" => "Increase Cannonball Speed",
            "Dash Upgrade" => "Improve Dash",
            "Force Field Upgrade" => "Improve Force Field",
            _ => name
        };
    }

    private static string FormatChoiceDescription(TreasureChestChoice option)
    {
        if (option == null || string.IsNullOrWhiteSpace(option.Description))
        {
            return string.Empty;
        }

        string description = option.Description.Trim();
        return string.Equals(description, option.Name, StringComparison.OrdinalIgnoreCase) ? string.Empty : description;
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

    private static string FormatChoiceType(TreasureChestChoiceType choiceType)
    {
        return choiceType switch
        {
            TreasureChestChoiceType.Crew => "Crew",
            TreasureChestChoiceType.CrewUpgrade => "Crew Upgrade",
            _ => "Upgrade"
        };
    }
}
