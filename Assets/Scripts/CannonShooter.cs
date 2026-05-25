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

        if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            return;
        }

        if (Time.time < nextShootTime)
        {
            return;
        }

        FireCannon();
        nextShootTime = Time.time + shootCooldown;
    }

    private void FireCannon()
    {
        if (cannonballPrefab == null)
        {
            Debug.LogWarning("CannonShooter: CannonballPrefab is missing.", this);
            return;
        }

        Vector2 direction = GetCardinalDirection(shipController.LastMoveDirection);
        Transform selectedPoint = GetDirectionalCannonPoint(direction);
        Transform spawnPoint = selectedPoint != null ? selectedPoint : cannonPoint;

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
    }

    private Vector2 GetCardinalDirection(Vector2 moveDirection)
    {
        if (moveDirection.sqrMagnitude < 0.001f)
        {
            moveDirection = shootDirection;
        }

        if (moveDirection.sqrMagnitude < 0.001f)
        {
            return Vector2.up;
        }

        if (Mathf.Abs(moveDirection.x) > Mathf.Abs(moveDirection.y))
        {
            return moveDirection.x >= 0f ? Vector2.right : Vector2.left;
        }

        return moveDirection.y >= 0f ? Vector2.up : Vector2.down;
    }

    private Transform GetDirectionalCannonPoint(Vector2 direction)
    {
        if (direction == Vector2.up)
        {
            return cannonPointUp;
        }

        if (direction == Vector2.down)
        {
            return cannonPointDown;
        }

        if (direction == Vector2.left)
        {
            return cannonPointLeft;
        }

        if (direction == Vector2.right)
        {
            return cannonPointRight;
        }

        return cannonPointUp;
    }
}
