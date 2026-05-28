using UnityEngine;

public class PlayerStartingStatsApplier : MonoBehaviour
{
    [Header("Upgrade Values Per Level")]
    [SerializeField] private int maxHealthPerLevel = 2;
    [SerializeField] private float moveSpeedPerLevel = 0.25f;
    [SerializeField] private float magnetRadiusPerLevel = 0.5f;
    [SerializeField] private int cannonDamagePerLevel = 1;

    [Header("References")]
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private ShipController2D shipController;
    [SerializeField] private PickupMagnetController pickupMagnetController;
    [SerializeField] private CannonShooter cannonShooter;

    private void Awake()
    {
        if (shipHealth == null) shipHealth = GetComponent<ShipHealth>();
        if (shipController == null) shipController = GetComponent<ShipController2D>();
        if (pickupMagnetController == null) pickupMagnetController = GetComponent<PickupMagnetController>();
        if (cannonShooter == null) cannonShooter = GetComponent<CannonShooter>();
    }

    private void Start()
    {
        PlayerProgression progression = PlayerProgression.Instance;

        int healthBonus = progression.GetPermanentHealthLevel() * maxHealthPerLevel;
        if (shipHealth != null && healthBonus > 0)
        {
            shipHealth.AddMaxHealth(healthBonus, true);
        }

        float speedBonus = progression.GetPermanentSpeedLevel() * moveSpeedPerLevel;
        if (shipController != null && speedBonus > 0f)
        {
            shipController.AddMoveSpeed(speedBonus);
        }

        float magnetBonus = progression.GetPermanentMagnetLevel() * magnetRadiusPerLevel;
        if (pickupMagnetController != null && magnetBonus > 0f)
        {
            pickupMagnetController.AddMagnetRadius(magnetBonus);
        }

        int cannonBonus = progression.GetPermanentCannonDamageLevel() * cannonDamagePerLevel;
        if (cannonShooter != null && cannonBonus > 0)
        {
            cannonShooter.AddCannonballDamage(cannonBonus);
        }
    }
}
