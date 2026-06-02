using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Drives the ship shop UI for upgrades, unlocks, cosmetics, crew hiring, prompts, and run navigation.
/// </summary>
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

    [Serializable]
    private class MenuButtonReferences
    {
        public TMP_Text labelText;
        public Button button;
    }

    [Header("Top Bar")]
    [SerializeField] private TMP_Text doubloonsText;

    [Header("Shop Feedback")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private float messageDuration = 2f;

    [Header("Scene Flow")]
    [SerializeField] private string mapSceneName = "Map";
    [SerializeField] private string mainSeaSceneName = "MainSea";
    [SerializeField] private bool logSceneTransitions = true;

    [Header("Debug")]
    [SerializeField] private bool logShopDebug = false;

    [Header("Category Menu Panels")]
    [SerializeField] private GameObject healthMenuPanel;
    [SerializeField] private GameObject speedMenuPanel;
    [SerializeField] private GameObject arsenalMenuPanel;
    [SerializeField] private GameObject abilitiesMenuPanel;
    [SerializeField] private bool closeMenusOnStart = true;

    [Header("Health Menu Buttons")]
    [SerializeField] private MenuButtonReferences healthUpgrade = new();
    [SerializeField] private MenuButtonReferences healthRegenUnlockButton = new();

    [Header("Speed Menu Buttons")]
    [SerializeField] private MenuButtonReferences speedUpgrade = new();
    [SerializeField] private MenuButtonReferences dashUnlockButton = new();

    [Header("Arsenal Menu Buttons")]
    [SerializeField] private MenuButtonReferences cannonDamageUpgrade = new();
    [SerializeField] private MenuButtonReferences cannonballSizeUnlockButton = new();
    [SerializeField] private MenuButtonReferences cannonballSpeedUnlockButton = new();
    [SerializeField] private MenuButtonReferences cannonballSpeedUpgrade = new();
    [SerializeField] private MenuButtonReferences explodingCannonballsUnlockButton = new();
    [SerializeField] private MenuButtonReferences explosionPowerUpgradeButton = new();
    [SerializeField] private MenuButtonReferences barnaclesUnlockButton = new();
    [SerializeField] private MenuButtonReferences barnaclePowerUpgradeButton = new();
    [SerializeField] private MenuButtonReferences cannonballPierceUnlockButton = new();

    [Header("Abilities Menu Buttons")]
    [SerializeField] private MenuButtonReferences magnetUnlockButton = new();
    [FormerlySerializedAs("forceFieldUnlockButton")]
    [SerializeField] private MenuButtonReferences forceFieldButton = new();
    [SerializeField] private MenuButtonReferences forceFieldDamageUpgradeButton = new();
    [FormerlySerializedAs("cursedDoubloonsUnlockButton")]
    [SerializeField] private MenuButtonReferences cursedDoubloonsButton = new();
    [SerializeField] private MenuButtonReferences cursedDoubloonsDamageUpgradeButton = new();

    [Header("Legacy Text References")]
    [Tooltip("Optional compatibility text for older scenes. Prefer wiring the Menu Button References above.")]
    [SerializeField] private TMP_Text healthUpgradeText;
    [SerializeField] private TMP_Text healthRegenUnlockText;
    [SerializeField] private TMP_Text speedUpgradeText;
    [SerializeField] private TMP_Text dashUnlockText;
    [SerializeField] private TMP_Text cannonUpgradeText;
    [SerializeField] private TMP_Text cannonballSizeUnlockText;
    [SerializeField] private TMP_Text cannonballSpeedUnlockText;
    [SerializeField] private TMP_Text cannonballSpeedUpgradeText;
    [SerializeField] private TMP_Text explodingCannonballsUnlockText;
    [SerializeField] private TMP_Text explosionPowerUpgradeText;
    [SerializeField] private TMP_Text barnaclesUnlockText;
    [SerializeField] private TMP_Text barnaclePowerUpgradeText;
    [SerializeField] private TMP_Text cannonballPierceUnlockText;
    [SerializeField] private TMP_Text magnetUnlockText;
    [SerializeField] private TMP_Text forceFieldUnlockText;
    [SerializeField] private TMP_Text forceFieldDamageUpgradeText;
    [SerializeField] private TMP_Text cursedDoubloonsUnlockText;
    [SerializeField] private TMP_Text cursedDoubloonsDamageUpgradeText;

    [Header("Permanent Upgrade Prices, Amounts, and Max Levels")]
    [SerializeField] private UpgradePurchaseConfig baseHealth = new() { baseCost = 50, costIncreasePerLevel = 50, maxLevel = 10, statIncreasePerLevel = 2f };
    [SerializeField] private UpgradePurchaseConfig baseSpeed = new() { baseCost = 75, costIncreasePerLevel = 75, maxLevel = 10, statIncreasePerLevel = 0.25f };
    [SerializeField] private UpgradePurchaseConfig baseCannonDamage = new() { baseCost = 100, costIncreasePerLevel = 100, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig baseCannonballSpeed = new() { baseCost = 100, costIncreasePerLevel = 100, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig explosionPower = new() { baseCost = 150, costIncreasePerLevel = 150, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig barnaclePower = new() { baseCost = 125, costIncreasePerLevel = 125, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig cursedDoubloonsDamage = new() { baseCost = 125, costIncreasePerLevel = 125, maxLevel = 10, statIncreasePerLevel = 1f };
    [SerializeField] private UpgradePurchaseConfig forceFieldDamage = new() { baseCost = 150, costIncreasePerLevel = 150, maxLevel = 10, statIncreasePerLevel = 1f };

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

    [Header("Crew Menu")]
    [SerializeField] private GameObject crewMenuPanel;
    [SerializeField] private TMP_Text crewNameText;
    [SerializeField] private TMP_Text crewDescriptionText;
    [SerializeField] private TMP_Text crewAbilityText;
    [SerializeField] private TMP_Text crewPriceText;
    [SerializeField] private TMP_Text crewStatusText;
    [SerializeField] private Image crewPortraitImage;
    [SerializeField] private Image crewScrollBackgroundImage;
    [SerializeField] private Sprite defaultCrewScrollSprite;
    [SerializeField] private bool hideTextWhenScrollHasBakedText = false;
    [SerializeField] private Button crewHireButton;
    [SerializeField] private Button crewCloseButton;

    [Header("Cosmetics")]
    [SerializeField] private string[] cosmeticIds;
    [SerializeField] private Sprite[] cosmeticSprites;
    [SerializeField] private int[] cosmeticCosts;
    [SerializeField] private TMP_Text cosmeticStatusText;

    private PlayerProgression progression;
    private int currentCosmeticIndex;
    private Coroutine clearMessageRoutine;
    private CrewNPCInteraction selectedCrew;

    private void Start()
    {
        progression = PlayerProgression.Instance;
        if (progression == null)
        {
            Debug.LogWarning("ShipShopController: PlayerProgression singleton could not be created.", this);
            return;
        }

        WireShopButtonListeners();

        if (closeMenusOnStart)
        {
            CloseAllMenus();
        }

        RefreshUI();
    }

    private void WireShopButtonListeners()
    {
        EnsureButtonListener(healthUpgrade.button, BuyHealthUpgrade, nameof(BuyHealthUpgrade));
        EnsureButtonListener(healthRegenUnlockButton.button, UnlockHealthRegeneration, nameof(UnlockHealthRegeneration));
        EnsureButtonListener(speedUpgrade.button, BuySpeedUpgrade, nameof(BuySpeedUpgrade));
        EnsureButtonListener(dashUnlockButton.button, BuyDashUnlock, nameof(BuyDashUnlock));
        EnsureButtonListener(cannonDamageUpgrade.button, BuyCannonDamageUpgrade, nameof(BuyCannonDamageUpgrade));
        EnsureButtonListener(cannonballSizeUnlockButton.button, UnlockCannonballSizeUpgrade, nameof(UnlockCannonballSizeUpgrade));
        EnsureButtonListener(cannonballSpeedUnlockButton.button, UnlockCannonballSpeedUpgrade, nameof(UnlockCannonballSpeedUpgrade));
        EnsureButtonListener(cannonballSpeedUpgrade.button, BuyBaseCannonballSpeedUpgrade, nameof(BuyBaseCannonballSpeedUpgrade));
        EnsureButtonListener(explodingCannonballsUnlockButton.button, BuyOrUnlockExplodingCannonballs, nameof(BuyOrUnlockExplodingCannonballs));
        EnsureButtonListener(explosionPowerUpgradeButton.button, BuyExplosionPowerUpgrade, nameof(BuyExplosionPowerUpgrade));
        EnsureButtonListener(barnaclesUnlockButton.button, BuyOrUnlockBarnacles, nameof(BuyOrUnlockBarnacles));
        EnsureButtonListener(barnaclePowerUpgradeButton.button, BuyBarnaclesUpgrade, nameof(BuyBarnaclesUpgrade));
        EnsureButtonListener(cannonballPierceUnlockButton.button, UnlockCannonballPierce, nameof(UnlockCannonballPierce));
        EnsureButtonListener(magnetUnlockButton.button, UnlockMagnetUpgrades, nameof(UnlockMagnetUpgrades));
        EnsureButtonListener(forceFieldButton.button, BuyOrUnlockForceField, nameof(BuyOrUnlockForceField));
        EnsureButtonListener(forceFieldDamageUpgradeButton.button, BuyForceFieldDamageUpgrade, nameof(BuyForceFieldDamageUpgrade));
        EnsureButtonListener(cursedDoubloonsButton.button, BuyOrUnlockCursedDoubloons, nameof(BuyOrUnlockCursedDoubloons));
        EnsureButtonListener(cursedDoubloonsDamageUpgradeButton.button, BuyCursedDoubloonsDamageUpgrade, nameof(BuyCursedDoubloonsDamageUpgrade));
        EnsureButtonListener(crewHireButton, HireSelectedCrew, nameof(HireSelectedCrew));
        EnsureButtonListener(crewCloseButton, CloseCrewMenu, nameof(CloseCrewMenu));
    }

    private void EnsureButtonListener(Button button, UnityAction action, string methodName)
    {
        if (button == null || action == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);

        if (!HasPersistentListener(button, methodName))
        {
            button.onClick.AddListener(action);
        }
    }

    private bool HasPersistentListener(Button button, string methodName)
    {
        if (button == null || string.IsNullOrWhiteSpace(methodName))
        {
            return false;
        }

        int listenerCount = button.onClick.GetPersistentEventCount();
        for (int i = 0; i < listenerCount; i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Opens only the health upgrade menu panel.
    /// </summary>
    public void OpenHealthMenu() => OpenOnlyMenu(healthMenuPanel, "Health Menu");
    /// <summary>
    /// Opens only the speed upgrade menu panel.
    /// </summary>
    public void OpenSpeedMenu() => OpenOnlyMenu(speedMenuPanel, "Speed Menu");
    /// <summary>
    /// Opens only the arsenal upgrade menu panel.
    /// </summary>
    public void OpenArsenalMenu() => OpenOnlyMenu(arsenalMenuPanel, "Arsenal Menu");
    /// <summary>
    /// Opens only the abilities upgrade menu panel.
    /// </summary>
    public void OpenAbilitiesMenu() => OpenOnlyMenu(abilitiesMenuPanel, "Abilities Menu");

    /// <summary>
    /// Closes all shop category and crew menu panels.
    /// </summary>
    public void CloseAllMenus()
    {
        SetMenuActive(healthMenuPanel, false);
        SetMenuActive(speedMenuPanel, false);
        SetMenuActive(arsenalMenuPanel, false);
        SetMenuActive(abilitiesMenuPanel, false);
        SetMenuActive(crewMenuPanel, false);
    }

    /// <summary>
    /// Attempts to purchase one base health upgrade level.
    /// </summary>
    public void BuyHealthUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseHealthId, baseHealth, null, "Base health upgraded!");
    /// <summary>
    /// Attempts to unlock health regeneration rewards.
    /// </summary>
    public void UnlockHealthRegeneration() => TryBuyUnlock(PlayerProgression.UnlockHealthRegenId, healthRegenUnlock, "Health regeneration unlocked for level-up choices!");
    /// <summary>
    /// Attempts to purchase one base speed upgrade level.
    /// </summary>
    public void BuySpeedUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseSpeedId, baseSpeed, null, "Base ship speed upgraded!");
    /// <summary>
    /// Unlocks dash and saves the progression state.
    /// </summary>
    public void UnlockDash() => TryBuyUnlock(PlayerProgression.UnlockDashId, dashUnlock, "Dash unlocked for level-up choices!");
    /// <summary>
    /// Compatibility entry point that purchases the dash unlock.
    /// </summary>
    public void BuyDashUnlock() => UnlockDash();
    /// <summary>
    /// Attempts to purchase one base cannon-damage upgrade level.
    /// </summary>
    public void BuyCannonDamageUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseCannonDamageId, baseCannonDamage, null, "Base cannonball damage upgraded!");
    /// <summary>
    /// Attempts to unlock cannonball size rewards.
    /// </summary>
    public void UnlockCannonballSizeUpgrade() => TryBuyUnlock(PlayerProgression.UnlockCannonballSizeId, cannonballSizeUnlock, "Cannonball size upgrades unlocked!");
    /// <summary>
    /// Attempts to unlock cannonball speed rewards and the linked shoot-rate unlock.
    /// </summary>
    public void UnlockCannonballSpeedUpgrade()
    {
        TryBuyUnlock(PlayerProgression.UnlockCannonballSpeedId, cannonballSpeedUnlock, "Cannonball speed and shoot-rate upgrades unlocked!");
        if (HasProgression() && progression.IsUnlocked(PlayerProgression.UnlockCannonballSpeedId) && !progression.IsUnlocked(PlayerProgression.UnlockCannonballShootRateId))
        {
            progression.Unlock(PlayerProgression.UnlockCannonballShootRateId);
        }
    }
    /// <summary>
    /// Attempts to purchase one base cannonball-speed upgrade level.
    /// </summary>
    public void BuyBaseCannonballSpeedUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBaseCannonballSpeedId, baseCannonballSpeed, PlayerProgression.UnlockCannonballSpeedId, "Base cannonball speed upgraded!");
    /// <summary>
    /// Attempts to unlock exploding cannonball rewards.
    /// </summary>
    public void UnlockExplodingCannonballs() => TryBuyUnlock(PlayerProgression.UnlockCannonballExplosionId, explodingCannonballsUnlock, "Exploding cannonballs unlocked for level-up choices!");
    /// <summary>
    /// Attempts to purchase one explosion-power upgrade level.
    /// </summary>
    public void BuyExplosionPowerUpgrade() => TryBuyLevel(PlayerProgression.UpgradeExplosionPowerId, explosionPower, PlayerProgression.UnlockCannonballExplosionId, "Explosion damage upgraded!");
    /// <summary>
    /// Buys explosion power when unlocked, otherwise attempts to unlock exploding cannonballs.
    /// </summary>
    public void BuyOrUnlockExplodingCannonballs()
    {
        if (!HasProgression()) return;
        if (progression.IsUnlocked(PlayerProgression.UnlockCannonballExplosionId)) BuyExplosionPowerUpgrade();
        else UnlockExplodingCannonballs();
    }
    /// <summary>
    /// Attempts to unlock barnacle rewards.
    /// </summary>
    public void UnlockBarnacles() => TryBuyUnlock(PlayerProgression.UnlockBarnaclesId, barnaclesUnlock, "Barnacles unlocked for level-up choices!");
    /// <summary>
    /// Attempts to purchase one barnacle-power upgrade level.
    /// </summary>
    public void BuyBarnaclesUpgrade() => TryBuyLevel(PlayerProgression.UpgradeBarnaclePowerId, barnaclePower, PlayerProgression.UnlockBarnaclesId, "Barnacles damage upgraded!");
    /// <summary>
    /// Buys barnacle power when unlocked, otherwise attempts to unlock barnacles.
    /// </summary>
    public void BuyOrUnlockBarnacles()
    {
        if (!HasProgression()) return;
        if (progression.IsUnlocked(PlayerProgression.UnlockBarnaclesId)) BuyBarnaclesUpgrade();
        else UnlockBarnacles();
    }
    /// <summary>
    /// Attempts to unlock cannonball pierce rewards.
    /// </summary>
    public void UnlockCannonballPierce() => TryBuyUnlock(PlayerProgression.UnlockCannonballPierceId, cannonballPierceUnlock, "Cannonball pierce unlocked!");
    /// <summary>
    /// Attempts to unlock magnet rewards.
    /// </summary>
    public void UnlockMagnetUpgrades() => TryBuyUnlock(PlayerProgression.UnlockMagnetId, magnetUnlock, "Magnet unlocked for level-up choices!");
    /// <summary>
    /// Unlocks force field and saves the progression state.
    /// </summary>
    public void UnlockForceField() => TryBuyUnlock(PlayerProgression.UnlockForceFieldId, forceFieldUnlock, "Force field unlocked for level-up choices!");
    /// <summary>
    /// Compatibility entry point that purchases the force-field unlock.
    /// </summary>
    public void BuyForceFieldUnlock() => UnlockForceField();
    /// <summary>
    /// Attempts to purchase one force-field damage upgrade level.
    /// </summary>
    public void BuyForceFieldDamageUpgrade() => TryBuyLevel(PlayerProgression.UpgradeForceFieldDamageId, forceFieldDamage, PlayerProgression.UnlockForceFieldId, "Force field damage upgraded!");
    /// <summary>
    /// Buys force-field damage when unlocked, otherwise attempts to unlock force field.
    /// </summary>
    public void BuyOrUnlockForceField()
    {
        if (!HasProgression()) return;
        if (progression.IsUnlocked(PlayerProgression.UnlockForceFieldId)) BuyForceFieldDamageUpgrade();
        else UnlockForceField();
    }
    /// <summary>
    /// Attempts to unlock cursed-doubloons rewards.
    /// </summary>
    public void UnlockCursedDoubloons() => TryBuyUnlock(PlayerProgression.UnlockCursedDoubloonsId, cursedDoubloonsUnlock, "Cursed doubloons unlocked for level-up choices!");
    /// <summary>
    /// Attempts to purchase one cursed-doubloons damage upgrade level.
    /// </summary>
    public void BuyCursedDoubloonsDamageUpgrade() => TryBuyLevel(PlayerProgression.UpgradeCursedDoubloonsDamageId, cursedDoubloonsDamage, PlayerProgression.UnlockCursedDoubloonsId, "Cursed doubloons damage upgraded!");
    /// <summary>
    /// Buys cursed-doubloons damage when unlocked, otherwise attempts to unlock cursed doubloons.
    /// </summary>
    public void BuyOrUnlockCursedDoubloons()
    {
        if (!HasProgression()) return;
        if (progression.IsUnlocked(PlayerProgression.UnlockCursedDoubloonsId)) BuyCursedDoubloonsDamageUpgrade();
        else UnlockCursedDoubloons();
    }

    /// <summary>
    /// Attempts to purchase a configured generic unlock by id.
    /// </summary>
    /// <param name="unlockId">Unlock identifier.</param>
    public void BuyGenericUnlock(string unlockId)
    {
        if (string.IsNullOrWhiteSpace(unlockId)) return;
        TryBuyUnlock(unlockId, GetUnlockConfig(unlockId), $"Unlocked: {unlockId}");
    }

    /// <summary>
    /// Purchases a cosmetic if needed, then selects it.
    /// </summary>
    /// <param name="index">Configured array index.</param>
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

    /// <summary>
    /// Selects or buys the current cosmetic and advances to the next configured cosmetic.
    /// </summary>
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

    /// <summary>
    /// Compatibility entry point that cycles cosmetics.
    /// </summary>
    public void OpenCosmeticShop() => CycleCosmetic();

    /// <summary>
    /// Opens the crew hiring menu for a selected crew interaction.
    /// </summary>
    /// <param name="crew">Crew interaction to display.</param>
    public void OpenCrewMenu(CrewNPCInteraction crew)
    {
        if (crew == null)
        {
            SetMessage("No crew selected.");
            return;
        }

        if (!HasProgression()) return;

        selectedCrew = crew;
        CloseAllMenus();

        if (crewMenuPanel == null)
        {
            SetMessage("Crew menu is not assigned.");
            return;
        }

        SetMenuActive(crewMenuPanel, true);
        RefreshCrewMenu();
        SetMessage($"Talking to {selectedCrew.CrewName}");
    }

    /// <summary>
    /// Attempts to spend doubloons and unlock the currently selected crew member.
    /// </summary>
    public void HireSelectedCrew()
    {
        if (selectedCrew == null)
        {
            SetMessage("No crew selected.");
            RefreshCrewMenu();
            return;
        }

        if (!CanAttemptPurchase())
        {
            RefreshCrewMenu();
            return;
        }

        string crewId = selectedCrew.CrewId;
        if (string.IsNullOrWhiteSpace(crewId))
        {
            SetMessage("This crew member is missing an id.");
            RefreshCrewMenu();
            return;
        }

        if (progression.IsCrewUnlocked(crewId))
        {
            SetMessage("Already hired.");
            RefreshCrewMenu();
            return;
        }

        int cost = selectedCrew.Price;
        if (!progression.SpendDoubloons(cost))
        {
            SetMessage("Not enough doubloons.");
            RefreshCrewMenu();
            return;
        }

        progression.UnlockCrew(crewId);
        PlayerPrefs.Save();
        SetMessage($"Hired {selectedCrew.CrewName}!");
        RefreshUI();
        RefreshCrewMenu();
    }

    /// <summary>
    /// Closes the crew menu and clears the selected crew.
    /// </summary>
    public void CloseCrewMenu()
    {
        SetMenuActive(crewMenuPanel, false);
        selectedCrew = null;
    }

    /// <summary>
    /// Updates the global shop prompt text when assigned.
    /// </summary>
    /// <param name="prompt">Prompt message to display.</param>
    public void SetGlobalPrompt(string prompt)
    {
        if (promptText != null)
        {
            promptText.text = prompt;
        }
    }

    /// <summary>
    /// Starts run flow for this controller.
    /// </summary>
    public void StartRun()
    {
        if (logSceneTransitions)
        {
            Debug.Log($"Starting run, loading {mapSceneName}", this);
        }

        SetMessage("Setting sail...");

        if (string.IsNullOrWhiteSpace(mapSceneName))
        {
            Debug.LogWarning("ShipShopController: mapSceneName is empty.", this);
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(mapSceneName);
    }

    private void OpenOnlyMenu(GameObject menuPanel, string menuName)
    {
        if (!HasProgression()) return;

        RefreshUI();
        CloseAllMenus();

        if (menuPanel == null)
        {
            SetMessage($"{menuName} is not assigned.");
            return;
        }

        SetMenuActive(menuPanel, true);
        SetMessage(menuName);
    }

    private static void SetMenuActive(GameObject panel, bool isActive)
    {
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
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

        SetUpgradeButton(healthUpgrade, healthUpgradeText, "Upgrade Base Health", PlayerProgression.UpgradeBaseHealthId, baseHealth);
        SetUnlockButton(healthRegenUnlockButton, healthRegenUnlockText, "Unlock Health Regeneration", PlayerProgression.UnlockHealthRegenId, healthRegenUnlock);
        SetUpgradeButton(speedUpgrade, speedUpgradeText, "Upgrade Base Speed", PlayerProgression.UpgradeBaseSpeedId, baseSpeed);
        SetUnlockButton(dashUnlockButton, dashUnlockText, "Unlock Dash", PlayerProgression.UnlockDashId, dashUnlock);
        SetUpgradeButton(cannonDamageUpgrade, cannonUpgradeText, "Upgrade Base Cannonball Damage", PlayerProgression.UpgradeBaseCannonDamageId, baseCannonDamage);
        SetUnlockButton(cannonballSizeUnlockButton, cannonballSizeUnlockText, "Unlock Cannonball Size", PlayerProgression.UnlockCannonballSizeId, cannonballSizeUnlock);
        SetUnlockButton(cannonballSpeedUnlockButton, cannonballSpeedUnlockText, "Unlock Cannonball Speed", PlayerProgression.UnlockCannonballSpeedId, cannonballSpeedUnlock);
        SetButtonInteractable(cannonballSpeedUpgrade, false);
        SetText(cannonballSpeedUpgrade, cannonballSpeedUpgradeText, progression.IsUnlocked(PlayerProgression.UnlockCannonballSpeedId) ? "Cannonball Speed: Unlocked" : "Unlock Cannonball Speed first");
        SetUnlockOrUpgradeButton(explodingCannonballsUnlockButton, explodingCannonballsUnlockText, "Unlock Cannonball Explosion", "Upgrade Cannonball Explosion Damage", PlayerProgression.UnlockCannonballExplosionId, explodingCannonballsUnlock, PlayerProgression.UpgradeExplosionPowerId, explosionPower);
        SetUpgradeButton(explosionPowerUpgradeButton, explosionPowerUpgradeText, "Upgrade Cannonball Explosion Damage", PlayerProgression.UpgradeExplosionPowerId, explosionPower, PlayerProgression.UnlockCannonballExplosionId);
        SetUnlockOrUpgradeButton(barnaclesUnlockButton, barnaclesUnlockText, "Unlock Barnacles", "Upgrade Barnacles Damage", PlayerProgression.UnlockBarnaclesId, barnaclesUnlock, PlayerProgression.UpgradeBarnaclePowerId, barnaclePower);
        SetUpgradeButton(barnaclePowerUpgradeButton, barnaclePowerUpgradeText, "Upgrade Barnacles Damage", PlayerProgression.UpgradeBarnaclePowerId, barnaclePower, PlayerProgression.UnlockBarnaclesId);
        SetUnlockButton(cannonballPierceUnlockButton, cannonballPierceUnlockText, "Unlock Cannonball Pierce", PlayerProgression.UnlockCannonballPierceId, cannonballPierceUnlock);
        SetUnlockButton(magnetUnlockButton, magnetUnlockText, "Unlock Magnet", PlayerProgression.UnlockMagnetId, magnetUnlock);
        SetUnlockOrUpgradeButton(forceFieldButton, forceFieldUnlockText, "Unlock Force Field", "Upgrade Force Field Damage", PlayerProgression.UnlockForceFieldId, forceFieldUnlock, PlayerProgression.UpgradeForceFieldDamageId, forceFieldDamage);
        SetUpgradeButton(forceFieldDamageUpgradeButton, forceFieldDamageUpgradeText, "Upgrade Force Field Damage", PlayerProgression.UpgradeForceFieldDamageId, forceFieldDamage, PlayerProgression.UnlockForceFieldId);
        SetUnlockOrUpgradeButton(cursedDoubloonsButton, cursedDoubloonsUnlockText, "Unlock Cursed Doubloons", "Upgrade Cursed Doubloons Damage", PlayerProgression.UnlockCursedDoubloonsId, cursedDoubloonsUnlock, PlayerProgression.UpgradeCursedDoubloonsDamageId, cursedDoubloonsDamage);
        SetUpgradeButton(cursedDoubloonsDamageUpgradeButton, cursedDoubloonsDamageUpgradeText, "Upgrade Cursed Doubloons Damage", PlayerProgression.UpgradeCursedDoubloonsDamageId, cursedDoubloonsDamage, PlayerProgression.UnlockCursedDoubloonsId);

        if (cosmeticStatusText != null)
        {
            int configuredSprites = cosmeticSprites == null ? 0 : cosmeticSprites.Length;
            int configuredIds = cosmeticIds == null ? 0 : cosmeticIds.Length;
            cosmeticStatusText.text = $"Selected Cosmetic: {progression.GetSelectedShipCosmeticId()}\nCosmetics Configured: {Mathf.Min(configuredIds, configuredSprites)}";
        }
    }


    private void RefreshCrewMenu()
    {
        if (!HasProgression()) return;

        if (selectedCrew == null)
        {
            SetCrewTextVisibility(true);
            SetCrewText(string.Empty, string.Empty, string.Empty, string.Empty, "No crew selected.");
            SetCrewPortrait(null);
            SetCrewScrollBackground(null);
            SetCrewHireButton(false, "Hire");
            return;
        }

        SetCrewScrollBackground(selectedCrew.HireScrollSprite);
        SetCrewTextVisibility(!hideTextWhenScrollHasBakedText || selectedCrew.HireScrollSprite == null);

        bool hasActiveSave = PlayerProgression.HasActiveSaveSlot;
        bool isUnlocked = hasActiveSave && progression.IsCrewUnlocked(selectedCrew.CrewId);
        int price = selectedCrew.Price;
        bool canAfford = hasActiveSave && progression.GetDoubloons() >= price;

        SetCrewText(
            selectedCrew.CrewName,
            selectedCrew.Description,
            selectedCrew.AbilityDescription,
            $"Price: {price} doubloons",
            GetCrewStatusText(hasActiveSave, isUnlocked, canAfford));
        SetCrewPortrait(selectedCrew.CrewSprite);
        SetCrewHireButton(hasActiveSave && !isUnlocked && canAfford, isUnlocked ? "Hired" : "Hire");
    }

    private void SetCrewText(string crewName, string description, string ability, string price, string status)
    {
        if (crewNameText != null) crewNameText.text = crewName;
        if (crewDescriptionText != null) crewDescriptionText.text = description;
        if (crewAbilityText != null) crewAbilityText.text = ability;
        if (crewPriceText != null) crewPriceText.text = price;
        if (crewStatusText != null) crewStatusText.text = status;
    }

    private void SetCrewPortrait(Sprite sprite)
    {
        if (crewPortraitImage == null) return;

        crewPortraitImage.sprite = sprite;
        crewPortraitImage.enabled = sprite != null;
    }

    private void SetCrewScrollBackground(Sprite sprite)
    {
        if (crewScrollBackgroundImage == null) return;

        Sprite backgroundSprite = sprite != null ? sprite : defaultCrewScrollSprite;
        crewScrollBackgroundImage.sprite = backgroundSprite;
        crewScrollBackgroundImage.enabled = backgroundSprite != null;
    }

    private void SetCrewTextVisibility(bool visible)
    {
        SetObjectActive(crewNameText, visible);
        SetObjectActive(crewDescriptionText, visible);
        SetObjectActive(crewAbilityText, visible);
        SetObjectActive(crewPriceText, visible);
        SetObjectActive(crewStatusText, visible);
    }

    private static void SetObjectActive(Component component, bool active)
    {
        if (component != null)
        {
            component.gameObject.SetActive(active);
        }
    }

    private void SetCrewHireButton(bool interactable, string label)
    {
        if (crewHireButton == null) return;

        crewHireButton.interactable = interactable;
        TMP_Text buttonText = crewHireButton.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.text = label;
        }
    }

    private static string GetCrewStatusText(bool hasActiveSave, bool isUnlocked, bool canAfford)
    {
        if (!hasActiveSave) return "No active save slot selected.";
        if (isUnlocked) return "Hired";
        return canAfford ? "Available to hire" : "Not enough doubloons";
    }


    private void SetUnlockOrUpgradeButton(MenuButtonReferences references, TMP_Text legacyText, string unlockLabel, string upgradeLabel, string unlockId, UnlockPurchaseConfig unlockConfig, string upgradeId, UpgradePurchaseConfig upgradeConfig)
    {
        if (!progression.IsUnlocked(unlockId))
        {
            SetUnlockButton(references, legacyText, unlockLabel, unlockId, unlockConfig);
            return;
        }

        SetUpgradeButton(references, legacyText, upgradeLabel, upgradeId, upgradeConfig, unlockId);
    }

    private void SetUpgradeButton(MenuButtonReferences references, TMP_Text legacyText, string label, string upgradeId, UpgradePurchaseConfig config, string requiredUnlockId = null)
    {
        config ??= new UpgradePurchaseConfig();
        int currentLevel = progression.GetUpgradeLevel(upgradeId);
        bool locked = !string.IsNullOrWhiteSpace(requiredUnlockId) && !progression.IsUnlocked(requiredUnlockId);
        bool maxed = IsAtMax(currentLevel, config.maxLevel);
        string status = locked ? "Locked" : maxed ? "Maxed" : $"Price: {GetUpgradeCost(config, currentLevel)}";
        string maxText = config.maxLevel > 0 ? $"/{config.maxLevel}" : string.Empty;
        string text = $"{label}\nLevel: {currentLevel}{maxText}\nIncrease: +{config.statIncreasePerLevel:0.##}\n{status}";

        SetText(references, legacyText, text);
        SetButtonInteractable(references, !locked && !maxed);
    }

    private void SetUnlockButton(MenuButtonReferences references, TMP_Text legacyText, string label, string unlockId, UnlockPurchaseConfig config)
    {
        bool unlocked = progression.IsUnlocked(unlockId);
        int cost = Mathf.Max(0, config != null ? config.cost : 0);
        string unlockedLabel = label.StartsWith("Unlock ", StringComparison.Ordinal) ? label.Substring("Unlock ".Length) : label;
        string text = unlocked ? $"{unlockedLabel}: Unlocked" : $"{label}\nPrice: {cost}\nStatus: Locked";

        SetText(references, legacyText, text);
        SetButtonInteractable(references, !unlocked);
    }

    private static void SetText(MenuButtonReferences references, TMP_Text legacyText, string text)
    {
        if (references != null && references.labelText != null)
        {
            references.labelText.text = text;
        }

        if (legacyText != null)
        {
            legacyText.text = text;
        }
    }

    private static void SetButtonInteractable(MenuButtonReferences references, bool interactable)
    {
        if (references != null && references.button != null)
        {
            references.button.interactable = interactable;
        }
    }
}
