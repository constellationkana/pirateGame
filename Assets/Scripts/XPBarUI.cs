using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    [SerializeField] private PlayerLevelSystem playerLevelSystem;
    [SerializeField] private Slider xpSlider;
    [SerializeField] private TMP_Text levelText;

    private void OnEnable()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.OnXPChanged += Refresh;
            playerLevelSystem.OnLevelUp += HandleLevelUp;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.OnXPChanged -= Refresh;
            playerLevelSystem.OnLevelUp -= HandleLevelUp;
        }
    }

    private void Start()
    {
        Refresh();
    }

    private void HandleLevelUp(int _)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (playerLevelSystem == null)
        {
            return;
        }

        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = playerLevelSystem.XPRequiredForNextLevel;
            xpSlider.value = playerLevelSystem.CurrentXP;
        }

        if (levelText != null)
        {
            levelText.text = $"Level {playerLevelSystem.CurrentLevel}";
        }
    }
}
