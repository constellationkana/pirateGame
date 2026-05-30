using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotEntryUI : MonoBehaviour
{
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

    public void Configure(PlayerProgression.SaveSlotSummary summary, Action<int> onLoad, Action<int> onRename, Action<int> onDelete)
    {
        slotId = summary.slotId;
        loadAction = onLoad;
        renameAction = onRename;
        deleteAction = onDelete;

        if (saveNameText != null)
        {
            saveNameText.text = summary.isActive ? $"{summary.saveName} (Active)" : summary.saveName;
        }

        if (doubloonsText != null)
        {
            doubloonsText.text = $"Doubloons: {summary.doubloons}";
        }

        if (progressCountText != null)
        {
            progressCountText.text = $"Upgrades: {summary.upgradeCount} | Unlocks: {summary.unlockCount}";
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

    public void Load() => loadAction?.Invoke(slotId);
    public void Rename() => renameAction?.Invoke(slotId);
    public void Delete() => deleteAction?.Invoke(slotId);
}
