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
}
