using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Fires cannonball prefabs from directional spawn points using keyboard or mouse input.
/// </summary>
public class CannonShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cannonPoint;
    [SerializeField] private Transform cannonPointUp;
    [SerializeField] private Transform cannonPointDown;
    [SerializeField] private Transform cannonPointLeft;
    [SerializeField] private Transform cannonPointRight;
    [SerializeField] private GameObject cannonballPrefab;

    [Header("Shooting")]
    [SerializeField] private float shootCooldown = 0.4f;
    [SerializeField] private Vector2 shootDirection = Vector2.up;
    [SerializeField] private float cannonballSpeed = 12f;
    [SerializeField] private int cannonballDamage = 1;

    [Header("Projectile Upgrades")]
    [SerializeField] private float cannonballSizeMultiplier = 1f;
    [SerializeField] private bool explosiveCannonballs;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int explosionDamage = 1;
    [SerializeField] private LayerMask explosionDamageMask = Physics2D.DefaultRaycastLayers;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private float explosionEffectLifetime = 0.5f;
    [SerializeField] private int cannonballPierceCount;

    private ShipController2D shipController;
    private float nextShootTime;
    private bool mouseFireQueued;

    /// <summary>
    /// Gets the cannonball prefab currently used for shots.
    /// </summary>
    public GameObject CannonballPrefab => cannonballPrefab;

    private void Awake()
    {
        shipController = GetComponent<ShipController2D>();

        if (shipController == null)
        {
            Debug.LogWarning("CannonShooter: ShipController2D is missing on PlayerShip.", this);
        }
    }

    private void Update()
    {
        if (shipController == null || !shipController.PlayerOnBoard)
        {
            mouseFireQueued = false;
            return;
        }

        if (WasLeftMousePressedThisFrame())
        {
            mouseFireQueued = true;
        }

        if (Time.time < nextShootTime)
        {
            return;
        }

        if (mouseFireQueued || IsLeftMouseHeld())
        {
            mouseFireQueued = false;
            TryFireTowardMouse();
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.upArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.up, cannonPointUp);
            return;
        }

        if (keyboard.downArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.down, cannonPointDown);
            return;
        }

        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.left, cannonPointLeft);
            return;
        }

        if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.right, cannonPointRight);
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            TryFireTowardMouse();
            return;
        }
    }

    private static bool WasLeftMousePressedThisFrame()
    {
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.wasPressedThisFrame;
    }

    private static bool IsLeftMouseHeld()
    {
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.isPressed;
    }

    private void TryFireDirectional(Vector2 direction, Transform directionalPoint)
    {
        if (cannonballPrefab == null)
        {
            Debug.LogWarning("CannonShooter: CannonballPrefab is missing.", this);
            return;
        }

        Transform spawnPoint = directionalPoint != null ? directionalPoint : cannonPoint;

        if (spawnPoint == null)
        {
            Debug.LogWarning("CannonShooter: No valid cannon spawn point assigned.", this);
            return;
        }

        GameObject cannonballObject = Instantiate(cannonballPrefab, spawnPoint.position, Quaternion.identity);
        Cannonball cannonball = cannonballObject.GetComponent<Cannonball>();

        if (cannonball == null)
        {
            Debug.LogWarning("CannonShooter: Spawned prefab does not have Cannonball script.", this);
            Destroy(cannonballObject);
            return;
        }

        cannonball.SetDamage(cannonballDamage);
        cannonball.SetFiredByPlayer(true);
        cannonball.SetSizeMultiplier(cannonballSizeMultiplier);
        cannonball.SetExplosion(explosiveCannonballs, explosionRadius, explosionDamage);
        cannonball.SetExplosionEffect(explosionEffectPrefab, explosionEffectLifetime);
        cannonball.SetExplosionDamageMask(explosionDamageMask);
        cannonball.SetPierceCount(cannonballPierceCount);
        cannonball.Initialize(direction, cannonballSpeed, gameObject);
        nextShootTime = Time.time + shootCooldown;
    }

    private void TryFireTowardMouse()
    {
        if (Mouse.current == null)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("CannonShooter: Main Camera not found for mouse aiming.", this);
            return;
        }

        Vector3 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(mouseScreen);
        Vector2 direction = mouseWorld - transform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            direction = shootDirection.sqrMagnitude > 0.001f ? shootDirection : Vector2.up;
        }

        Transform spawnPoint = GetSpawnPointForDirection(direction);
        TryFireDirectional(direction.normalized, spawnPoint);
    }

    /// <summary>
    /// Adds to the configured cannonball damage.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void AddCannonballDamage(int amount)
    {
        cannonballDamage = Mathf.Max(0, cannonballDamage + amount);
    }

    /// <summary>
    /// Adds to the configured cannonball travel speed.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void AddCannonballSpeed(float amount)
    {
        cannonballSpeed = Mathf.Max(0f, cannonballSpeed + amount);
    }

    /// <summary>
    /// Sets the size multiplier applied to spawned cannonballs.
    /// </summary>
    /// <param name="multiplier">Size multiplier.</param>
    public void SetCannonballSizeMultiplier(float multiplier)
    {
        cannonballSizeMultiplier = Mathf.Max(0.1f, multiplier);
    }

    /// <summary>
    /// Adds to the cannonball size multiplier.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void AddCannonballSizeMultiplier(float amount)
    {
        cannonballSizeMultiplier = Mathf.Max(0.1f, cannonballSizeMultiplier + amount);
    }

    /// <summary>
    /// Reduces the delay between shots while preserving the minimum cooldown.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void ReduceShootCooldown(float amount)
    {
        shootCooldown = Mathf.Max(0.05f, shootCooldown - amount);
    }

    /// <summary>
    /// Sets how many additional targets spawned cannonballs may pierce.
    /// </summary>
    /// <param name="pierceCount">Parameter used by this method.</param>
    public void SetCannonballPierceCount(int pierceCount)
    {
        cannonballPierceCount = Mathf.Max(0, pierceCount);
    }

    /// <summary>
    /// Adds to the configured cannonball pierce count.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void AddCannonballPierce(int amount)
    {
        cannonballPierceCount = Mathf.Max(0, cannonballPierceCount + amount);
    }

    /// <summary>
    /// Enables explosive cannonball settings for future shots.
    /// </summary>
    /// <param name="radius">Radius value.</param>
    /// <param name="damage">Damage amount.</param>
    public void EnableExplosiveCannonballs(float radius, int damage)
    {
        explosiveCannonballs = true;
        explosionRadius = Mathf.Max(0f, radius);
        explosionDamage = Mathf.Max(0, damage);
    }

    private Transform GetSpawnPointForDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x >= 0f ? cannonPointRight : cannonPointLeft;
        }

        return direction.y >= 0f ? cannonPointUp : cannonPointDown;
    }
}
