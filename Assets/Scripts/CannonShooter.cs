using UnityEngine;
using UnityEngine.InputSystem;

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
    [SerializeField] private float explosionRadiusPerPowerLevel = 0.25f;
    [SerializeField] private int explosionDamagePerPowerLevel = 1;
    [SerializeField] private LayerMask explosionDamageMask = Physics2D.DefaultRaycastLayers;
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private bool applyShipShopExplosionUnlock = true;

    private ShipController2D shipController;
    private float nextShootTime;
    private bool mouseFireQueued;
    private float baseExplosionRadius;
    private int baseExplosionDamage;

    private void Awake()
    {
        baseExplosionRadius = explosionRadius;
        baseExplosionDamage = explosionDamage;
        shipController = GetComponent<ShipController2D>();

        if (shipController == null)
        {
            Debug.LogWarning("CannonShooter: ShipController2D is missing on PlayerShip.", this);
        }
    }

    private void Start()
    {
        ApplyShipShopExplosionUnlock();
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
        cannonball.ConfigureExplosion(explosiveCannonballs, explosionRadius, explosionDamage, explosionEffectPrefab, explosionDamageMask);
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

    public void AddCannonballDamage(int amount)
    {
        cannonballDamage = Mathf.Max(0, cannonballDamage + amount);
    }

    public void AddCannonballSpeed(float amount)
    {
        cannonballSpeed = Mathf.Max(0f, cannonballSpeed + amount);
    }

    public void SetCannonballSizeMultiplier(float multiplier)
    {
        cannonballSizeMultiplier = Mathf.Max(0.1f, multiplier);
    }

    public void EnableExplosiveCannonballs(float radius, int damage)
    {
        explosiveCannonballs = true;
        explosionRadius = Mathf.Max(0f, radius);
        explosionDamage = Mathf.Max(0, damage);
    }

    private void ApplyShipShopExplosionUnlock()
    {
        if (!applyShipShopExplosionUnlock || !PlayerProgression.HasActiveSaveSlot)
        {
            return;
        }

        PlayerProgression progression = PlayerProgression.Instance;
        if (progression == null || !progression.IsUnlocked(PlayerProgression.UnlockCannonballExplosionId))
        {
            return;
        }

        int explosionPowerLevel = progression.GetExplosionPowerLevel();
        float upgradedRadius = baseExplosionRadius + explosionRadiusPerPowerLevel * explosionPowerLevel;
        int upgradedDamage = baseExplosionDamage + explosionDamagePerPowerLevel * explosionPowerLevel;
        EnableExplosiveCannonballs(upgradedRadius, upgradedDamage);
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
