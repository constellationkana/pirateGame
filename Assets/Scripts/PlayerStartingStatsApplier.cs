using UnityEngine;

public class PlayerStartingStatsApplier : MonoBehaviour
{
    [Header("Upgrade Values Per Level")]
    [SerializeField] private int maxHealthPerLevel = 2;
    [SerializeField] private float moveSpeedPerLevel = 0.25f;
    [SerializeField] private float magnetRadiusPerLevel = 0.5f;
    [SerializeField] private int cannonDamagePerLevel = 1;
    [SerializeField] private float cannonballSpeedPerLevel = 1f;
    [SerializeField] private float cannonballSizeMultiplierOnUnlock = 1.5f;
    [SerializeField] private int healthRegenerationAmount = 1;
    [SerializeField] private float healthRegenerationInterval = 5f;
    [SerializeField] private float baseExplosionRadius = 2f;
    [SerializeField] private float explosionRadiusPerPowerLevel = 0.25f;
    [SerializeField] private int baseExplosionDamage = 1;
    [SerializeField] private int explosionDamagePerPowerLevel = 1;

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

        float magnetBonus = progression.GetBaseMagnetRadiusLevel() * magnetRadiusPerLevel;
        if (pickupMagnetController != null && progression.IsUnlocked(PlayerProgression.UnlockMagnetId) && magnetBonus > 0f)
        {
            pickupMagnetController.AddMagnetRadius(magnetBonus);
        }

        int cannonBonus = progression.GetPermanentCannonDamageLevel() * cannonDamagePerLevel;
        if (cannonShooter != null && cannonBonus > 0)
        {
            cannonShooter.AddCannonballDamage(cannonBonus);
        }

        float cannonballSpeedBonus = progression.GetBaseCannonballSpeedLevel() * cannonballSpeedPerLevel;
        if (cannonShooter != null && progression.IsUnlocked(PlayerProgression.UnlockCannonballSpeedId) && cannonballSpeedBonus > 0f)
        {
            cannonShooter.AddCannonballSpeed(cannonballSpeedBonus);
        }

        if (cannonShooter != null && progression.IsUnlocked(PlayerProgression.UnlockCannonballSizeId))
        {
            cannonShooter.SetCannonballSizeMultiplier(cannonballSizeMultiplierOnUnlock);
        }

        if (cannonShooter != null && progression.IsUnlocked(PlayerProgression.UnlockCannonballExplosionId))
        {
            int explosionPowerLevel = progression.GetExplosionPowerLevel();
            float radius = baseExplosionRadius + explosionRadiusPerPowerLevel * explosionPowerLevel;
            int damage = baseExplosionDamage + explosionDamagePerPowerLevel * explosionPowerLevel;
            cannonShooter.EnableExplosiveCannonballs(radius, damage);
        }

        if (shipHealth != null && progression.IsUnlocked(PlayerProgression.UnlockHealthRegenId))
        {
            shipHealth.EnableHealthRegeneration(healthRegenerationAmount, healthRegenerationInterval);
        }
    }
}
