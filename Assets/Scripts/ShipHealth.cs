using System.Collections;
using UnityEngine;

public class ShipHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int currentHealth;

    [Header("Death")]
    [Tooltip("If true, this GameObject is destroyed when health reaches 0.")]
    [SerializeField] private bool destroyOnDeath = true;

    [Tooltip("Optional object to disable on death when destroyOnDeath is false. If empty, this GameObject is disabled.")]
    [SerializeField] private GameObject objectToDisableOnDeath;

    [Header("Hit Feedback")]
    [Tooltip("Optional sprite renderer to flash when damage is taken.")]
    [SerializeField] private SpriteRenderer flashRenderer;

    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private bool logDamage = true;

    private Color originalFlashColor = Color.white;
    private Coroutine flashRoutine;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        currentHealth = maxHealth;

        if (flashRenderer == null)
        {
            flashRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (flashRenderer != null)
        {
            originalFlashColor = flashRenderer.color;
        }
    }

    /// <summary>
    /// Call this from projectiles or hazards to damage this ship.
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (logDamage)
        {
            Debug.Log($"{name} took {amount} damage. Health: {currentHealth}/{maxHealth}", this);
        }

        PlayHitFeedback();

        if (currentHealth == 0)
        {
            Die();
        }
    }

    private void PlayHitFeedback()
    {
        if (flashRenderer == null)
        {
            return;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        flashRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);

        if (flashRenderer != null)
        {
            flashRenderer.color = originalFlashColor;
        }

        flashRoutine = null;
    }

    private void Die()
    {
        Debug.Log($"{name} was destroyed.", this);

        if (destroyOnDeath)
        {
            Destroy(gameObject);
            return;
        }

        GameObject target = objectToDisableOnDeath != null ? objectToDisableOnDeath : gameObject;
        target.SetActive(false);
    }
}
