using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DamageFlashFeedback : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private bool autoFindRenderers = true;

    [Header("Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private int flashCount = 1;

    private Color[] originalColors;
    private Coroutine flashCoroutine;
    private int lastHealth;

    private void Awake()
    {
        ResolveReferences();
        CacheOriginalColors();

        if (shipHealth != null)
        {
            lastHealth = shipHealth.CurrentHealth;
        }
    }

    private void OnEnable()
    {
        ResolveReferences();
        CacheOriginalColors();

        if (shipHealth != null)
        {
            lastHealth = shipHealth.CurrentHealth;
            shipHealth.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDisable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnHealthChanged -= HandleHealthChanged;
        }

        StopFlashAndRestoreColors();
    }

    private void HandleHealthChanged(ShipHealth health)
    {
        int currentHealth = health.CurrentHealth;
        bool tookDamage = currentHealth < lastHealth;
        lastHealth = currentHealth;

        if (!tookDamage)
        {
            return;
        }

        PlayFlash();
    }

    private void PlayFlash()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            return;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            RestoreOriginalColors();
        }

        CacheOriginalColors();
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        int loops = Mathf.Max(1, flashCount);
        float duration = Mathf.Max(0.01f, flashDuration);

        for (int i = 0; i < loops; i++)
        {
            SetSpriteColors(flashColor);
            yield return new WaitForSeconds(duration);
            RestoreOriginalColors();

            if (i < loops - 1)
            {
                yield return new WaitForSeconds(duration);
            }
        }

        flashCoroutine = null;
    }

    private void ResolveReferences()
    {
        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
            if (shipHealth == null)
            {
                shipHealth = GetComponentInParent<ShipHealth>();
            }
        }

        if (autoFindRenderers && (spriteRenderers == null || spriteRenderers.Length == 0))
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    private void CacheOriginalColors()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            originalColors = null;
            return;
        }

        originalColors = new Color[spriteRenderers.Length];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            originalColors[i] = spriteRenderers[i] != null ? spriteRenderers[i].color : Color.white;
        }
    }

    private void SetSpriteColors(Color color)
    {
        if (spriteRenderers == null)
        {
            return;
        }

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = color;
            }
        }
    }

    private void RestoreOriginalColors()
    {
        if (spriteRenderers == null || originalColors == null)
        {
            return;
        }

        int count = Mathf.Min(spriteRenderers.Length, originalColors.Length);
        for (int i = 0; i < count; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].color = originalColors[i];
            }
        }
    }

    private void StopFlashAndRestoreColors()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }

        RestoreOriginalColors();
    }
}
