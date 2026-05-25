using System;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;

    private bool isDead;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public float HealthPercent => maxHealth <= 0 ? 0f : (float)currentHealth / maxHealth;

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
        NotifyHealthChanged();

        if (currentHealth <= 0)
        {
            Die();
        }
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
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(this);
    }
}
