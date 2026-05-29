using System.Text;
using TMPro;
using UnityEngine;

public class PauseProgressionMenu : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.U;

    [Header("Panel")]
    [SerializeField] private GameObject menuPanel;

    [Header("Text")]
    [SerializeField] private TMP_Text activeSaveNameText;
    [SerializeField] private TMP_Text progressionSummaryText;
    [SerializeField] private TMP_Text messageText;

    [Header("Generic Unlocks Shown")]
    [SerializeField]
    private string[] genericUnlockIdsToShow =
    {
        PlayerProgression.UnlockMagnetRadius,
        PlayerProgression.UnlockDashId,
        PlayerProgression.UnlockForceFieldId,
        PlayerProgression.UnlockHealthRegenId,
        PlayerProgression.UnlockCannonballSizeId,
        PlayerProgression.UnlockCannonballSpeedId,
        PlayerProgression.UnlockCannonballPierceId,
        PlayerProgression.UnlockCannonballExplosionId,
        PlayerProgression.UnlockBarnaclesId,
        PlayerProgression.UnlockCursedDoubloonsId
    };

    private bool isOpen;
    private float previousTimeScale = 1f;

    private void Start()
    {
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (isOpen)
        {
            ResumeGame();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (isOpen)
        {
            return;
        }

        isOpen = true;
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
        }

        RefreshProgressionText();
    }

    public void ResumeGame()
    {
        isOpen = false;
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;

        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }

    public void SaveGame()
    {
        PlayerProgression.SaveActiveSlot();
        PlayerPrefs.Save();

        if (messageText != null)
        {
            messageText.text = "Game saved.";
        }

        Debug.Log($"PauseProgressionMenu: Saved active slot '{PlayerProgression.GetActiveSaveName()}'.", this);
    }

    public void RefreshProgressionText()
    {
        PlayerProgression progression = PlayerProgression.Instance;

        if (activeSaveNameText != null)
        {
            activeSaveNameText.text = $"Active Save: {PlayerProgression.GetActiveSaveName()}";
        }

        if (progressionSummaryText == null)
        {
            return;
        }

        StringBuilder builder = new();
        builder.AppendLine($"Doubloons: {progression.GetDoubloons()}");
        builder.AppendLine($"Base Health Level: {progression.GetPermanentHealthLevel()}");
        builder.AppendLine($"Base Speed Level: {progression.GetPermanentSpeedLevel()}");
        builder.AppendLine($"Cannon Damage Level: {progression.GetPermanentCannonDamageLevel()}");
        builder.AppendLine($"Magnet Level: {progression.GetPermanentMagnetLevel()}");
        builder.AppendLine($"Dash Unlocked: {FormatYesNo(progression.IsDashUnlocked())}");
        builder.AppendLine($"Force Field Unlocked: {FormatYesNo(progression.IsForceFieldUnlocked())}");

        if (genericUnlockIdsToShow != null && genericUnlockIdsToShow.Length > 0)
        {
            builder.AppendLine("Generic Unlocks:");
            for (int i = 0; i < genericUnlockIdsToShow.Length; i++)
            {
                string unlockId = genericUnlockIdsToShow[i];
                if (!string.IsNullOrWhiteSpace(unlockId))
                {
                    builder.AppendLine($"- {unlockId}: {FormatYesNo(progression.IsUnlocked(unlockId))}");
                }
            }
        }

        progressionSummaryText.text = builder.ToString();
    }

    private static string FormatYesNo(bool value) => value ? "Yes" : "No";
}
