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

    private Rigidbody2D rb;

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

    private void FixedUpdate()
    {
        if (targetShip == null || playerShipController == null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        if (!playerShipController.PlayerOnBoard)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        Vector2 toTarget = targetShip.position - transform.position;
        float distance = toTarget.magnitude;

        RotateTowardsTarget(toTarget);

        if (distance <= stoppingDistance)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        Vector2 moveDirection = toTarget.normalized;
        rb.linearVelocity = moveDirection * moveSpeed;
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
}
