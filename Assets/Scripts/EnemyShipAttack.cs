using UnityEngine;

public class EnemyShipAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform targetShip;
    [SerializeField] private ShipController2D playerShipController;

    [Header("Attack Rules")]
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private bool useCannonAttack = true;
    [SerializeField] private bool useFruitAttack = true;
    [SerializeField] private bool logAttacks = false;

    [Header("Cannon Attack")]
    [SerializeField] private GameObject cannonballPrefab;
    [SerializeField] private Transform cannonFirePoint;
    [SerializeField] private Transform[] cannonFirePoints;
    [SerializeField] private int cannonDamage = 2;
    [SerializeField] private float cannonballSpeed = 10f;
    [SerializeField] private float cannonShootInterval = 3f;

    [Header("Fruit Attack")]
    [SerializeField] private GameObject fruitProjectilePrefab;
    [SerializeField] private Transform fruitFirePoint;
    [SerializeField] private Transform[] fruitFirePoints;
    [SerializeField] private int fruitDamage = 1;
    [SerializeField] private float fruitSpeed = 7f;
    [SerializeField] private float fruitThrowInterval = 1f;

    private float nextCannonTime;
    private float nextFruitTime;

    private void Awake()
    {
        ConfigureTargeting(targetShip, playerShipController);
    }

    public void ConfigureTargeting(Transform runtimeTargetShip, ShipController2D runtimePlayerShipController)
    {
        targetShip = runtimeTargetShip;

        if (runtimePlayerShipController != null)
        {
            playerShipController = runtimePlayerShipController;
        }
        else if (targetShip != null)
        {
            playerShipController = targetShip.GetComponent<ShipController2D>();
        }

        if (targetShip == null)
        {
            Debug.LogWarning("EnemyShipAttack: TargetShip reference is missing. Enemy cannot fire at player.", this);
        }

        if (playerShipController == null)
        {
            Debug.LogWarning("EnemyShipAttack: PlayerShipController reference is missing. Enemy attacks disabled until assigned.", this);
        }
    }

    private void Update()
    {
        if (targetShip == null || playerShipController == null)
        {
            return;
        }

        if (!playerShipController.PlayerOnBoard)
        {
            return;
        }

        Vector2 toTarget = targetShip.position - transform.position;
        if (toTarget.magnitude > attackRange)
        {
            return;
        }

        if (useCannonAttack && Time.time >= nextCannonTime)
        {
            FireProjectile(cannonballPrefab, GetFirePoint(cannonFirePoints, cannonFirePoint), cannonballSpeed, cannonDamage, "cannonball");
            nextCannonTime = Time.time + Mathf.Max(0.1f, cannonShootInterval);
        }

        if (useFruitAttack && Time.time >= nextFruitTime)
        {
            FireProjectile(fruitProjectilePrefab, GetFirePoint(fruitFirePoints, fruitFirePoint), fruitSpeed, fruitDamage, "fruit");
            nextFruitTime = Time.time + Mathf.Max(0.1f, fruitThrowInterval);
        }
    }

    private void FireProjectile(GameObject projectilePrefab, Transform firePoint, float speed, int fallbackDamage, string attackName)
    {
        if (projectilePrefab == null)
        {
            if (logAttacks) Debug.LogWarning($"EnemyShipAttack: {attackName} prefab not assigned.", this);
            return;
        }

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Cannonball projectile = projectileObject.GetComponent<Cannonball>();
        if (projectile == null)
        {
            Debug.LogWarning($"EnemyShipAttack: {attackName} prefab is missing Cannonball component.", this);
            Destroy(projectileObject);
            return;
        }

        projectile.SetDamage(fallbackDamage);

        Vector2 direction = (targetShip.position - spawnPos);
        if (direction.sqrMagnitude < 0.001f)
        {
            direction = transform.up;
        }

        projectile.Initialize(direction.normalized, speed, gameObject);

        if (logAttacks)
        {
            Debug.Log($"EnemyShipAttack: Fired {attackName} toward player.", this);
        }
    }

    private Transform GetFirePoint(Transform[] firePoints, Transform singleFirePoint)
    {
        if (firePoints != null && firePoints.Length > 0)
        {
            int index = Random.Range(0, firePoints.Length);
            return firePoints[index];
        }

        return singleFirePoint;
    }
}
