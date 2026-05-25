using UnityEngine;
using UnityEngine.UI;

public class ShipHealthBar : MonoBehaviour
{
    [SerializeField] private ShipHealth targetHealth;
    [SerializeField] private Slider slider;
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        TryResolveReferences();

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += OnTargetHealthChanged;
            UpdateVisuals();
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= OnTargetHealthChanged;
        }
    }

    private void OnTargetHealthChanged(ShipHealth _)
    {
        UpdateVisuals();
    }

    private void TryResolveReferences()
    {
        if (targetHealth == null)
        {
            targetHealth = GetComponentInParent<ShipHealth>();
        }

        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }
    }

    public void SetTarget(ShipHealth health)
    {
        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged -= OnTargetHealthChanged;
        }

        targetHealth = health;

        if (targetHealth != null)
        {
            targetHealth.OnHealthChanged += OnTargetHealthChanged;
        }

        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (targetHealth == null)
        {
            return;
        }

        float healthPercent = targetHealth.HealthPercent;

        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = healthPercent;
        }

        if (fillImage != null)
        {
            fillImage.fillAmount = healthPercent;
        }
    }
}
