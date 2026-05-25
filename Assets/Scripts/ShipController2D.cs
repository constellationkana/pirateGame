using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController2D : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float moveSpeed = 5f;
<<<<<<< HEAD
    [SerializeField] private float rotationSpeed = 140f;

    [Header("Sprite Orientation")]
    [SerializeField] private bool useUpAsForward = true;
=======
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53

    [Header("State")]
    [SerializeField] private bool playerOnBoard;

    private Rigidbody2D rb;
<<<<<<< HEAD
    private float forwardInput;
    private float rotationInput;

    public bool PlayerOnBoard => playerOnBoard;
=======
    private Vector2 movementInput;

    public bool PlayerOnBoard => playerOnBoard;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.up;
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
<<<<<<< HEAD
           }
=======
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53

    private void Update()
    {
        if (!playerOnBoard)
        {
<<<<<<< HEAD
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
=======
            movementInput = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(x, y).normalized;

        if (movementInput.sqrMagnitude > 0.001f)
        {
            LastMoveDirection = movementInput;
        }
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
    }

    private void FixedUpdate()
    {
        if (!playerOnBoard)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

<<<<<<< HEAD
        // Rotate the sprite smoothly in 2D for arcade-like steering.
        float nextRotation = rb.rotation + (rotationInput * rotationSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(nextRotation);

        // Move in the ship's forward axis (up or right based on sprite orientation).
        Vector2 forwardAxis = useUpAsForward ? -(Vector2)transform.up : -(Vector2)transform.right;
        rb.linearVelocity = forwardAxis * (forwardInput * moveSpeed);
=======
        rb.linearVelocity = movementInput * moveSpeed;
        rb.angularVelocity = 0f;
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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
