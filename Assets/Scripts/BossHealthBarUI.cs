using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject root;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text healthText;

    [Header("Text")]
    [SerializeField] private string defaultBossName = "Boss";

    [Header("Health Colors")]
    [SerializeField] private Color highHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    private ShipHealth bossHealth;
    private ShipHealth subscribedHealth;
    private string currentBossName;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        if (healthSlider == null)
        {
            healthSlider = GetComponentInChildren<Slider>(true);
        }

        if (fillImage == null && healthSlider != null && healthSlider.fillRect != null)
        {
            fillImage = healthSlider.fillRect.GetComponent<Image>();
        }

        Hide();
    }

    private void OnEnable()
    {
        SubscribeToBoss();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromBoss();
    }

    public void SetBoss(ShipHealth newBossHealth, string bossName)
    {
        UnsubscribeFromBoss();

        bossHealth = newBossHealth;
        currentBossName = string.IsNullOrWhiteSpace(bossName) ? defaultBossName : bossName;

        if (bossHealth == null || bossHealth.IsDead)
        {
            Hide();
            return;
        }

        Show();
        SubscribeToBoss();
        Refresh();
    }

    private void SubscribeToBoss()
    {
        if (bossHealth == null || subscribedHealth == bossHealth)
        {
            return;
        }

        bossHealth.OnHealthChanged += HandleBossHealthChanged;
        bossHealth.OnDeath += HandleBossDeath;
        subscribedHealth = bossHealth;
    }

    private void UnsubscribeFromBoss()
    {
        if (subscribedHealth == null)
        {
            return;
        }

        subscribedHealth.OnHealthChanged -= HandleBossHealthChanged;
        subscribedHealth.OnDeath -= HandleBossDeath;
        subscribedHealth = null;
    }

    private void HandleBossHealthChanged(ShipHealth _)
    {
        Refresh();
    }

    private void HandleBossDeath(ShipHealth _)
    {
        Hide();
    }

    private void Refresh()
    {
        if (bossHealth == null || bossHealth.IsDead)
        {
            Hide();
            return;
        }

        if (bossNameText != null)
        {
            bossNameText.text = string.IsNullOrWhiteSpace(currentBossName) ? defaultBossName : currentBossName;
        }

        int maxHealth = Mathf.Max(1, bossHealth.MaxHealth);
        int currentHealth = Mathf.Clamp(bossHealth.CurrentHealth, 0, maxHealth);
        float healthPercent = (float)currentHealth / maxHealth;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        UpdateFillColor(healthPercent);

        if (healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    private void UpdateFillColor(float healthPercent)
    {
        if (fillImage == null)
        {
            return;
        }

        float clampedPercent = Mathf.Clamp01(healthPercent);

        if (clampedPercent > 0.5f)
        {
            float highRangePercent = Mathf.InverseLerp(0.5f, 1f, clampedPercent);
            fillImage.color = Color.Lerp(midHealthColor, highHealthColor, highRangePercent);
            return;
        }

        float lowRangePercent = Mathf.InverseLerp(0f, 0.5f, clampedPercent);
        fillImage.color = Color.Lerp(lowHealthColor, midHealthColor, lowRangePercent);
    }

    private void Show()
    {
        SetRootActive(true);
    }

    private void Hide()
    {
        SetRootActive(false);
    }

    private void SetRootActive(bool active)
    {
        if (root != null && root.activeSelf != active)
        {
            root.SetActive(active);
        }
    }
}
