using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController2D : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 540f;
    [SerializeField] private float thrust = 6f;
    [SerializeField] private float turnSpeed = 130f;
    [SerializeField] private float maxSpeed = 5f;

    [Header("State")]
    [SerializeField] private bool playerOnBoard;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    public bool PlayerOnBoard => playerOnBoard;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.None;
    }

    private void Update()
    {
        if (!playerOnBoard)
        {
            movementInput = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        if (!playerOnBoard)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = movementInput * moveSpeed;

        if (movementInput.sqrMagnitude > 0.001f)
        {
            float targetAngle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg - 90f;
            float newAngle = Mathf.MoveTowardsAngle(rb.rotation, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(newAngle);
        }
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (!playerOnBoard)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.05f);
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, 0.1f);
            return;
        }

        float forwardInput = Input.GetAxis("Vertical");
        float turnInput = -Input.GetAxis("Horizontal");

        Vector2 force = (Vector2)transform.up * (forwardInput * thrust);
        rb.AddForce(force);

        float targetAngularVelocity = turnInput * turnSpeed;
        rb.angularVelocity = targetAngularVelocity;

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    public void SetPlayerOnBoard(bool value)
    {
        playerOnBoard = value;
    }
}
