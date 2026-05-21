using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController2D : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 140f;

    [Header("Sprite Orientation")]
    [SerializeField] private bool useUpAsForward = true;

    [Header("State")]
    [SerializeField] private bool playerOnBoard;

    private Rigidbody2D rb;
    private float forwardInput;
    private float rotationInput;

    public bool PlayerOnBoard => playerOnBoard;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        if (!playerOnBoard)
        {
            forwardInput = 0f;
            rotationInput = 0f;
            return;
        }

        // W/S move the ship forward/back based on facing direction.
        forwardInput = 0f;
        if (Input.GetKey(KeyCode.W)) forwardInput += 1f;
        if (Input.GetKey(KeyCode.S)) forwardInput -= 1f;

        // A/D rotate left/right.
        rotationInput = 0f;
        if (Input.GetKey(KeyCode.A)) rotationInput += 1f;
        if (Input.GetKey(KeyCode.D)) rotationInput -= 1f;
    }

    private void FixedUpdate()
    {
        if (!playerOnBoard)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        // Rotate the sprite smoothly in 2D for arcade-like steering.
        float nextRotation = rb.rotation + (rotationInput * rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(nextRotation);

        // Move in the ship's forward axis (up or right based on sprite orientation).
        Vector2 forwardAxis = useUpAsForward ? (Vector2)transform.up : (Vector2)transform.right;
        rb.linearVelocity = forwardAxis * (forwardInput * moveSpeed);
    }

    public void SetPlayerOnBoard(bool value)
    {
        playerOnBoard = value;

        if (!playerOnBoard)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
