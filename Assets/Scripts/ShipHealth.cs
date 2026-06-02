using System;
using UnityEngine;

/// <summary>
/// Tracks ship hit points, health regeneration, damage, healing, and death events.
/// </summary>
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

    /// <summary>
    /// Gets the maximum health value.
    /// </summary>
    public int MaxHealth => maxHealth;
    /// <summary>
    /// Gets the current health value.
    /// </summary>
    public int CurrentHealth => currentHealth;
    /// <summary>
    /// Gets current health divided by max health.
    /// </summary>
    public float HealthPercent => maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;
    /// <summary>
    /// Gets whether this ship has died.
    /// </summary>
    public bool IsDead => isDead;

    /// <summary>
    /// Event raised by OnHealthChanged.
    /// </summary>
    public event Action<ShipHealth> OnHealthChanged;
    /// <summary>
    /// Event raised by OnDeath.
    /// </summary>
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

    /// <summary>
    /// Applies positive damage to the ship and triggers death when health reaches zero.
    /// </summary>
    /// <param name="damage">Damage amount.</param>
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

    /// <summary>
    /// Restores positive health without exceeding max health.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void Heal(int amount)
    {
        if (amount <= 0 || isDead || currentHealth >= maxHealth)
        {
            return;
        }

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        NotifyHealthChanged();
    }

    /// <summary>
    /// Enables periodic health regeneration with the provided amount and interval.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    /// <param name="interval">Seconds between ticks.</param>
    public void EnableHealthRegeneration(int amount, float interval)
    {
        healthRegenerationEnabled = true;
        regenerationAmount = Mathf.Max(1, amount);
        regenerationInterval = Mathf.Max(0.1f, interval);
        regenerationTimer = 0f;
    }

    /// <summary>
    /// Improves existing regeneration, or enables it if it is not active.
    /// </summary>
    /// <param name="amountIncrease">Additional health restored per tick.</param>
    /// <param name="intervalReduction">Seconds to subtract from the interval.</param>
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

    /// <summary>
    /// Increases maximum health and adjusts current health.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    /// <param name="healToFull">True to set current health to the new maximum.</param>
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
