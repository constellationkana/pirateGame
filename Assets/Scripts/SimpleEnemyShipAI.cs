using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleEnemyShipAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetShip;
    [SerializeField] private ShipController2D playerShipController;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stoppingDistance = 3f;
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Status Effects")]
    [SerializeField, Range(0.05f, 1f)] private float slowSpeedMultiplier = 0.5f;

    private Rigidbody2D rb;
    private float slowTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.None;

        if (targetShip == null)
        {
            Debug.LogWarning("SimpleEnemyShipAI: Target Ship reference is missing.", this);
        }

        if (playerShipController == null)
        {
            Debug.LogWarning("SimpleEnemyShipAI: Player Ship Controller reference is missing.", this);
        }
    }

    public void Initialize(Transform newTargetShip, ShipController2D newPlayerShipController)
    {
        targetShip = newTargetShip;
        playerShipController = newPlayerShipController;
    }

    public void ApplySlow(float duration)
    {
        if (duration <= 0f)
        {
            return;
        }

        slowTimer = Mathf.Max(slowTimer, duration);
    }

    private void Update()
    {
        if (slowTimer > 0f)
        {
            slowTimer = Mathf.Max(0f, slowTimer - Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (targetShip == null || playerShipController == null)
        {
            StopMovement();
            return;
        }

        if (!playerShipController.PlayerOnBoard)
        {
            StopMovement();
            return;
        }

        Vector2 toTarget = targetShip.position - transform.position;
        RotateTowardsTarget(toTarget);

        if (toTarget.magnitude <= stoppingDistance)
        {
            StopMovement();
            return;
        }

        float currentMoveSpeed = slowTimer > 0f ? moveSpeed * slowSpeedMultiplier : moveSpeed;
        rb.linearVelocity = toTarget.normalized * currentMoveSpeed;
        rb.angularVelocity = 0f;
    }

    private void RotateTowardsTarget(Vector2 toTarget)
    {
        if (toTarget.sqrMagnitude < 0.001f)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg - 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
}
