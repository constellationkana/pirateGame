using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays and routes actions for one save slot entry in the main menu.
/// </summary>
public class SaveSlotEntryUI : MonoBehaviour
{
    private static readonly Color ButtonNormalColor = new(0.84313726f, 0.6666667f, 0.27450982f, 1f);
    private static readonly Color ButtonHighlightedColor = new(0.9411765f, 0.78431374f, 0.37254903f, 1f);
    private static readonly Color ButtonPressedColor = new(0.627451f, 0.47058824f, 0.1764706f, 1f);
    private static readonly Color ButtonSelectedColor = new(0.9f, 0.7f, 0.25f, 1f);
    private static readonly Color ButtonDisabledColor = new(0.43137255f, 0.4117647f, 0.37254903f, 0.65f);
    private static readonly Color DeleteNormalColor = new(0.45f, 0.16f, 0.09f, 1f);
    private static readonly Color DeleteHighlightedColor = new(0.62f, 0.22f, 0.12f, 1f);
    private static readonly Color DeletePressedColor = new(0.30f, 0.08f, 0.04f, 1f);
    private static readonly Color TextColor = new(0.13725491f, 0.09411765f, 0.039215688f, 1f);
    private static readonly Color DeleteTextColor = new(1f, 0.9098039f, 0.6666667f, 1f);

    [Header("Text")]
    [SerializeField] private TMP_Text saveNameText;
    [SerializeField] private TMP_Text doubloonsText;
    [SerializeField] private TMP_Text progressCountText;

    [Header("Buttons")]
    [SerializeField] private Button loadButton;
    [SerializeField] private Button renameButton;
    [SerializeField] private Button deleteButton;

    private int slotId;
    private Action<int> loadAction;
    private Action<int> renameAction;
    private Action<int> deleteAction;

    private void Awake()
    {
        ApplyPirateButtonStyle(loadButton, ButtonNormalColor, ButtonHighlightedColor, ButtonPressedColor, TextColor);
        ApplyPirateButtonStyle(renameButton, ButtonNormalColor, ButtonHighlightedColor, ButtonPressedColor, TextColor);
        ApplyPirateButtonStyle(deleteButton, DeleteNormalColor, DeleteHighlightedColor, DeletePressedColor, DeleteTextColor);
    }

    /// <summary>
    /// Configures this UI element with display data and callbacks.
    /// </summary>
    /// <param name="summary">Save-slot summary to display.</param>
    /// <param name="onLoad">Callback invoked when this slot is loaded.</param>
    /// <param name="onRename">Callback invoked when this slot is renamed.</param>
    /// <param name="onDelete">Callback invoked when this slot is deleted.</param>
    public void Configure(PlayerProgression.SaveSlotSummary summary, Action<int> onLoad, Action<int> onRename, Action<int> onDelete)
    {
        slotId = summary.slotId;
        loadAction = onLoad;
        renameAction = onRename;
        deleteAction = onDelete;

        if (saveNameText != null)
        {
            saveNameText.text = summary.saveName;
        }

        if (doubloonsText != null)
        {
            doubloonsText.text = string.Empty;
        }

        if (progressCountText != null)
        {
            progressCountText.text = $"D: {summary.doubloons} | Up: {summary.upgradeCount} | Un: {summary.unlockCount}";
        }

        if (loadButton != null)
        {
            loadButton.onClick.RemoveListener(Load);
            loadButton.onClick.AddListener(Load);
        }

        if (renameButton != null)
        {
            renameButton.onClick.RemoveListener(Rename);
            renameButton.onClick.AddListener(Rename);
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveListener(Delete);
            deleteButton.onClick.AddListener(Delete);
        }
    }

    private static void ApplyPirateButtonStyle(Button button, Color normalColor, Color highlightedColor, Color pressedColor, Color textColor)
    {
        if (button == null)
        {
            return;
        }

        Image buttonImage = button.GetComponent<Image>();
        if (buttonImage != null)
        {
            buttonImage.color = normalColor;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = ButtonSelectedColor;
        colors.disabledColor = ButtonDisabledColor;
        button.colors = colors;

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.color = textColor;
            buttonText.fontSize = Mathf.Max(buttonText.fontSize, 22f);
        }
    }

    /// <summary>
    /// Invokes the configured load action.
    /// </summary>
    public void Load() => loadAction?.Invoke(slotId);
    /// <summary>
    /// Invokes the configured rename action.
    /// </summary>
    public void Rename() => renameAction?.Invoke(slotId);
    /// <summary>
    /// Invokes the configured delete action.
    /// </summary>
    public void Delete() => deleteAction?.Invoke(slotId);
}
