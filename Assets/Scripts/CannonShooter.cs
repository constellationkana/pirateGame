using UnityEngine;
using UnityEngine.InputSystem;

public class CannonShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform cannonPoint;
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
        if (cannonPoint == null || cannonballPrefab == null)
        {
            Debug.LogWarning("CannonShooter: CannonPoint or CannonballPrefab is missing.", this);
            return;
        }

        GameObject cannonballObject = Instantiate(cannonballPrefab, cannonPoint.position, Quaternion.identity);
        Cannonball cannonball = cannonballObject.GetComponent<Cannonball>();

        if (cannonball == null)
        {
            Debug.LogWarning("CannonShooter: Spawned prefab does not have Cannonball script.", this);
            Destroy(cannonballObject);
            return;
        }

        Vector2 direction = shootDirection.normalized;
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = Vector2.up;
        }

        cannonball.Initialize(direction, cannonballSpeed, gameObject);
    }
}
