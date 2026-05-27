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

    private bool isDead;

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
