using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player ship movement, boarding state, movement direction, and optional unboarding.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ShipController2D : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("State")]
    [SerializeField] private bool playerOnBoard;
    [SerializeField] private bool allowUnboarding = true;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private ShipDashController dashController;

    /// <summary>
    /// Gets whether the player is currently controlling the ship.
    /// </summary>
    public bool PlayerOnBoard => playerOnBoard;
    /// <summary>
    /// Gets whether the current stage allows the player to leave the ship.
    /// </summary>
    public bool AllowUnboarding => allowUnboarding;
    /// <summary>
    /// Gets the most recent non-zero movement direction used by the ship.
    /// </summary>
    public Vector2 LastMoveDirection { get; private set; } = Vector2.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        dashController = GetComponent<ShipDashController>();
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

        if (dashController != null && dashController.IsDashing)
        {
            rb.angularVelocity = 0f;
            return;
        }

        rb.linearVelocity = movementInput * moveSpeed;
        rb.angularVelocity = 0f;
    }

    /// <summary>
    /// Gets the ship forward direction based on movement or transform orientation.
    /// </summary>
    /// <returns>The requested direction vector.</returns>
    public Vector2 GetForwardDirection()
    {
        Vector2 fromMove = LastMoveDirection.sqrMagnitude > 0.001f ? LastMoveDirection : Vector2.zero;
        if (fromMove != Vector2.zero)
        {
            return fromMove.normalized;
        }

        return ((Vector2)transform.up).normalized;
    }

    /// <summary>
    /// Adds to the ship movement speed without allowing it below zero.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void AddMoveSpeed(float amount)
    {
        moveSpeed = Mathf.Max(0f, moveSpeed + amount);
    }

    /// <summary>
    /// Forces the ship into the boarded/player-controlled state.
    /// </summary>
    public void ForceBoardPlayer()
    {
        SetPlayerOnBoard(true);
    }

    /// <summary>
    /// Forces the ship out of the boarded/player-controlled state.
    /// </summary>
    public void ForceUnboardPlayer()
    {
        SetPlayerOnBoard(false);
    }

    /// <summary>
    /// Sets whether unboarding is currently allowed.
    /// </summary>
    /// <param name="allow">True to allow the option; false to prevent it.</param>
    public void SetAllowUnboarding(bool allow)
    {
        allowUnboarding = allow;
    }

    /// <summary>
    /// Sets whether the player is considered onboard and controlling the ship.
    /// </summary>
    /// <param name="value">New state value.</param>
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
