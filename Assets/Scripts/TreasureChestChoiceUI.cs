using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    private Action<TreasureChestChoice> onChoiceSelected;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        EnsureRuntimeUI();
        HidePanel(false);
    }

    public void ShowChoices(List<TreasureChestChoice> options, Action<TreasureChestChoice> onChosen)
    {
        if (options == null || options.Count == 0)
        {
            return;
        }

        EnsureRuntimeUI();
        onChoiceSelected = onChosen;

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

            if (i >= options.Count)
            {
                widgets.button.gameObject.SetActive(false);
                continue;
            }

            TreasureChestChoice option = options[i];
            widgets.button.gameObject.SetActive(true);
            widgets.button.onClick.RemoveAllListeners();
            widgets.button.onClick.AddListener(() => SelectOption(option));

            if (widgets.nameText != null)
            {
                widgets.nameText.text = option.Name;
            }

            if (widgets.descriptionText != null)
            {
                widgets.descriptionText.text = option.Description;
            }

            if (widgets.typeText != null)
            {
                widgets.typeText.text = FormatChoiceType(option.Type);
            }
        }
    }

    private void SelectOption(TreasureChestChoice option)
    {
        onChoiceSelected?.Invoke(option);
        HidePanel(true);
    }

    private void HidePanel(bool restoreTimeScale)
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (restoreTimeScale)
        {
            Time.timeScale = Mathf.Approximately(previousTimeScale, 0f) ? 1f : previousTimeScale;
        }
    }

    private void EnsureRuntimeUI()
    {
        if (HasConfiguredChoices())
        {
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
        background.color = new Color(0f, 0f, 0f, 0.78f);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(80, 80, 60, 60);
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text titleText = CreateText("Title", root.transform, title, 42, TextAlignmentOptions.Center);
        LayoutElement titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 58f;

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
        image.color = new Color(0.2f, 0.13f, 0.05f, 0.96f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.55f, 0.36f, 0.1f, 1f);
        colors.pressedColor = new Color(0.85f, 0.6f, 0.18f, 1f);
        button.colors = colors;

        LayoutElement layoutElement = buttonObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 92f;

        VerticalLayoutGroup layout = buttonObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(20, 20, 8, 8);
        layout.spacing = 2f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text typeText = CreateText("Type", buttonObject.transform, "Upgrade", 18, TextAlignmentOptions.Center);
        TMP_Text nameText = CreateText("Name", buttonObject.transform, "Choice", 26, TextAlignmentOptions.Center);
        TMP_Text descriptionText = CreateText("Description", buttonObject.transform, "Description", 18, TextAlignmentOptions.Center);

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
        return tmpText;
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
