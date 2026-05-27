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
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();
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

        rb.linearVelocity = toTarget.normalized * moveSpeed;
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

    private void ResolveReferences()
    {
        if (targetShip == null)
        {
            GameObject taggedShip = GameObject.FindWithTag("PlayerShip");
            if (taggedShip != null)
            {
                targetShip = taggedShip.transform;
            }
            else
            {
                GameObject namedShip = GameObject.Find("PlayerShip");
                if (namedShip != null)
                {
                    targetShip = namedShip.transform;
                }
            }
        }

        if (playerShipController == null && targetShip != null)
        {
            playerShipController = targetShip.GetComponent<ShipController2D>();
        }

        if (targetShip == null)
        {
            Debug.LogWarning("SimpleEnemyShipAI: Could not find PlayerShip target. Assign Target Ship or tag PlayerShip.", this);
        }

        if (playerShipController == null)
        {
            Debug.LogWarning("SimpleEnemyShipAI: Could not find ShipController2D on PlayerShip.", this);
        }
    }
}
