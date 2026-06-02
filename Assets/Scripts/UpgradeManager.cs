using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and applies run upgrade choices for the player ship and abilities.
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    private const string HealthId = "health";
    private const string SpeedId = "speed";
    private const string CannonDamageId = "cannon_damage";
    private const string CannonballExplosionId = "cannonball_explosion";
    private const string CannonballSizeId = "cannonball_size";
    private const string CannonballSpeedId = "cannonball_speed";
    private const string CannonballShootRateId = "cannonball_shoot_rate";
    private const string BarnaclesId = "barnacles";
    private const string CursedDoubloonsId = "cursed_doubloons";
    private const string DashId = "dash";
    private const string MagnetId = "magnet";
    private const string ForceFieldId = "force_field";
    private const string HealthRegenId = "health_regen";
    private const string CannonballPierceId = "cannonball_pierce";

    /// <summary>
    /// Describes one selectable run upgrade option.
    /// </summary>
    [Serializable]
    public class UpgradeOption
    {
        /// <summary>
        /// Unique identifier for this option.
        /// </summary>
        public string id;
        /// <summary>
        /// Display name shown to the player.
        /// </summary>
        public string displayName;
        /// <summary>
        /// Description shown for this upgrade option.
        /// </summary>
        [TextArea] public string description;
    }

    [Header("References")]
    [SerializeField] private PlayerLevelSystem playerLevelSystem;
    [SerializeField] private ShipController2D shipController;
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private CannonShooter cannonShooter;
    [SerializeField] private UpgradeChoiceUI upgradeChoiceUI;
    [SerializeField] private PickupMagnetController pickupMagnetController;
    [SerializeField] private ShipDashController dashController;
    [SerializeField] private ForceFieldController forceFieldController;
    [SerializeField] private CursedDoubloonsController cursedDoubloonsController;

    [Header("Default Active Upgrade Values")]
    [SerializeField] private int healthIncreasePerUpgrade = 2;
    [SerializeField] private float speedIncreasePerUpgrade = 0.5f;
    [SerializeField] private int cannonDamageIncreasePerUpgrade = 1;

    [Header("Unlockable Active Upgrade Values")]
    [SerializeField] private float cannonballExplosionRadiusPerUpgrade = 0.25f;
    [SerializeField] private float cannonballBaseExplosionRadius = 2f;
    [SerializeField] private int cannonballBaseExplosionDamage = 1;
    [SerializeField] private int cannonballExplosionDamagePerShipShopLevel = 1;
    [SerializeField] private float cannonballSizeIncreasePerUpgrade = 0.25f;
    [SerializeField] private float cannonballSpeedUpgradeAmount = 1f;
    [SerializeField] private float cannonballShootCooldownReductionAmount = 0.05f;
    [SerializeField] private float dashSpeedUpgradeAmount = 2f;
    [SerializeField] private float dashCooldownReductionAmount = 0.2f;
    [SerializeField] private float magnetRadiusUpgradeAmount = 1f;
    [SerializeField] private float forceFieldRadiusUpgradeAmount = 0.5f;
    [SerializeField] private int forceFieldDamageUpgradeAmount = 1;
    [SerializeField] private int healthRegenerationAmountIncrease = 1;
    [SerializeField] private float healthRegenerationIntervalReduction = 0.5f;


    [Header("Active Upgrade Max Levels")]
    [SerializeField] private int maxHealthActiveLevel = 10;
    [SerializeField] private int maxSpeedActiveLevel = 10;
    [SerializeField] private int maxCannonDamageActiveLevel = 10;
    [SerializeField] private int maxCannonballExplosionActiveLevel = 10;
    [SerializeField] private int maxCannonballSizeActiveLevel = 10;
    [SerializeField] private int maxCannonballSpeedActiveLevel = 10;
    [SerializeField] private int maxCannonballShootRateActiveLevel = 10;
    [SerializeField] private int maxBarnaclesActiveLevel = 10;
    [SerializeField] private int maxCursedDoubloonsActiveLevel = 10;
    [SerializeField] private int maxDashActiveLevel = 10;
    [SerializeField] private int maxMagnetActiveLevel = 10;
    [SerializeField] private int maxForceFieldActiveLevel = 10;
    [SerializeField] private int maxHealthRegenerationActiveLevel = 10;
    [SerializeField] private int maxCannonballPierceActiveLevel = 10;

    [Header("Shop Gating")]
    [SerializeField] private bool allowDashWithoutShopUnlock = false;
    [SerializeField] private bool allowForceFieldWithoutShopUnlock = false;

    [Header("Placeholder Upgrades")]
    [SerializeField] private bool includePlaceholderLuckUpgrades = false;

    [Header("Runtime Stats")]
    [SerializeField] private float magnetRadius;

    private readonly List<UpgradeOption> currentChoices = new();
    private readonly Dictionary<string, int> currentRunUpgradeLevels = new();
    private readonly Dictionary<string, string> currentRunUpgradeDisplayNames = new();

    /// <summary>
    /// Gets the current pickup magnet radius.
    /// </summary>
    public float MagnetRadius => magnetRadius;
    /// <summary>
    /// Gets current run upgrade levels keyed by upgrade id.
    /// </summary>
    public IReadOnlyDictionary<string, int> CurrentRunUpgradeLevels => currentRunUpgradeLevels;

    /// <summary>
    /// Gets the current run level for one upgrade id.
    /// </summary>
    /// <param name="upgradeId">Upgrade identifier to look up.</param>
    /// <returns>The current run upgrade level value.</returns>
    public int GetCurrentRunUpgradeLevel(string upgradeId)
    {
        string canonicalId = GetCanonicalUpgradeId(upgradeId);
        return !string.IsNullOrWhiteSpace(canonicalId) && currentRunUpgradeLevels.TryGetValue(canonicalId, out int level) ? level : 0;
    }

    /// <summary>
    /// Gets a display name for one current run upgrade id.
    /// </summary>
    /// <param name="upgradeId">Upgrade identifier to look up.</param>
    /// <returns>The current run upgrade display name value.</returns>
    public string GetCurrentRunUpgradeDisplayName(string upgradeId)
    {
        string canonicalId = GetCanonicalUpgradeId(upgradeId);
        if (string.IsNullOrWhiteSpace(canonicalId))
        {
            return string.Empty;
        }

        return currentRunUpgradeDisplayNames.TryGetValue(canonicalId, out string displayName) ? displayName : canonicalId;
    }

    /// <summary>
    /// Clears upgrade levels tracked for the current run.
    /// </summary>
    public void ResetCurrentRunUpgradeLevels()
    {
        currentRunUpgradeLevels.Clear();
        currentRunUpgradeDisplayNames.Clear();

        if (cursedDoubloonsController != null)
        {
            cursedDoubloonsController.ResetForNewRun();
        }
    }

    private void Awake()
    {
        ResetCurrentRunUpgradeLevels();

        if (playerLevelSystem == null) playerLevelSystem = GetComponent<PlayerLevelSystem>();
        if (shipController == null) shipController = GetComponent<ShipController2D>();
        if (shipHealth == null) shipHealth = GetComponent<ShipHealth>();
        if (cannonShooter == null) cannonShooter = GetComponent<CannonShooter>();
        if (pickupMagnetController == null) pickupMagnetController = GetComponent<PickupMagnetController>();
        if (dashController == null) dashController = GetComponent<ShipDashController>();
        if (forceFieldController == null) forceFieldController = GetComponent<ForceFieldController>();
        if (cursedDoubloonsController == null) cursedDoubloonsController = GetComponent<CursedDoubloonsController>();
        if (cursedDoubloonsController != null) cursedDoubloonsController.ResetForNewRun();
    }

    private void OnEnable()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {
        if (playerLevelSystem != null)
        {
            playerLevelSystem.OnLevelUp -= HandleLevelUp;
        }
    }

    private void HandleLevelUp(int _)
    {
        if (upgradeChoiceUI == null)
        {
            Debug.LogWarning("UpgradeManager: UpgradeChoiceUI reference is missing.", this);
            playerLevelSystem.NotifyLevelUpChoiceCompleted();
            return;
        }

        BuildUpgradeChoices();

        if (currentChoices.Count == 0)
        {
            Debug.LogWarning("UpgradeManager: No valid upgrade choices available.", this);
            playerLevelSystem.NotifyLevelUpChoiceCompleted();
            return;
        }

        upgradeChoiceUI.ShowChoices(currentChoices, ApplyUpgrade);
    }

    /// <summary>
    /// Gets a random set of available upgrade options.
    /// </summary>
    /// <param name="choiceCount">Number of choices to return.</param>
    /// <returns>A list of upgrade options available for selection.</returns>
    public List<UpgradeOption> GetRandomUpgradeChoices(int choiceCount)
    {
        return new List<UpgradeOption>(BuildRandomUpgradeChoices(choiceCount));
    }

    /// <summary>
    /// Applies a selected upgrade without charging currency.
    /// </summary>
    /// <param name="chosenOption">Upgrade option to apply.</param>
    public void ApplyFreeUpgrade(UpgradeOption chosenOption)
    {
        ApplyUpgradeInternal(chosenOption);
    }

    private void BuildUpgradeChoices()
    {
        currentChoices.Clear();
        currentChoices.AddRange(BuildRandomUpgradeChoices(3));
    }

    private List<UpgradeOption> BuildRandomUpgradeChoices(int choiceCount)
    {
        List<UpgradeOption> choices = new();
        if (choiceCount <= 0)
        {
            return choices;
        }

        List<UpgradeOption> pool = BuildActiveGameUpgradePool();
        int count = Mathf.Min(choiceCount, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(i, pool.Count);
            (pool[i], pool[index]) = (pool[index], pool[i]);
            choices.Add(pool[i]);
        }

        return choices;
    }

    private List<UpgradeOption> BuildActiveGameUpgradePool()
    {
        PlayerProgression progression = PlayerProgression.Instance;
        List<UpgradeOption> pool = new()
        {
            CreateOption(HealthId, "Health Upgrade", "Increase max health and heal to full."),
            CreateOption(SpeedId, "Ship Speed", "Increase ship movement speed."),
            CreateOption(CannonDamageId, "Cannonball Damage", "Increase cannonball damage.")
        };

        AddUnlocked(pool, progression, PlayerProgression.UnlockCannonballExplosionId, CannonballExplosionId, "Cannonball Explosion", "Enable explosive cannonballs and increase explosion radius.", cannonShooter != null);
        AddUnlocked(pool, progression, PlayerProgression.UnlockCannonballSizeId, CannonballSizeId, "Cannonball Size", "Increase cannonball size for this run.", cannonShooter != null);
        AddUnlocked(pool, progression, PlayerProgression.UnlockCannonballSpeedId, CannonballSpeedId, "Cannonball Speed", "Increase cannonball travel speed.", cannonShooter != null);
        AddUnlocked(pool, progression, PlayerProgression.UnlockCannonballShootRateId, CannonballShootRateId, "Cannonball Shoot Rate", "Fire cannonballs more frequently.", cannonShooter != null, progression.IsUnlocked(PlayerProgression.UnlockCannonballSpeedId));
        AddUnlocked(pool, progression, PlayerProgression.UnlockBarnaclesId, BarnaclesId, "Barnacles", "Improve barnacle attachment chance and duration.", true);
        AddUnlocked(pool, progression, PlayerProgression.UnlockCursedDoubloonsId, CursedDoubloonsId, "Cursed Doubloons", "Activate or improve spinning cursed doubloons.", cursedDoubloonsController != null);
        AddUnlocked(pool, progression, PlayerProgression.UnlockDashId, DashId, "Dash Upgrade", "Unlock dash, then dash farther and reduce cooldown.", dashController != null || allowDashWithoutShopUnlock, allowDashWithoutShopUnlock);
        AddUnlocked(pool, progression, PlayerProgression.UnlockMagnetId, MagnetId, "Magnet Radius Upgrade", "Increase pickup magnet radius.", true);
        AddUnlocked(pool, progression, PlayerProgression.UnlockForceFieldId, ForceFieldId, "Force Field Upgrade", "Activate or improve a damaging aura around the ship.", forceFieldController != null || allowForceFieldWithoutShopUnlock, allowForceFieldWithoutShopUnlock);
        AddUnlocked(pool, progression, PlayerProgression.UnlockHealthRegenId, HealthRegenId, "Health Regeneration", "Regenerate health faster during this run.", shipHealth != null);
        AddUnlocked(pool, progression, PlayerProgression.UnlockCannonballPierceId, CannonballPierceId, "Cannonball Pierce", "Let cannonballs pass through one additional enemy.", cannonShooter != null);

        if (includePlaceholderLuckUpgrades)
        {
            pool.Add(CreateOption("gold_luck", "I Love Gold", "Not implemented yet."));
            pool.Add(CreateOption("xp_luck", "XP Luck", "Not implemented yet."));
            pool.Add(CreateOption("wood_luck", "Wood Luck", "Not implemented yet."));
        }

        pool.RemoveAll(option => IsAtActiveMax(option.id));
        return pool;
    }

    private void AddUnlocked(List<UpgradeOption> pool, PlayerProgression progression, string unlockId, string id, string displayName, string description, bool dependenciesAvailable, bool forceAvailable = false)
    {
        if (!dependenciesAvailable)
        {
            return;
        }

        if (forceAvailable || progression.IsUnlocked(unlockId))
        {
            pool.Add(CreateOption(id, displayName, description));
        }
    }

    private static UpgradeOption CreateOption(string id, string displayName, string description)
    {
        return new UpgradeOption { id = id, displayName = displayName, description = description };
    }

    private bool IsAtActiveMax(string id)
    {
        int maxLevel = GetMaxActiveLevel(id);
        return maxLevel > 0 && GetCurrentRunUpgradeLevel(id) >= maxLevel;
    }

    private int GetMaxActiveLevel(string id)
    {
        return GetCanonicalUpgradeId(id) switch
        {
            HealthId => maxHealthActiveLevel,
            SpeedId => maxSpeedActiveLevel,
            CannonDamageId => maxCannonDamageActiveLevel,
            CannonballExplosionId => maxCannonballExplosionActiveLevel,
            CannonballSizeId => maxCannonballSizeActiveLevel,
            CannonballSpeedId => maxCannonballSpeedActiveLevel,
            CannonballShootRateId => maxCannonballShootRateActiveLevel,
            BarnaclesId => maxBarnaclesActiveLevel,
            CursedDoubloonsId => maxCursedDoubloonsActiveLevel,
            DashId => maxDashActiveLevel,
            MagnetId => maxMagnetActiveLevel,
            ForceFieldId => maxForceFieldActiveLevel,
            HealthRegenId => maxHealthRegenerationActiveLevel,
            CannonballPierceId => maxCannonballPierceActiveLevel,
            _ => 0
        };
    }

    private void ApplyUpgrade(UpgradeOption chosenOption)
    {
        if (chosenOption == null)
        {
            return;
        }

        ApplyUpgradeInternal(chosenOption);

        if (playerLevelSystem != null)
        {
            playerLevelSystem.NotifyLevelUpChoiceCompleted();
        }
    }

    private void ApplyUpgradeInternal(UpgradeOption chosenOption)
    {
        if (chosenOption == null)
        {
            return;
        }

        string upgradeId = GetCanonicalUpgradeId(chosenOption.id);
        int currentLevel = GetCurrentRunUpgradeLevel(upgradeId);
        int nextLevel = currentLevel + 1;

        switch (upgradeId)
        {
            case HealthId:
                if (shipHealth != null) shipHealth.AddMaxHealth(healthIncreasePerUpgrade, true);
                break;
            case SpeedId:
                if (shipController != null) shipController.AddMoveSpeed(speedIncreasePerUpgrade);
                break;
            case CannonDamageId:
                if (cannonShooter != null) cannonShooter.AddCannonballDamage(cannonDamageIncreasePerUpgrade);
                break;
            case CannonballExplosionId:
                ApplyCannonballExplosionUpgrade(nextLevel);
                break;
            case CannonballSizeId:
                if (cannonShooter != null) cannonShooter.AddCannonballSizeMultiplier(cannonballSizeIncreasePerUpgrade);
                break;
            case CannonballSpeedId:
                if (cannonShooter != null) cannonShooter.AddCannonballSpeed(cannonballSpeedUpgradeAmount);
                break;
            case CannonballShootRateId:
                if (cannonShooter != null) cannonShooter.ReduceShootCooldown(cannonballShootCooldownReductionAmount);
                break;
            case DashId:
                ApplyDashUpgrade(currentLevel);
                break;
            case MagnetId:
                ApplyMagnetUpgrade();
                break;
            case ForceFieldId:
                ApplyForceFieldUpgrade(currentLevel);
                break;
            case HealthRegenId:
                ApplyHealthRegenerationUpgrade(currentLevel);
                break;
            case CannonballPierceId:
                if (cannonShooter != null) cannonShooter.SetCannonballPierceCount(nextLevel);
                break;
            case BarnaclesId:
                Debug.Log($"UpgradeManager: {chosenOption.displayName} selected for this run, but no runtime controller is wired yet.", this);
                break;
            case CursedDoubloonsId:
                ApplyCursedDoubloonsUpgrade();
                break;
            case "gold_luck":
            case "xp_luck":
            case "wood_luck":
                Debug.Log($"UpgradeManager: {chosenOption.displayName} is not implemented yet.", this);
                break;
        }

        TrackCurrentRunUpgrade(chosenOption, upgradeId);
    }

    private void ApplyCannonballExplosionUpgrade(int nextLevel)
    {
        if (cannonShooter == null)
        {
            return;
        }

        PlayerProgression progression = PlayerProgression.Instance;
        int shipShopDamageLevel = progression != null ? progression.GetExplosionPowerLevel() : 0;
        float radius = cannonballBaseExplosionRadius + cannonballExplosionRadiusPerUpgrade * Mathf.Max(0, nextLevel - 1);
        int damage = cannonballBaseExplosionDamage + cannonballExplosionDamagePerShipShopLevel * shipShopDamageLevel;
        cannonShooter.EnableExplosiveCannonballs(radius, damage);
    }

    private void ApplyDashUpgrade(int currentLevel)
    {
        if (dashController == null)
        {
            return;
        }

        if (currentLevel <= 0 && !dashController.DashUnlocked)
        {
            dashController.UnlockDash();
        }
        else
        {
            dashController.AddDashSpeed(dashSpeedUpgradeAmount);
            dashController.ReduceDashCooldown(dashCooldownReductionAmount);
        }
    }

    private void ApplyMagnetUpgrade()
    {
        if (pickupMagnetController == null) pickupMagnetController = GetComponent<PickupMagnetController>();
        if (pickupMagnetController != null)
        {
            pickupMagnetController.AddMagnetRadius(magnetRadiusUpgradeAmount);
            magnetRadius = pickupMagnetController.MagnetRadius;
        }
        else
        {
            magnetRadius += magnetRadiusUpgradeAmount;
        }
    }

    private void ApplyForceFieldUpgrade(int currentLevel)
    {
        if (forceFieldController == null)
        {
            return;
        }

        if (currentLevel <= 0 && !forceFieldController.ForceFieldUnlocked)
        {
            forceFieldController.UnlockForceField();
            int shipShopDamageLevel = PlayerProgression.Instance != null ? PlayerProgression.Instance.GetForceFieldDamageLevel() : 0;
            if (shipShopDamageLevel > 0)
            {
                forceFieldController.AddDamage(shipShopDamageLevel * forceFieldDamageUpgradeAmount);
            }
            return;
        }

        forceFieldController.AddRadius(forceFieldRadiusUpgradeAmount);
        forceFieldController.AddDamage(forceFieldDamageUpgradeAmount);
    }

    private void ApplyCursedDoubloonsUpgrade()
    {
        if (cursedDoubloonsController == null)
        {
            return;
        }

        cursedDoubloonsController.ActivateOrUpgrade();
    }

    private void ApplyHealthRegenerationUpgrade(int currentLevel)
    {
        if (shipHealth == null)
        {
            return;
        }

        if (currentLevel <= 0)
        {
            shipHealth.EnableHealthRegeneration(healthRegenerationAmountIncrease, 5f);
        }
        else
        {
            shipHealth.ImproveHealthRegeneration(healthRegenerationAmountIncrease, healthRegenerationIntervalReduction);
        }
    }

    private void TrackCurrentRunUpgrade(UpgradeOption chosenOption, string canonicalId)
    {
        if (chosenOption == null || string.IsNullOrWhiteSpace(canonicalId))
        {
            return;
        }

        currentRunUpgradeLevels.TryGetValue(canonicalId, out int currentLevel);
        currentRunUpgradeLevels[canonicalId] = currentLevel + 1;
        currentRunUpgradeDisplayNames[canonicalId] = string.IsNullOrWhiteSpace(chosenOption.displayName) ? canonicalId : chosenOption.displayName;
    }

    private static string GetCanonicalUpgradeId(string upgradeId)
    {
        return upgradeId switch
        {
            "dash_unlock" => DashId,
            "dash_boost" => DashId,
            "force_field_unlock" => ForceFieldId,
            "force_field_boost" => ForceFieldId,
            _ => upgradeId
        };
    }
}
