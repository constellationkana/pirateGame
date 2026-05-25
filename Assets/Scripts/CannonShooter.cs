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

    private ShipController2D shipController;
    private float nextShootTime;

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
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Time.time < nextShootTime)
        {
            return;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.up, cannonPointUp);
            return;
        }

        if (Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.down, cannonPointDown);
            return;
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.left, cannonPointLeft);
            return;
        }

        if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            TryFireDirectional(Vector2.right, cannonPointRight);
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryFireTowardMouse();
            return;
        }
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

    private Transform GetSpawnPointForDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            return direction.x >= 0f ? cannonPointRight : cannonPointLeft;
        }

        return direction.y >= 0f ? cannonPointUp : cannonPointDown;
    }
}
