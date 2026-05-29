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
        PlayerProgression.UnlockHealthRegenId,
        PlayerProgression.UnlockDashId,
        PlayerProgression.UnlockMagnetId,
        PlayerProgression.UnlockForceFieldId,
        PlayerProgression.UnlockCannonballSizeId,
        PlayerProgression.UnlockCannonballSpeedId,
        PlayerProgression.UnlockCannonballExplosionId,
        PlayerProgression.UnlockCannonballPierceId,
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
        builder.AppendLine($"Save Name: {PlayerProgression.GetActiveSaveName()}");
        builder.AppendLine($"Doubloons: {progression.GetDoubloons()}");
        builder.AppendLine("Permanent Upgrade Levels:");
        builder.AppendLine($"- {PlayerProgression.UpgradeBaseHealthId}: {progression.GetUpgradeLevel(PlayerProgression.UpgradeBaseHealthId)}");
        builder.AppendLine($"- {PlayerProgression.UpgradeBaseSpeedId}: {progression.GetUpgradeLevel(PlayerProgression.UpgradeBaseSpeedId)}");
        builder.AppendLine($"- {PlayerProgression.UpgradeBaseCannonDamageId}: {progression.GetUpgradeLevel(PlayerProgression.UpgradeBaseCannonDamageId)}");
        builder.AppendLine($"- {PlayerProgression.UpgradeBaseCannonballSpeedId}: {progression.GetUpgradeLevel(PlayerProgression.UpgradeBaseCannonballSpeedId)}");
        builder.AppendLine($"- {PlayerProgression.UpgradeBaseMagnetRadiusId}: {progression.GetUpgradeLevel(PlayerProgression.UpgradeBaseMagnetRadiusId)}");
        builder.AppendLine($"- {PlayerProgression.UpgradeExplosionPowerId}: {progression.GetUpgradeLevel(PlayerProgression.UpgradeExplosionPowerId)}");
        builder.AppendLine($"- {PlayerProgression.UpgradeBarnaclePowerId}: {progression.GetUpgradeLevel(PlayerProgression.UpgradeBarnaclePowerId)}");
        builder.AppendLine("Unlocked Abilities:");
        builder.AppendLine($"- Dash: {FormatYesNo(progression.IsDashUnlocked())}");
        builder.AppendLine($"- Force Field: {FormatYesNo(progression.IsForceFieldUnlocked())}");

        if (genericUnlockIdsToShow != null && genericUnlockIdsToShow.Length > 0)
        {
            builder.AppendLine("ShipShop Unlocks:");
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
