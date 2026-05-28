using UnityEngine;

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

    private bool dashUnlocked;
    private bool isDashing;
    private float dashTimeRemaining;
    private float cooldownRemaining;
    private Vector2 dashDirection = Vector2.up;

    public bool DashUnlocked => dashUnlocked;

    private void Awake()
    {
        if (shipController == null) shipController = GetComponent<ShipController2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (cooldownRemaining > 0f)
        {
            cooldownRemaining -= Time.deltaTime;
        }

        if (!dashUnlocked || isDashing || !Input.GetKeyDown(dashKey) || cooldownRemaining > 0f)
        {
            return;
        }

        if (requirePlayerOnBoard && shipController != null && !shipController.PlayerOnBoard)
        {
            return;
        }

        StartDash();
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
        }
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

    public void UnlockDash()
    {
        dashUnlocked = true;
    }

    public void AddDashSpeed(float amount)
    {
        dashSpeed = Mathf.Max(0f, dashSpeed + amount);
    }

    public void ReduceDashCooldown(float amount)
    {
        dashCooldown = Mathf.Max(0.1f, dashCooldown - amount);
    }

    public void AddDashDuration(float amount)
    {
        dashDuration = Mathf.Max(0.05f, dashDuration + amount);
    }
}
