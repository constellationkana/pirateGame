using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates the player health bar UI from a ShipHealth component.
/// </summary>
public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;

    private void Awake()
    {
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
    }

    private void OnEnable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnHealthChanged += HandleHealthChanged;
        }

        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(ShipHealth _)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (shipHealth == null)
        {
            return;
        }

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = shipHealth.MaxHealth;
            healthSlider.value = shipHealth.CurrentHealth;
        }

        if (healthText != null)
        {
            healthText.text = $"{shipHealth.CurrentHealth}/{shipHealth.MaxHealth}";
        }
    }
}
