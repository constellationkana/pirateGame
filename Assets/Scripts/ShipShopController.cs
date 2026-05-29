using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipShopController : MonoBehaviour
{
    [Serializable]
    private class UpgradePurchaseConfig
    {
        public int baseCost = 50;
        public int costIncreasePerLevel = 50;
        [Tooltip("0 means uncapped.")]
        public int maxLevel = 10;
        public float statIncreasePerLevel = 1f;
    }

    [Serializable]
    private class UnlockPurchaseConfig
    {
        public int cost = 100;
    }

    [Header("Top Bar")]
    [SerializeField] private TMP_Text doubloonsText;

    [Header("Shop Feedback")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float messageDuration = 2f;

    [Header("Scene Flow")]
    [SerializeField] private string mainSeaSceneName = "MainSea";
    [SerializeField] private bool logSceneTransitions = true;

    [Header("Health Stand Text")]
    [SerializeField] private TMP_Text healthUpgradeText;
    [SerializeField] private TMP_Text healthRegenUnlockText;

    [Header("Speed Stand Text")]
    [SerializeField] private TMP_Text speedUpgradeText;
    [SerializeField] private TMP_Text dashUnlockText;

    [Header("Arsenal Stand Text")]
    [SerializeField] private TMP_Text cannonUpgradeText;
    [SerializeField] private TMP_Text cannonballSizeUnlockText;
    [SerializeField] private TMP_Text cannonballSpeedUnlockText;
    [SerializeField] private TMP_Text cannonballSpeedUpgradeText;
    [SerializeField] private TMP_Text explodingCannonballsUnlockText;
    [SerializeField] private TMP_Text explosionPowerUpgradeText;
    [SerializeField] private TMP_Text barnaclesUnlockText;
    [SerializeField] private TMP_Text barnaclePowerUpgradeText;
    [SerializeField] private TMP_Text cannonballPierceUnlockText;

    [Header("Abilities Stand Text")]
    [SerializeField] private TMP_Text magnetUnlockText;
    [SerializeField] private TMP_Text magnetUpgradeText;
    [SerializeField] private TMP_Text forceFieldUnlockText;
    [SerializeField] private TMP_Text cursedDoubloonsUnlockText;

    [Header("Permanent Upgrade Prices, Amounts, and Max Levels")]
    [SerializeField] private UpgradePurchaseConfig baseHealth = new() { baseCost = 50, costIncreasePerLevel = 50, maxLevel = 10, statIncreasePerLevel = 2f };
    [SerializeField] private UpgradePurchaseConfig baseSpeed = new() { baseCost = 75, costIncreasePerLevel = 75, maxLevel = 10, statIncreasePerLevel = 0.25f };
    [SerializeField] private UpgradePurchaseConfig baseCannonDamage = new() { baseCost = 100, costIncreasePerLevel = 100, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig baseCannonballSpeed = new() { baseCost = 100, costIncreasePerLevel = 100, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig baseMagnetRadius = new() { baseCost = 60, costIncreasePerLevel = 60, maxLevel = 10, statIncreasePerLevel = 0.5f };
    [SerializeField] private UpgradePurchaseConfig explosionPower = new() { baseCost = 150, costIncreasePerLevel = 150, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig barnaclePower = new() { baseCost = 125, costIncreasePerLevel = 125, maxLevel = 10, statIncreasePerLevel = 1f };

    [Header("One-Time Unlock Prices")]
    [SerializeField] private UnlockPurchaseConfig healthRegenUnlock = new() { cost = 150 };
    [SerializeField] private UnlockPurchaseConfig dashUnlock = new() { cost = 250 };
    [SerializeField] private UnlockPurchaseConfig magnetUnlock = new() { cost = 60 };
    [SerializeField] private UnlockPurchaseConfig forceFieldUnlock = new() { cost = 400 };
    [SerializeField] private UnlockPurchaseConfig cannonballSizeUnlock = new() { cost = 150 };
    [SerializeField] private UnlockPurchaseConfig cannonballSpeedUnlock = new() { cost = 150 };
    [SerializeField] private UnlockPurchaseConfig explodingCannonballsUnlock = new() { cost = 250 };
    [SerializeField] private UnlockPurchaseConfig cannonballPierceUnlock = new() { cost = 200 };
    [SerializeField] private UnlockPurchaseConfig barnaclesUnlock = new() { cost = 200 };
    [SerializeField] private UnlockPurchaseConfig cursedDoubloonsUnlock = new() { cost = 200 };

    [Header("Cosmetics")]
    [SerializeField] private string[] cosmeticIds;
    [SerializeField] private Sprite[] cosmeticSprites;
    [SerializeField] private int[] cosmeticCosts;
    [SerializeField] private TMP_Text cosmeticStatusText;

    private PlayerProgression progression;
    private int currentCosmeticIndex;
    private Coroutine clearMessageRoutine;

    private void Start()
    {
        progression = PlayerProgression.Instance;
        if (progression == null)
        {
            Debug.LogWarning("ShipShopController: PlayerProgression singleton could not be created.", this);
            return;
        }

        RefreshUI();
    }

    public void BuyHealthUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseHealthId, baseHealth, null, "Base health upgraded!");
    public void UnlockHealthRegeneration() => TryBuyUnlock(PlayerProgression.UnlockHealthRegenId, healthRegenUnlock, "Health regeneration unlocked for level-up choices!");
    public void BuySpeedUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseSpeedId, baseSpeed, null, "Base ship speed upgraded!");
    public void UnlockDash() => TryBuyUnlock(PlayerProgression.UnlockDashId, dashUnlock, "Dash unlocked for level-up choices!");
    public void BuyDashUnlock() => UnlockDash();
    public void BuyCannonDamageUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseCannonDamageId, baseCannonDamage, null, "Base cannonball damage upgraded!");
    public void UnlockCannonballSizeUpgrade() => TryBuyUnlock(PlayerProgression.UnlockCannonballSizeId, cannonballSizeUnlock, "Cannonball size upgrades unlocked!");
    public void UnlockCannonballSpeedUpgrade() => TryBuyUnlock(PlayerProgression.UnlockCannonballSpeedId, cannonballSpeedUnlock, "Cannonball speed upgrades unlocked!");
    public void BuyBaseCannonballSpeedUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseCannonballSpeedId, baseCannonballSpeed, PlayerProgression.UnlockCannonballSpeedId, "Base cannonball speed upgraded!");
    public void UnlockExplodingCannonballs() => TryBuyUnlock(PlayerProgression.UnlockCannonballExplosionId, explodingCannonballsUnlock, "Exploding cannonballs unlocked!");
    public void BuyExplosionPowerUpgrade() => TryBuyLevel(PlayerProgression.UpgradeExplosionPowerId, explosionPower, PlayerProgression.UnlockCannonballExplosionId, "Explosion power upgraded!");
    public void UnlockBarnacles() => TryBuyUnlock(PlayerProgression.UnlockBarnaclesId, barnaclesUnlock, "Barnacles unlocked!");
    public void BuyBarnaclesUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBarnaclePowerId, barnaclePower, PlayerProgression.UnlockBarnaclesId, "Barnacles upgraded!");
    public void UnlockCannonballPierce() => TryBuyUnlock(PlayerProgression.UnlockCannonballPierceId, cannonballPierceUnlock, "Cannonball pierce unlocked!");
    public void UnlockMagnetUpgrades() => TryBuyUnlock(PlayerProgression.UnlockMagnetId, magnetUnlock, "Magnet upgrades unlocked!");
    public void BuyMagnetUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseMagnetRadiusId, baseMagnetRadius, PlayerProgression.UnlockMagnetId, "Base magnet radius upgraded!");
    public void UnlockForceField() => TryBuyUnlock(PlayerProgression.UnlockForceFieldId, forceFieldUnlock, "Force field unlocked for level-up choices!");
    public void BuyForceFieldUnlock() => UnlockForceField();
    public void UnlockCursedDoubloons() => TryBuyUnlock(PlayerProgression.UnlockCursedDoubloonsId, cursedDoubloonsUnlock, "Cursed doubloons unlocked!");

    public void BuyGenericUnlock(string unlockId)
    {
        if (string.IsNullOrWhiteSpace(unlockId)) return;
        TryBuyUnlock(unlockId, GetUnlockConfig(unlockId), $"Unlocked: {unlockId}");
    }

    public void BuyOrSelectCosmetic(int index)
    {
        if (!HasProgression()) return;
        if (cosmeticIds == null || index < 0 || index >= cosmeticIds.Length) return;

        string cosmeticId = cosmeticIds[index];
        int cost = (cosmeticCosts != null && index < cosmeticCosts.Length) ? Mathf.Max(0, cosmeticCosts[index]) : 0;

        if (!progression.IsCosmeticOwned(cosmeticId) && !progression.BuyCosmetic(cosmeticId, cost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        progression.SetSelectedShipCosmeticId(cosmeticId);
        SetMessage($"Selected: {cosmeticId}");
        RefreshUI();
    }

    public void CycleCosmetic()
    {
        if (cosmeticIds == null || cosmeticIds.Length == 0)
        {
            SetMessage("No cosmetics configured.");
            return;
        }

        BuyOrSelectCosmetic(currentCosmeticIndex);
        currentCosmeticIndex = (currentCosmeticIndex + 1) % cosmeticIds.Length;
    }

    public void OpenCosmeticShop() => CycleCosmetic();

    public void SetGlobalPrompt(string prompt)
    {
        if (promptText != null)
        {
            promptText.text = prompt;
        }
    }

    public void StartRun()
    {
        if (logSceneTransitions)
        {
            Debug.Log($"Starting run, loading {mainSeaSceneName}", this);
        }

        SetMessage("Setting sail...");

        if (string.IsNullOrWhiteSpace(mainSeaSceneName))
        {
            Debug.LogWarning("ShipShopController: mainSeaSceneName is empty.", this);
            return;
        }

        SceneManager.LoadScene(mainSeaSceneName);
    }

    private void TryBuyUnlock(string unlockId, UnlockPurchaseConfig config, string successMessage)
    {
        if (!CanAttemptPurchase()) return;
        if (progression.IsUnlocked(unlockId))
        {
            SetMessage("Already unlocked.");
            RefreshUI();
            return;
        }

        int cost = Mathf.Max(0, config != null ? config.cost : 0);
        if (!progression.TryPurchaseUnlock(unlockId, cost))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        SetMessage(successMessage);
        RefreshUI();
    }

    private void TryBuyLevel(string upgradeId, UpgradePurchaseConfig config, string requiredUnlockId, string successMessage)
    {
        if (!CanAttemptPurchase()) return;
        if (!string.IsNullOrWhiteSpace(requiredUnlockId) && !progression.IsUnlocked(requiredUnlockId))
        {
            SetMessage("Locked. Buy the required unlock first.");
            RefreshUI();
            return;
        }

        config ??= new UpgradePurchaseConfig();
        int currentLevel = progression.GetUpgradeLevel(upgradeId);
        if (IsAtMax(currentLevel, config.maxLevel))
        {
            SetMessage("Max level reached.");
            RefreshUI();
            return;
        }

        if (!progression.TryPurchaseUpgrade(upgradeId, GetUpgradeCost(config, currentLevel), config.maxLevel, requiredUnlockId))
        {
            SetMessage("Not enough doubloons.");
            RefreshUI();
            return;
        }

        SetMessage(successMessage);
        RefreshUI();
    }

    private bool CanAttemptPurchase()
    {
        if (!HasProgression()) return false;
        if (!PlayerProgression.HasActiveSaveSlot)
        {
            SetMessage("No active save slot selected.");
            RefreshUI();
            return false;
        }

        return true;
    }

    private static int GetUpgradeCost(UpgradePurchaseConfig config, int currentLevel)
    {
        if (config == null) return 0;
        return Mathf.Max(0, config.baseCost + config.costIncreasePerLevel * Mathf.Max(0, currentLevel));
    }

    private static bool IsAtMax(int currentLevel, int maxLevel) => maxLevel > 0 && currentLevel >= maxLevel;

    private UnlockPurchaseConfig GetUnlockConfig(string unlockId)
    {
        return unlockId switch
        {
            PlayerProgression.UnlockHealthRegenId => healthRegenUnlock,
            PlayerProgression.UnlockDashId => dashUnlock,
            PlayerProgression.UnlockMagnetId => magnetUnlock,
            PlayerProgression.UnlockForceFieldId => forceFieldUnlock,
            PlayerProgression.UnlockCannonballSizeId => cannonballSizeUnlock,
            PlayerProgression.UnlockCannonballSpeedId => cannonballSpeedUnlock,
            PlayerProgression.UnlockCannonballExplosionId => explodingCannonballsUnlock,
            PlayerProgression.UnlockCannonballPierceId => cannonballPierceUnlock,
            PlayerProgression.UnlockBarnaclesId => barnaclesUnlock,
            PlayerProgression.UnlockCursedDoubloonsId => cursedDoubloonsUnlock,
            _ => new UnlockPurchaseConfig { cost = 0 }
        };
    }

    private bool HasProgression()
    {
        if (progression != null) return true;
        progression = PlayerProgression.Instance;
        if (progression == null)
        {
            Debug.LogWarning("ShipShopController: PlayerProgression is unavailable.", this);
            return false;
        }

        return true;
    }

    private void SetMessage(string message)
    {
        if (messageText == null) return;
        messageText.text = message;

        if (clearMessageRoutine != null)
        {
            StopCoroutine(clearMessageRoutine);
        }

        if (messageDuration > 0f)
        {
            clearMessageRoutine = StartCoroutine(ClearMessageAfterDelay(messageDuration));
        }
    }

    private IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }

        clearMessageRoutine = null;
    }

    private void RefreshUI()
    {
        if (!HasProgression()) return;

        if (doubloonsText != null)
        {
            doubloonsText.text = $"Save: {PlayerProgression.GetActiveSaveName()} | Doubloons: {progression.GetDoubloons()}";
        }

        SetUpgradeText(healthUpgradeText, "Base Health", PlayerProgression.UpgradeBaseHealthId, baseHealth);
        SetUnlockText(healthRegenUnlockText, "Health Regeneration", PlayerProgression.UnlockHealthRegenId, healthRegenUnlock);
        SetUpgradeText(speedUpgradeText, "Base Ship Speed", PlayerProgression.UpgradeBaseSpeedId, baseSpeed);
        SetUnlockText(dashUnlockText, "Dash", PlayerProgression.UnlockDashId, dashUnlock);
        SetUpgradeText(cannonUpgradeText, "Base Cannonball Damage", PlayerProgression.UpgradeBaseCannonDamageId, baseCannonDamage);
        SetUnlockText(cannonballSizeUnlockText, "Cannonball Size Upgrade", PlayerProgression.UnlockCannonballSizeId, cannonballSizeUnlock);
        SetUnlockText(cannonballSpeedUnlockText, "Cannonball Speed Upgrade", PlayerProgression.UnlockCannonballSpeedId, cannonballSpeedUnlock);
        SetUpgradeText(cannonballSpeedUpgradeText, "Base Cannonball Speed", PlayerProgression.UpgradeBaseCannonballSpeedId, baseCannonballSpeed, PlayerProgression.UnlockCannonballSpeedId);
        SetUnlockText(explodingCannonballsUnlockText, "Exploding Cannonballs", PlayerProgression.UnlockCannonballExplosionId, explodingCannonballsUnlock);
        SetUpgradeText(explosionPowerUpgradeText, "Explosion Power", PlayerProgression.UpgradeExplosionPowerId, explosionPower, PlayerProgression.UnlockCannonballExplosionId);
        SetUnlockText(barnaclesUnlockText, "Barnacles", PlayerProgression.UnlockBarnaclesId, barnaclesUnlock);
        SetUpgradeText(barnaclePowerUpgradeText, "Barnacles", PlayerProgression.UpgradeBarnaclePowerId, barnaclePower, PlayerProgression.UnlockBarnaclesId);
        SetUnlockText(cannonballPierceUnlockText, "Cannonball Pierce", PlayerProgression.UnlockCannonballPierceId, cannonballPierceUnlock);
        SetUnlockText(magnetUnlockText, "Magnet Upgrades", PlayerProgression.UnlockMagnetId, magnetUnlock);
        SetUpgradeText(magnetUpgradeText, "Base Magnet Radius", PlayerProgression.UpgradeBaseMagnetRadiusId, baseMagnetRadius, PlayerProgression.UnlockMagnetId);
        SetUnlockText(forceFieldUnlockText, "Force Field", PlayerProgression.UnlockForceFieldId, forceFieldUnlock);
        SetUnlockText(cursedDoubloonsUnlockText, "Cursed Doubloons", PlayerProgression.UnlockCursedDoubloonsId, cursedDoubloonsUnlock);

        if (cosmeticStatusText != null)
        {
            int configuredSprites = cosmeticSprites == null ? 0 : cosmeticSprites.Length;
            int configuredIds = cosmeticIds == null ? 0 : cosmeticIds.Length;
            cosmeticStatusText.text = $"Selected Cosmetic: {progression.GetSelectedShipCosmeticId()}\nCosmetics Configured: {Mathf.Min(configuredIds, configuredSprites)}";
        }
    }

    private void SetUpgradeText(TMP_Text target, string label, string upgradeId, UpgradePurchaseConfig config, string requiredUnlockId = null)
    {
        if (target == null) return;
        config ??= new UpgradePurchaseConfig();
        int currentLevel = progression.GetUpgradeLevel(upgradeId);
        bool locked = !string.IsNullOrWhiteSpace(requiredUnlockId) && !progression.IsUnlocked(requiredUnlockId);
        string status = locked ? "Locked" : IsAtMax(currentLevel, config.maxLevel) ? "Max" : $"Cost: {GetUpgradeCost(config, currentLevel)}";
        string maxText = config.maxLevel > 0 ? $"/{config.maxLevel}" : string.Empty;
        target.text = $"{label} Lv {currentLevel}{maxText} (+{config.statIncreasePerLevel:0.##}) | {status}";
    }

    private void SetUnlockText(TMP_Text target, string label, string unlockId, UnlockPurchaseConfig config)
    {
        if (target == null) return;
        target.text = progression.IsUnlocked(unlockId) ? $"{label}: Unlocked" : $"Unlock {label} ({Mathf.Max(0, config != null ? config.cost : 0)})";
    }
}
