using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private ShipHealth playerHealth;
    [SerializeField] private Slider healthSlider;

    private void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
    }

    private void Start()
    {
        UpdateHealthBar();
    }

    private void Update()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (playerHealth == null || healthSlider == null)
        {
            return;
        }

        healthSlider.value = playerHealth.CurrentHealth / playerHealth.MaxHealth;
    }
}