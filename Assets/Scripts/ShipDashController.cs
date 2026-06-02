using UnityEngine;

/// <summary>
/// Handles the ship dash ability, cooldown, duration, and unlock state.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class ShipDashController : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;
    [SerializeField] private bool requirePlayerOnBoard = true;
    [SerializeField] private ShipController2D shipController;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private bool logDash = false;

    [Header("Runtime Debug")]
    [SerializeField] private bool dashUnlocked;
    [SerializeField] private bool isDashing;
    [SerializeField] private float cooldownRemaining;

    private float dashTimeRemaining;
    private Vector2 dashDirection = Vector2.up;

    /// <summary>
    /// Gets whether dash is unlocked for this ship.
    /// </summary>
    public bool DashUnlocked => dashUnlocked;
    /// <summary>
    /// Checks whether dashing.
    /// </summary>
    public bool IsDashing => isDashing;

    private void Awake()
    {
        if (shipController == null) shipController = GetComponent<ShipController2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining = Mathf.Max(0f, cooldownRemaining - Time.deltaTime);
        }

        if (!Input.GetKeyDown(dashKey))
        {
            return;
        }

        if (logDash)
        {
            Debug.Log("ShipDashController: Dash key pressed.", this);
        }

        TryStartDash();
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            return;
        }

        rb.linearVelocity = dashDirection * dashSpeed;
        dashTimeRemaining -= Time.fixedDeltaTime;

        if (dashTimeRemaining <= 0f)
        {
            isDashing = false;

            if (logDash)
            {
                Debug.Log("ShipDashController: Dash ended.", this);
            }
        }
    }

    private void TryStartDash()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (!dashUnlocked)
        {
            if (logDash) Debug.Log("ShipDashController: Dash blocked (locked).", this);
            return;
        }

        if (isDashing)
        {
            return;
        }

        if (cooldownRemaining > 0f)
        {
            if (logDash) Debug.Log($"ShipDashController: Dash blocked (cooldown {cooldownRemaining:F2}s).", this);
            return;
        }

        if (requirePlayerOnBoard && shipController != null && !shipController.PlayerOnBoard)
        {
            if (logDash) Debug.Log("ShipDashController: Dash blocked (player not on board).", this);
            return;
        }

        StartDash();
    }

    private void StartDash()
    {
        dashDirection = shipController != null ? shipController.GetForwardDirection() : (Vector2)transform.up;
        if (dashDirection.sqrMagnitude < 0.001f)
        {
            dashDirection = (Vector2)transform.up;
        }

        isDashing = true;
        dashTimeRemaining = dashDuration;
        cooldownRemaining = dashCooldown;

        if (logDash)
        {
            Debug.Log($"ShipDashController: Dash started. dir={dashDirection} speed={dashSpeed}", this);
        }
    }

    /// <summary>
    /// Unlocks dash for this ship.
    /// </summary>
    public void UnlockDash()
    {
        dashUnlocked = true;
    }

    /// <summary>
    /// Adds to the dash speed value.
    /// </summary>
    /// <param name="amount">Amount to add or subtract.</param>
    public void AddDashSpeed(float amount)
    {
        dashSpeed = Mathf.Max(0f, dashSpeed + amount);
    }

    /// <summary>
    /// Reduces the dash cooldown value.
    /// </summary>
    /// <param name="amount">Amount to add or subtract.</param>
    public void ReduceDashCooldown(float amount)
    {
        dashCooldown = Mathf.Max(0.1f, dashCooldown - amount);
    }

    /// <summary>
    /// Adds to the dash duration value.
    /// </summary>
    /// <param name="amount">Amount to add or subtract.</param>
    public void AddDashDuration(float amount)
    {
        dashDuration = Mathf.Max(0.05f, dashDuration + amount);
    }
}
