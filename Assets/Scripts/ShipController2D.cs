using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController2D : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("State")]
    [SerializeField] private bool playerOnBoard;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    public bool PlayerOnBoard => playerOnBoard;
    public Vector2 LastMoveDirection { get; private set; } = Vector2.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (!playerOnBoard)
        {
            movementInput = Vector2.zero;
            return;
        }

        if (Keyboard.current == null)
        {
            movementInput = Vector2.zero;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.aKey.isPressed) x -= 1f;
        if (Keyboard.current.dKey.isPressed) x += 1f;
        if (Keyboard.current.sKey.isPressed) y -= 1f;
        if (Keyboard.current.wKey.isPressed) y += 1f;

        movementInput = new Vector2(x, y).normalized;

        if (movementInput.sqrMagnitude > 0.001f)
        {
            LastMoveDirection = movementInput;
        }
    }

    private void FixedUpdate()
    {
        if (!playerOnBoard)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            return;
        }

        rb.linearVelocity = movementInput * moveSpeed;
        rb.angularVelocity = 0f;
    }


    public void AddMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Max(0f, moveSpeed + amount);
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
