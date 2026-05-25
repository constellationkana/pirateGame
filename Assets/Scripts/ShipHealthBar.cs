using UnityEngine;
using UnityEngine.UI;

public class ShipHealthBar : MonoBehaviour
{
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage;

    private void Awake()
    {
        if (shipHealth == null)
        {
            shipHealth = GetComponentInParent<ShipHealth>();
        }
    }

    private void OnEnable()
    {
        if (shipHealth == null)
        {
            return;
        }

        shipHealth.OnHealthChanged += HandleHealthChanged;
        HandleHealthChanged(shipHealth);
    }

    private void OnDisable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(ShipHealth health)
    {
        float value = Mathf.Clamp01(health.HealthPercent);

        if (healthSlider != null)
        {
            healthSlider.normalizedValue = value;
        }

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = value;
        }
    }
}
