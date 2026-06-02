using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays level-up upgrade choices and invokes a callback when one is selected.
/// </summary>
public class UpgradeChoiceUI : MonoBehaviour
{
    [Serializable]
    private class ChoiceWidgets
    {
        /// <summary>
        /// UI Button reference used by this entry.
        /// </summary>
        public Button button;
        /// <summary>
        /// Title text shown by this UI entry.
        /// </summary>
        public TMP_Text titleText;
        /// <summary>
        /// Description text shown by this UI entry.
        /// </summary>
        public TMP_Text descriptionText;
    }

    [SerializeField] private GameObject panelRoot;
    [SerializeField] private ChoiceWidgets[] choices = new ChoiceWidgets[3];

    private Action<UpgradeManager.UpgradeOption> onChoiceSelected;
    private List<UpgradeManager.UpgradeOption> cachedOptions;

    private void Awake()
    {
        HidePanel();
    }

    /// <summary>
    /// Shows selectable UI choices and reports the selected option.
    /// </summary>
    /// <param name="options">Options value used by this method.</param>
    /// <param name="onChosen">On chosen value used by this method.</param>
    public void ShowChoices(List<UpgradeManager.UpgradeOption> options, Action<UpgradeManager.UpgradeOption> onChosen)
    {
        if (options == null || options.Count == 0)
        {
            return;
        }

        cachedOptions = options;
        onChoiceSelected = onChosen;

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

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

            UpgradeManager.UpgradeOption option = options[i];
            widgets.button.gameObject.SetActive(true);
            widgets.button.onClick.RemoveAllListeners();
            widgets.button.onClick.AddListener(() => SelectOption(option));

            if (widgets.titleText != null)
            {
                widgets.titleText.text = option.displayName;
            }

            if (widgets.descriptionText != null)
            {
                widgets.descriptionText.text = option.description;
            }
        }
    }

    private void SelectOption(UpgradeManager.UpgradeOption option)
    {
        onChoiceSelected?.Invoke(option);
        HidePanel();
    }

    private void HidePanel()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        Time.timeScale = 1f;
    }
}
