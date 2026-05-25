using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
