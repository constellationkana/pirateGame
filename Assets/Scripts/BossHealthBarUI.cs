using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject root;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text healthText;

    [Header("Text")]
    [SerializeField] private string defaultBossName = "Boss";

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

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = Mathf.Max(1, bossHealth.MaxHealth);
            healthSlider.value = Mathf.Clamp(bossHealth.CurrentHealth, 0, bossHealth.MaxHealth);
        }

        if (healthText != null)
        {
            healthText.text = $"{bossHealth.CurrentHealth}/{bossHealth.MaxHealth}";
        }
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
