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
    [SerializeField] private CannonShooter cannonShooter;
    [SerializeField] private UpgradeChoiceUI upgradeChoiceUI;

    [Header("Upgrade Values")]
    [SerializeField] private float speedIncreasePerUpgrade = 0.5f;
    [SerializeField] private float magnetRadiusIncreasePerUpgrade = 0.75f;
    [SerializeField] private int cannonDamageIncreasePerUpgrade = 1;

    [Header("Runtime Stats")]
    [SerializeField] private float magnetRadius;

    private readonly List<UpgradeOption> phaseOneOptions = new();

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

        if (cannonShooter == null)
        {
            cannonShooter = GetComponent<CannonShooter>();
        }

        phaseOneOptions.Add(new UpgradeOption { id = "speed", displayName = "Speed Upgrade", description = "Increase ship movement speed." });
        phaseOneOptions.Add(new UpgradeOption { id = "magnet", displayName = "Magnet Radius Upgrade", description = "Increase pickup magnet radius for future systems." });
        phaseOneOptions.Add(new UpgradeOption { id = "cannon_damage", displayName = "Cannonball Damage Upgrade", description = "Increase cannonball damage." });
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

        upgradeChoiceUI.ShowChoices(phaseOneOptions, ApplyUpgrade);
    }

    private void ApplyUpgrade(UpgradeOption chosenOption)
    {
        if (chosenOption == null)
        {
            return;
        }

        switch (chosenOption.id)
        {
            case "speed":
                if (shipController != null)
                {
                    shipController.AddMoveSpeed(speedIncreasePerUpgrade);
                }
                break;
            case "magnet":
                magnetRadius += magnetRadiusIncreasePerUpgrade;
                break;
            case "cannon_damage":
                if (cannonShooter != null)
                {
                    cannonShooter.AddCannonballDamage(cannonDamageIncreasePerUpgrade);
                }
                break;
        }

        if (playerLevelSystem != null)
        {
            playerLevelSystem.NotifyLevelUpChoiceCompleted();
        }
    }
}
