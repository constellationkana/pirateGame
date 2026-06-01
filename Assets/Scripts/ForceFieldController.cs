using UnityEngine;

public class ForceFieldController : MonoBehaviour
{
    [Header("Force Field")]
    [SerializeField] private bool forceFieldUnlocked = false;
    [SerializeField] private float radius = 3f;
    [SerializeField] private int damagePerTick = 1;
    [SerializeField] private float tickInterval = 1f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private bool showDebugRadius = true;
    [SerializeField] private bool logDamage = false;

    [Header("Optional Visual")]
    [SerializeField] private GameObject forceFieldVisual;
    [SerializeField] private Transform ringVisual;

    private float tickTimer;
    private ShipHealth selfHealth;

    public bool ForceFieldUnlocked => forceFieldUnlocked;

    private void Awake()
    {
        selfHealth = GetComponent<ShipHealth>();
        forceFieldUnlocked = false;
        SetVisualActive(false);
        UpdateVisualScale();
    }

    private void Update()
    {
        UpdateVisualScale();

        if (!forceFieldUnlocked)
        {
            return;
        }

        tickTimer += Time.deltaTime;
        if (tickTimer < tickInterval)
        {
            return;
        }

        tickTimer = 0f;
        DamageEnemiesInRange();
    }

    private void DamageEnemiesInRange()
    {
        Collider2D[] hits = enemyLayer.value != 0
            ? Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer)
            : Physics2D.OverlapCircleAll(transform.position, radius);

        for (int i = 0; i < hits.Length; i++)
        {
            ShipHealth health = hits[i].GetComponentInParent<ShipHealth>();
            if (health == null || health == selfHealth)
            {
                continue;
            }

            health.TakeDamage(damagePerTick);

            if (logDamage)
            {
                Debug.Log($"ForceFieldController: Damaged {health.name} for {damagePerTick}", health);
            }
        }
    }

    private void SetVisualActive(bool active)
    {
        GameObject visual = GetVisualGameObject();
        if (visual != null)
        {
            visual.SetActive(active);
        }
    }

    private GameObject GetVisualGameObject()
    {
        if (forceFieldVisual != null)
        {
            return forceFieldVisual;
        }

        return ringVisual != null ? ringVisual.gameObject : null;
    }

    private Transform GetVisualTransform()
    {
        if (forceFieldVisual != null)
        {
            return forceFieldVisual.transform;
        }

        return ringVisual;
    }

    private void UpdateVisualScale()
    {
        Transform visualTransform = GetVisualTransform();
        if (visualTransform == null)
        {
            return;
        }

        float diameter = Mathf.Max(0.1f, radius * 2f);
        visualTransform.localScale = new Vector3(diameter, diameter, 1f);
    }

    public void UnlockForceField()
    {
        forceFieldUnlocked = true;
        tickTimer = 0f;
        SetVisualActive(true);
        UpdateVisualScale();
    }

    public void AddRadius(float amount)
    {
        radius = Mathf.Max(0.1f, radius + amount);
        UpdateVisualScale();
    }

    public void AddDamage(int amount)
    {
        damagePerTick = Mathf.Max(1, damagePerTick + amount);
    }

    public void ReduceTickInterval(float amount)
    {
        tickInterval = Mathf.Max(0.1f, tickInterval - amount);
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRadius)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
