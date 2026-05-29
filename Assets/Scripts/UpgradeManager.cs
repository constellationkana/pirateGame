using System;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    [Serializable]
    public class UpgradeOption
    {
        public string id;
        public string displayName;
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

    [Header("Upgrade Values")]
    [SerializeField] private int healthIncreasePerUpgrade = 2;
    [SerializeField] private float speedIncreasePerUpgrade = 0.5f;
    [SerializeField] private float magnetRadiusUpgradeAmount = 1f;
    [SerializeField] private int cannonDamageIncreasePerUpgrade = 1;
    [SerializeField] private float cannonballSpeedUpgradeAmount = 1f;
    [SerializeField] private float dashSpeedUpgradeAmount = 2f;
    [SerializeField] private float dashCooldownReductionAmount = 0.2f;
    [SerializeField] private float forceFieldRadiusUpgradeAmount = 0.5f;
    [SerializeField] private int forceFieldDamageUpgradeAmount = 1;

    [Header("Shop Gating")]
    [SerializeField] private bool allowDashWithoutShopUnlock = false;
    [SerializeField] private bool allowForceFieldWithoutShopUnlock = false;

    [Header("Runtime Stats")]
    [SerializeField] private float magnetRadius;

    private readonly List<UpgradeOption> phaseOneOptions = new();
    private readonly List<UpgradeOption> currentChoices = new();

    public float MagnetRadius => magnetRadius;

    private void Awake()
    {
        if (playerLevelSystem == null)
        {
            playerLevelSystem = GetComponent<PlayerLevelSystem>();
        }

        if (shipController == null)
        {
            shipController = GetComponent<ShipController2D>();
        }

        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
        }

        if (cannonShooter == null)
        {
            cannonShooter = GetComponent<CannonShooter>();
        }

        if (pickupMagnetController == null)
        {
            pickupMagnetController = GetComponent<PickupMagnetController>();
        }

        if (dashController == null)
        {
            dashController = GetComponent<ShipDashController>();
        }

        if (forceFieldController == null)
        {
            forceFieldController = GetComponent<ForceFieldController>();
        }

        phaseOneOptions.Add(new UpgradeOption { id = "health", displayName = "Health Upgrade", description = "Increase max health and heal to full." });
        phaseOneOptions.Add(new UpgradeOption { id = "cannon_damage", displayName = "Cannonball Damage", description = "Increase cannonball damage." });
        phaseOneOptions.Add(new UpgradeOption { id = "speed", displayName = "Ship Speed", description = "Increase ship movement speed." });
        phaseOneOptions.Add(new UpgradeOption { id = "gold_luck", displayName = "I Love Gold", description = "Not implemented yet." });
        phaseOneOptions.Add(new UpgradeOption { id = "xp_luck", displayName = "XP Luck", description = "Not implemented yet." });
        phaseOneOptions.Add(new UpgradeOption { id = "wood_luck", displayName = "Wood Luck", description = "Not implemented yet." });
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

    private void BuildUpgradeChoices()
    {
        currentChoices.Clear();

        List<UpgradeOption> pool = new();
        pool.AddRange(phaseOneOptions);

        PlayerProgression progression = PlayerProgression.Instance;

        if (progression.IsUnlocked(PlayerProgression.UnlockMagnetId))
        {
            pool.Add(new UpgradeOption { id = "magnet", displayName = "Magnet Radius Upgrade", description = "Increase pickup magnet radius." });
        }

        if (progression.IsUnlocked(PlayerProgression.UnlockCannonballSpeedId) && cannonShooter != null)
        {
            pool.Add(new UpgradeOption { id = "cannonball_speed", displayName = "Cannonball Speed", description = "Increase cannonball travel speed." });
        }

        bool dashShopUnlocked = progression.IsUnlocked(PlayerProgression.UnlockDashId) || allowDashWithoutShopUnlock;
        if (dashShopUnlocked)
        {
            if (dashController != null)
            {
                if (!dashController.DashUnlocked)
                {
                    pool.Add(new UpgradeOption { id = "dash_unlock", displayName = "Dash Upgrade", description = "Press Left Shift to burst forward." });
                }
                else
                {
                    pool.Add(new UpgradeOption { id = "dash_boost", displayName = "Dash Upgrade", description = "Dash farther and reduce the cooldown." });
                }
            }
            else
            {
                Debug.LogWarning("UpgradeManager: ShipDashController is missing. Dash upgrades disabled.", this);
            }
        }

        bool forceFieldShopUnlocked = progression.IsUnlocked(PlayerProgression.UnlockForceFieldId) || allowForceFieldWithoutShopUnlock;
        if (forceFieldShopUnlocked)
        {
            if (forceFieldController != null)
            {
                if (!forceFieldController.ForceFieldUnlocked)
                {
                    pool.Add(new UpgradeOption { id = "force_field_unlock", displayName = "Force Field Upgrade", description = "Damages nearby enemy ships over time." });
                }
                else
                {
                    pool.Add(new UpgradeOption { id = "force_field_boost", displayName = "Force Field Upgrade", description = "Increase aura radius and damage." });
                }
            }
            else
            {
                Debug.LogWarning("UpgradeManager: ForceFieldController is missing. Force Field upgrades disabled.", this);
            }
        }

        int count = Mathf.Min(3, pool.Count);
        for (int i = 0; i < count; i++)
        {
            int index = UnityEngine.Random.Range(i, pool.Count);
            (pool[i], pool[index]) = (pool[index], pool[i]);
            currentChoices.Add(pool[i]);
        }
    }

    private void ApplyUpgrade(UpgradeOption chosenOption)
    {
        if (chosenOption == null)
        {
            return;
        }

        switch (chosenOption.id)
        {
            case "health":
                if (shipHealth != null)
                {
                    shipHealth.AddMaxHealth(healthIncreasePerUpgrade, true);
                }
                break;
            case "speed":
                if (shipController != null)
                {
                    shipController.AddMoveSpeed(speedIncreasePerUpgrade);
                }
                break;
            case "magnet":
                if (pickupMagnetController == null)
                {
                    pickupMagnetController = GetComponent<PickupMagnetController>();
                }

                if (pickupMagnetController != null)
                {
                    pickupMagnetController.AddMagnetRadius(magnetRadiusUpgradeAmount);
                    magnetRadius = pickupMagnetController.MagnetRadius;
                }
                else
                {
                    magnetRadius += magnetRadiusUpgradeAmount;
                }
                break;
            case "cannon_damage":
                if (cannonShooter != null)
                {
                    cannonShooter.AddCannonballDamage(cannonDamageIncreasePerUpgrade);
                }
                break;
            case "cannonball_speed":
                if (cannonShooter != null)
                {
                    cannonShooter.AddCannonballSpeed(cannonballSpeedUpgradeAmount);
                }
                break;
            case "gold_luck":
            case "xp_luck":
            case "wood_luck":
                Debug.Log($"UpgradeManager: {chosenOption.displayName} is not implemented yet.", this);
                break;
            case "dash_unlock":
                if (dashController != null)
                {
                    dashController.UnlockDash();
                }
                break;
            case "dash_boost":
                if (dashController != null)
                {
                    dashController.AddDashSpeed(dashSpeedUpgradeAmount);
                    dashController.ReduceDashCooldown(dashCooldownReductionAmount);
                }
                break;
            case "force_field_unlock":
                if (forceFieldController != null)
                {
                    forceFieldController.UnlockForceField();
                }
                break;
            case "force_field_boost":
                if (forceFieldController != null)
                {
                    forceFieldController.AddRadius(forceFieldRadiusUpgradeAmount);
                    forceFieldController.AddDamage(forceFieldDamageUpgradeAmount);
                }
                break;
        }

        if (playerLevelSystem != null)
        {
            playerLevelSystem.NotifyLevelUpChoiceCompleted();
        }
    }
}
