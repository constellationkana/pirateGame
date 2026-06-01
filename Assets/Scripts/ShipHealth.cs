using System;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;

    [Header("Death Behavior")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private bool disableOnDeath;

    [Header("Regeneration")]
    [SerializeField] private bool healthRegenerationEnabled;
    [SerializeField] private int regenerationAmount = 1;
    [SerializeField] private float regenerationInterval = 5f;

    private bool isDead;
    private float regenerationTimer;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;
    public bool IsDead => isDead;

    public event Action<ShipHealth> OnHealthChanged;
    public event Action<ShipHealth> OnDeath;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = Mathf.Clamp(currentHealth <= 0 ? maxHealth : currentHealth, 0, maxHealth);
    }

    private void Start()
    {
        NotifyHealthChanged();
    }

    private void Update()
    {
        TickHealthRegeneration();
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || isDead)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damage);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        NotifyHealthChanged();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || isDead || currentHealth >= maxHealth)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        NotifyHealthChanged();
    }

    public void EnableHealthRegeneration(int amount, float interval)
    {
        healthRegenerationEnabled = true;
        regenerationAmount = Mathf.Max(1, amount);
        regenerationInterval = Mathf.Max(0.1f, interval);
        regenerationTimer = 0f;
    }

    public void ImproveHealthRegeneration(int amountIncrease, float intervalReduction)
    {
        if (!healthRegenerationEnabled)
        {
            EnableHealthRegeneration(Mathf.Max(1, amountIncrease), regenerationInterval);
            return;
        }

        regenerationAmount = Mathf.Max(1, regenerationAmount + amountIncrease);
        regenerationInterval = Mathf.Max(0.1f, regenerationInterval - intervalReduction);
    }

    private void TickHealthRegeneration()
    {
        if (!healthRegenerationEnabled || isDead || currentHealth >= maxHealth)
        {
            regenerationTimer = 0f;
            return;
        }

        regenerationTimer += Time.deltaTime;
        if (regenerationTimer < regenerationInterval)
        {
            return;
        }

        regenerationTimer = 0f;
        Heal(regenerationAmount);
    }

    public void AddMaxHealth(int amount, bool healToFull)
    {
        if (amount <= 0 || isDead)
        {
            return;
        }

        maxHealth = Mathf.Max(1, maxHealth + amount);

        if (healToFull)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        }

        NotifyHealthChanged();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentHealth = 0;

        NotifyHealthChanged();
        OnDeath?.Invoke(this);

        if (destroyOnDeath)
        {
            Destroy(gameObject);
        }
        else if (disableOnDeath)
        {
            gameObject.SetActive(false);
        }
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(this);
    }
}
