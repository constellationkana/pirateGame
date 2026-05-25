using UnityEngine;
using UnityEngine.InputSystem;

public class CannonShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cannonPoint;
    [SerializeField] private GameObject cannonballPrefab;

    [Header("Shooting")]
    [SerializeField] private float shootCooldown = 0.4f;
<<<<<<< HEAD
=======
    [SerializeField] private Vector2 shootDirection = Vector2.up;
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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
        if (cannonPoint == null || cannonballPrefab == null)
        {
            Debug.LogWarning("CannonShooter: CannonPoint or CannonballPrefab is missing.", this);
            return;
        }

<<<<<<< HEAD
        Camera currentCamera = Camera.main;
        if (currentCamera == null)
        {
            Debug.LogWarning("CannonShooter: No Camera tagged MainCamera found.", this);
            return;
        }

        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = currentCamera.ScreenToWorldPoint(mouseScreen);
        mouseWorld.z = cannonPoint.position.z;

        // Direction from cannon muzzle to mouse position.
        Vector2 direction = (mouseWorld - cannonPoint.position);
        if (direction.sqrMagnitude < 0.0001f)
        {
            direction = (Vector2)transform.up;
        }

=======
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
        GameObject cannonballObject = Instantiate(cannonballPrefab, cannonPoint.position, Quaternion.identity);
        Cannonball cannonball = cannonballObject.GetComponent<Cannonball>();

        if (cannonball == null)
        {
            Debug.LogWarning("CannonShooter: Spawned prefab does not have Cannonball script.", this);
            Destroy(cannonballObject);
            return;
        }

<<<<<<< HEAD
        cannonball.Initialize(direction.normalized, cannonballSpeed, gameObject);
=======
        Vector2 direction = shipController.LastMoveDirection;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = shootDirection;
        }

        direction = direction.normalized;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.up;
        }

        cannonball.Initialize(direction, cannonballSpeed, gameObject);
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
    }
}
