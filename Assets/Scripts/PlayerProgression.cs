using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores save-slot data, currency, unlocks, upgrades, crew, cosmetics, and stage progression using PlayerPrefs.
/// </summary>
public class PlayerProgression : MonoBehaviour
{
    /// <summary>
    /// Provides display data for one save slot in the save selector UI.
    /// </summary>
    [Serializable]
    public class SaveSlotSummary
    {
        /// <summary>
        /// Save-slot identifier.
        /// </summary>
        public int slotId;
        /// <summary>
        /// Save-slot display name.
        /// </summary>
        public string saveName;
        /// <summary>
        /// Saved doubloon count for this slot.
        /// </summary>
        public int doubloons;
        /// <summary>
        /// Total saved upgrade count for this slot.
        /// </summary>
        public int upgradeCount;
        /// <summary>
        /// Total saved unlock count for this slot.
        /// </summary>
        public int unlockCount;
        /// <summary>
        /// Whether this save slot is currently active.
        /// </summary>
        public bool isActive;
    }

    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockHealthRegenId = "health_regen";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockDashId = "dash";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockMagnetId = "magnet";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockForceFieldId = "force_field";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockCannonballSizeId = "cannonball_size";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockCannonballSpeedId = "cannonball_speed";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockCannonballShootRateId = "cannonball_shoot_rate";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockCannonballExplosionId = "cannonball_explosion";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockCannonballPierceId = "cannonball_pierce";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockBarnaclesId = "barnacles";
    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockCursedDoubloonsId = "cursed_doubloons";

    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeBaseHealthId = "base_health";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeBaseSpeedId = "base_speed";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeBaseCannonDamageId = "base_cannon_damage";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeBaseCannonballSpeedId = "base_cannonball_speed";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeBaseMagnetRadiusId = "base_magnet_radius";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeExplosionPowerId = "explosion_power";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeBarnaclePowerId = "barnacle_power";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeCursedDoubloonsDamageId = "cursed_doubloons_damage";
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeForceFieldDamageId = "force_field_damage";

    /// <summary>
    /// Progression unlock identifier.
    /// </summary>
    public const string UnlockMagnetRadius = UnlockMagnetId;
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeHealthId = UpgradeBaseHealthId;
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeSpeedId = UpgradeBaseSpeedId;
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeMagnetRadiusId = UpgradeBaseMagnetRadiusId;
    /// <summary>
    /// Progression upgrade identifier.
    /// </summary>
    public const string UpgradeCannonDamageId = UpgradeBaseCannonDamageId;

    private const string LegacyHasSaveFileKey = "HasSaveFile";
    private const string SaveSlotIdsKey = "SaveSlotIds";
    private const string ActiveSaveSlotIdKey = "ActiveSaveSlotId";
    private const string NextSaveSlotIdKey = "NextSaveSlotId";
    private const string SaveSlotPrefix = "SaveSlot_";
    private const int NoActiveSaveSlot = -1;

    private const string TotalDoubloonsKey = "Doubloons";
    private const string SelectedShipCosmeticIdKey = "SelectedShipCosmeticId";
    private const string PermanentHealthLevelKey = "PermanentHealthLevel";
    private const string PermanentSpeedLevelKey = "PermanentSpeedLevel";
    private const string PermanentMagnetLevelKey = "PermanentMagnetLevel";
    private const string PermanentCannonDamageLevelKey = "PermanentCannonDamageLevel";
    private const string DashUnlockedKey = "DashUnlocked";
    private const string ForceFieldUnlockedKey = "ForceFieldUnlocked";
    private const string OwnedCosmeticsKey = "OwnedCosmetics";
    private const string GenericUnlockIdsKey = "GenericUnlockIds";
    private const string GenericUpgradeIdsKey = "GenericUpgradeIds";
    private const string CrewUnlockIdsKey = "CrewUnlockIds";
    private const string GenericUnlockKeyPrefix = "Unlock_";
    private const string GenericUpgradeKeyPrefix = "Upgrade_";
    private const string CrewUnlockKeyPrefix = "CrewUnlock_";
    private const string SaveNameKey = "Name";
    private const string HighestUnlockedStageKey = "HighestUnlockedStage";
    private const string CompletedStagesKey = "CompletedStages";
    private const string CompletedStageKeyPrefix = "CompletedStage_";
    private const int FirstStageNumber = 1;

    private static readonly string[] BuiltInGenericUnlockIds =
    {
        UnlockMagnetRadius,
        UnlockDashId,
        UnlockForceFieldId,
        UnlockHealthRegenId,
        UnlockCannonballSizeId,
        UnlockCannonballSpeedId,
        UnlockCannonballShootRateId,
        UnlockCannonballPierceId,
        UnlockCannonballExplosionId,
        UnlockBarnaclesId,
        UnlockCursedDoubloonsId
    };

    private static readonly string[] BuiltInGenericUpgradeIds =
    {
        UpgradeHealthId,
        UpgradeSpeedId,
        UpgradeMagnetRadiusId,
        UpgradeCannonDamageId,
        UpgradeBaseCannonballSpeedId,
        UpgradeExplosionPowerId,
        UpgradeBarnaclePowerId,
        UpgradeCursedDoubloonsDamageId,
        UpgradeForceFieldDamageId
    };

    private static PlayerProgression instance;

    private int totalDoubloons;
    private string selectedShipCosmeticId = "default";
    private int permanentHealthLevel;
    private int permanentSpeedLevel;
    private int permanentMagnetLevel;
    private int permanentCannonDamageLevel;
    private bool dashUnlocked;
    private bool forceFieldUnlocked;
    private readonly HashSet<string> ownedCosmetics = new();
    private readonly HashSet<string> genericUnlockIds = new();
    private readonly Dictionary<string, int> genericUpgradeLevels = new();
    private readonly HashSet<string> crewUnlockIds = new();
    private readonly HashSet<int> completedStages = new();
    private int highestUnlockedStage = FirstStageNumber;

    /// <summary>
    /// Gets the persistent progression singleton, creating one if no scene instance exists.
    /// </summary>
    public static PlayerProgression Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            GameObject progressionObject = new("PlayerProgression");
            instance = progressionObject.AddComponent<PlayerProgression>();
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    /// <summary>
    /// Gets the currently selected save-slot identifier, or -1 when no slot is active.
    /// </summary>
    public static int ActiveSaveSlotId => PlayerPrefs.GetInt(ActiveSaveSlotIdKey, NoActiveSaveSlot);
    /// <summary>
    /// Gets whether the active save-slot id points to an existing save slot.
    /// </summary>
    public static bool HasActiveSaveSlot => SlotExists(ActiveSaveSlotId);
    /// <summary>
    /// Returns whether any save slot exists.
    /// </summary>
    /// <returns>True when at least one save slot is stored.</returns>
    public static bool HasSaveFile() => GetSaveSlotIds().Count > 0;
    /// <summary>
    /// Gets the display name for the active save slot.
    /// </summary>
    /// <returns>The requested string value.</returns>
    public static string GetActiveSaveName() => HasActiveSaveSlot ? GetSaveSlotName(ActiveSaveSlotId) : "No Active Save";

    /// <summary>
    /// Ensures a save slot exists and updates the legacy save-file flag.
    /// </summary>
    public static void MarkSaveExists()
    {
        EnsureActiveSaveSlot(true);
        PlayerPrefs.SetInt(LegacyHasSaveFileKey, HasSaveFile() ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Creates a new save slot, makes it active, and initializes default progression.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public static int CreateNewSaveSlot()
    {
        int slotId = Mathf.Max(0, PlayerPrefs.GetInt(NextSaveSlotIdKey, 0));
        while (SlotExists(slotId))
        {
            slotId++;
        }

        List<int> slotIds = GetSaveSlotIds();
        slotIds.Add(slotId);
        PlayerPrefs.SetString(SaveSlotIdsKey, JoinIds(slotIds));
        PlayerPrefs.SetInt(NextSaveSlotIdKey, slotId + 1);
        PlayerPrefs.SetString(GetSlotKey(slotId, SaveNameKey), $"Save {slotIds.Count}");
        PlayerPrefs.SetInt(ActiveSaveSlotIdKey, slotId);
        PlayerPrefs.SetInt(LegacyHasSaveFileKey, 1);

        if (instance != null)
        {
            instance.ResetCachedProgression();
            instance.Save();
        }
        else
        {
            SaveDefaultProgressionForSlot(slotId);
            PlayerPrefs.Save();
        }

        Debug.Log($"PlayerProgression: Created save slot {slotId} named '{GetSaveSlotName(slotId)}' and made it active.");
        return slotId;
    }

    /// <summary>
    /// Selects an existing save slot and loads its progression when available.
    /// </summary>
    /// <param name="slotId">Save-slot identifier.</param>
    /// <returns>True if the slot exists and was activated; otherwise false.</returns>
    public static bool SetActiveSaveSlot(int slotId)
    {
        if (!SlotExists(slotId))
        {
            Debug.LogWarning($"PlayerProgression: Cannot load missing save slot {slotId}.");
            return false;
        }

        PlayerPrefs.SetInt(ActiveSaveSlotIdKey, slotId);
        PlayerPrefs.SetInt(LegacyHasSaveFileKey, 1);
        PlayerPrefs.Save();

        if (instance != null)
        {
            instance.Load();
        }

        Debug.Log($"PlayerProgression: Loaded save slot {slotId} named '{GetSaveSlotName(slotId)}'.");
        return true;
    }

    /// <summary>
    /// Renames an existing save slot.
    /// </summary>
    /// <param name="slotId">Save-slot identifier.</param>
    /// <param name="newName">New display name.</param>
    /// <returns>True if the slot exists and was renamed; otherwise false.</returns>
    public static bool RenameSaveSlot(int slotId, string newName)
    {
        if (!SlotExists(slotId))
        {
            Debug.LogWarning($"PlayerProgression: Cannot rename missing save slot {slotId}.");
            return false;
        }

        string cleanedName = string.IsNullOrWhiteSpace(newName) ? GetSaveSlotName(slotId) : newName.Trim();
        PlayerPrefs.SetString(GetSlotKey(slotId, SaveNameKey), cleanedName);
        PlayerPrefs.Save();
        Debug.Log($"PlayerProgression: Renamed save slot {slotId} to '{cleanedName}'.");
        return true;
    }

    /// <summary>
    /// Deletes an existing save slot and selects another slot when needed.
    /// </summary>
    /// <param name="slotId">Save-slot identifier.</param>
    /// <returns>True if the slot existed and was deleted; otherwise false.</returns>
    public static bool DeleteSaveSlot(int slotId)
    {
        if (!SlotExists(slotId))
        {
            Debug.LogWarning($"PlayerProgression: Cannot delete missing save slot {slotId}.");
            return false;
        }

        DeleteSlotProgression(slotId);

        List<int> slotIds = GetSaveSlotIds();
        slotIds.Remove(slotId);
        PlayerPrefs.SetString(SaveSlotIdsKey, JoinIds(slotIds));

        if (ActiveSaveSlotId == slotId)
        {
            if (slotIds.Count > 0)
            {
                PlayerPrefs.SetInt(ActiveSaveSlotIdKey, slotIds[0]);
                if (instance != null)
                {
                    instance.Load();
                }
            }
            else
            {
                PlayerPrefs.DeleteKey(ActiveSaveSlotIdKey);
                PlayerPrefs.SetInt(LegacyHasSaveFileKey, 0);
                if (instance != null)
                {
                    instance.ResetCachedProgression();
                }
            }
        }

        PlayerPrefs.Save();
        Debug.Log($"PlayerProgression: Deleted save slot {slotId}.");
        return true;
    }

    /// <summary>
    /// Builds summaries for every stored save slot.
    /// </summary>
    /// <returns>The requested list.</returns>
    public static List<SaveSlotSummary> GetAllSaveSlotSummaries()
    {
        List<SaveSlotSummary> summaries = new();
        List<int> slotIds = GetSaveSlotIds();
        int activeSlotId = ActiveSaveSlotId;

        foreach (int slotId in slotIds)
        {
            summaries.Add(new SaveSlotSummary
            {
                slotId = slotId,
                saveName = GetSaveSlotName(slotId),
                doubloons = Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, TotalDoubloonsKey), 0)),
                upgradeCount = GetTotalUpgradeCount(slotId),
                unlockCount = GetUnlockCount(slotId),
                isActive = slotId == activeSlotId
            });
        }

        return summaries;
    }

    /// <summary>
    /// Clears progression data for the active save slot and writes default values.
    /// </summary>
    public static void ResetAllProgression()
    {
        int slotId = EnsureActiveSaveSlot(true);
        DeleteSlotProgressionData(slotId);

        if (instance != null)
        {
            instance.ResetCachedProgression();
            instance.Save();
        }
        else
        {
            SaveDefaultProgressionForSlot(slotId);
            PlayerPrefs.Save();
        }

        Debug.Log($"PlayerProgression: Reset progression for active save slot {slotId}.");
    }

    /// <summary>
    /// Saves the current singleton progression data to the active save slot.
    /// </summary>
    public static void SaveActiveSlot() => Instance.Save();

    /// <summary>
    /// Gets the current saved doubloon total.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetDoubloons() => totalDoubloons;

    /// <summary>
    /// Adds positive doubloons to the active progression and saves.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    public void AddDoubloons(int amount)
    {
        if (amount <= 0) return;
        totalDoubloons += amount;
        Save();
    }

    /// <summary>
    /// Attempts to spend doubloons from the active progression.
    /// </summary>
    /// <param name="amount">Amount to apply.</param>
    /// <returns>True when the amount can be spent or is non-positive; otherwise false.</returns>
    public bool SpendDoubloons(int amount)
    {
        if (!HasActiveSaveSlot) return false;
        if (amount <= 0) return true;
        if (totalDoubloons < amount) return false;

        totalDoubloons -= amount;
        Save();
        return true;
    }

    /// <summary>
    /// Checks whether a stage number is currently available.
    /// </summary>
    /// <param name="stageNumber">One-based stage number.</param>
    /// <returns>True when the stage is within the unlocked range.</returns>
    public bool IsStageUnlocked(int stageNumber)
    {
        return stageNumber >= FirstStageNumber && stageNumber <= GetHighestUnlockedStage();
    }

    /// <summary>
    /// Raises the highest unlocked stage to include the provided stage number.
    /// </summary>
    /// <param name="stageNumber">One-based stage number.</param>
    public void UnlockStage(int stageNumber)
    {
        if (stageNumber < FirstStageNumber) return;

        highestUnlockedStage = Mathf.Max(GetHighestUnlockedStage(), stageNumber);
        Save();
    }

    /// <summary>
    /// Gets the highest unlocked stage, including any stored PlayerPrefs value.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetHighestUnlockedStage()
    {
        highestUnlockedStage = Mathf.Max(FirstStageNumber, highestUnlockedStage);

        if (HasActiveSaveSlot)
        {
            highestUnlockedStage = Mathf.Max(
                highestUnlockedStage,
                PlayerPrefs.GetInt(GetActiveKey(HighestUnlockedStageKey), FirstStageNumber));
        }

        return highestUnlockedStage;
    }

    /// <summary>
    /// Marks a stage complete and unlocks the next stage.
    /// </summary>
    /// <param name="stageNumber">One-based stage number.</param>
    public void CompleteStage(int stageNumber)
    {
        if (stageNumber < FirstStageNumber) return;

        completedStages.Add(stageNumber);
        highestUnlockedStage = Mathf.Max(GetHighestUnlockedStage(), stageNumber + 1);
        Save();
    }

    /// <summary>
    /// Gets whether the dash unlock is currently owned.
    /// </summary>
    /// <returns>True when dash is unlocked.</returns>
    public bool IsDashUnlocked() => IsUnlocked(UnlockDashId) || dashUnlocked;
    /// <summary>
    /// Gets whether the force-field unlock is currently owned.
    /// </summary>
    /// <returns>True when force field is unlocked.</returns>
    public bool IsForceFieldUnlocked() => IsUnlocked(UnlockForceFieldId) || forceFieldUnlocked;

    /// <summary>
    /// Unlocks dash and saves the progression state.
    /// </summary>
    public void UnlockDash()
    {
        dashUnlocked = true;
        Unlock(UnlockDashId);
    }

    /// <summary>
    /// Unlocks force field and saves the progression state.
    /// </summary>
    public void UnlockForceField()
    {
        forceFieldUnlocked = true;
        Unlock(UnlockForceFieldId);
    }

    /// <summary>
    /// Gets the legacy permanent health upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetPermanentHealthLevel() => permanentHealthLevel;
    /// <summary>
    /// Gets the legacy permanent speed upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetPermanentSpeedLevel() => permanentSpeedLevel;
    /// <summary>
    /// Gets the legacy permanent magnet upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetPermanentMagnetLevel() => permanentMagnetLevel;
    /// <summary>
    /// Gets the legacy permanent cannon-damage upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetPermanentCannonDamageLevel() => permanentCannonDamageLevel;

    /// <summary>
    /// Attempts to buy one permanent health upgrade level.
    /// </summary>
    /// <param name="cost">Doubloon cost.</param>
    /// <returns>True if the purchase succeeded; otherwise false.</returns>
    public bool BuyPermanentHealthUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentHealthLevel++;
        SetUpgradeLevel(UpgradeHealthId, permanentHealthLevel);
        return true;
    }

    /// <summary>
    /// Attempts to buy one permanent speed upgrade level.
    /// </summary>
    /// <param name="cost">Doubloon cost.</param>
    /// <returns>True if the purchase succeeded; otherwise false.</returns>
    public bool BuyPermanentSpeedUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentSpeedLevel++;
        SetUpgradeLevel(UpgradeSpeedId, permanentSpeedLevel);
        return true;
    }

    /// <summary>
    /// Attempts to buy one permanent magnet upgrade level and unlock magnet support.
    /// </summary>
    /// <param name="cost">Doubloon cost.</param>
    /// <returns>True if the purchase succeeded; otherwise false.</returns>
    public bool BuyPermanentMagnetUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentMagnetLevel++;
        Unlock(UnlockMagnetRadius);
        SetUpgradeLevel(UpgradeMagnetRadiusId, permanentMagnetLevel);
        return true;
    }

    /// <summary>
    /// Attempts to buy one permanent cannon-damage upgrade level.
    /// </summary>
    /// <param name="cost">Doubloon cost.</param>
    /// <returns>True if the purchase succeeded; otherwise false.</returns>
    public bool BuyPermanentCannonDamageUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentCannonDamageLevel++;
        SetUpgradeLevel(UpgradeCannonDamageId, permanentCannonDamageLevel);
        return true;
    }

    /// <summary>
    /// Gets the purchased base cannonball-speed upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetBaseCannonballSpeedLevel() => GetUpgradeLevel(UpgradeBaseCannonballSpeedId);
    /// <summary>
    /// Gets the purchased base magnet-radius upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetBaseMagnetRadiusLevel() => GetUpgradeLevel(UpgradeBaseMagnetRadiusId);
    /// <summary>
    /// Gets the purchased explosion-power upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetExplosionPowerLevel() => GetUpgradeLevel(UpgradeExplosionPowerId);
    /// <summary>
    /// Gets the purchased barnacle-power upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetBarnaclePowerLevel() => GetUpgradeLevel(UpgradeBarnaclePowerId);
    /// <summary>
    /// Gets the purchased cursed-doubloons damage upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetCursedDoubloonsDamageLevel() => GetUpgradeLevel(UpgradeCursedDoubloonsDamageId);
    /// <summary>
    /// Gets the purchased force-field damage upgrade level.
    /// </summary>
    /// <returns>The requested integer value.</returns>
    public int GetForceFieldDamageLevel() => GetUpgradeLevel(UpgradeForceFieldDamageId);

    /// <summary>
    /// Attempts to buy a named unlock for the active save slot.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <param name="cost">Doubloon cost.</param>
    /// <returns>True if the unlock was purchased; otherwise false.</returns>
    public bool TryPurchaseUnlock(string id, int cost)
    {
        if (!HasActiveSaveSlot) return false;

        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId) || IsUnlocked(normalizedId)) return false;
        if (!SpendDoubloons(cost)) return false;

        Unlock(normalizedId);
        return true;
    }

    /// <summary>
    /// Attempts to buy one level of a named upgrade.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <param name="cost">Doubloon cost.</param>
    /// <param name="maxLevel">Maximum allowed level; values less than or equal to zero are uncapped.</param>
    /// <param name="requiredUnlockId">Optional unlock identifier required before purchase.</param>
    /// <returns>True if an upgrade level was purchased; otherwise false.</returns>
    public bool TryPurchaseUpgrade(string id, int cost, int maxLevel, string requiredUnlockId = null)
    {
        if (!HasActiveSaveSlot) return false;

        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId)) return false;
        if (!string.IsNullOrWhiteSpace(requiredUnlockId) && !IsUnlocked(requiredUnlockId)) return false;
        int currentLevel = GetUpgradeLevel(normalizedId);
        if (maxLevel > 0 && currentLevel >= maxLevel) return false;
        if (!SpendDoubloons(cost)) return false;

        SetUpgradeLevel(normalizedId, currentLevel + 1);
        return true;
    }

    /// <summary>
    /// Checks whether a named generic unlock is owned.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <returns>True when the unlock id is stored.</returns>
    public bool IsUnlocked(string id)
    {
        string normalizedId = NormalizeId(id);
        return !string.IsNullOrEmpty(normalizedId) && genericUnlockIds.Contains(normalizedId);
    }

    /// <summary>
    /// Stores a named generic unlock and saves progression.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    public void Unlock(string id)
    {
        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId)) return;

        genericUnlockIds.Add(normalizedId);
        if (normalizedId == UnlockDashId) dashUnlocked = true;
        if (normalizedId == UnlockForceFieldId) forceFieldUnlocked = true;
        Save();
    }

    /// <summary>
    /// Removes a named generic unlock from the active progression.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    public void Lock(string id)
    {
        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId)) return;

        genericUnlockIds.Remove(normalizedId);
        DeleteActiveKey(GenericUnlockKeyPrefix + normalizedId);
        if (normalizedId == UnlockDashId) dashUnlocked = false;
        if (normalizedId == UnlockForceFieldId) forceFieldUnlocked = false;
        Save();
    }

    /// <summary>
    /// Gets the stored level for a named generic upgrade.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <returns>The requested integer value.</returns>
    public int GetUpgradeLevel(string id)
    {
        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId)) return 0;

        return genericUpgradeLevels.TryGetValue(normalizedId, out int level) ? Mathf.Max(0, level) : 0;
    }

    /// <summary>
    /// Sets the stored level for a named generic upgrade and saves progression.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <param name="level">Parameter used by this method.</param>
    public void SetUpgradeLevel(string id, int level)
    {
        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId)) return;

        int clampedLevel = Mathf.Max(0, level);
        if (clampedLevel == 0)
        {
            genericUpgradeLevels.Remove(normalizedId);
            DeleteActiveKey(GenericUpgradeKeyPrefix + normalizedId);
        }
        else
        {
            genericUpgradeLevels[normalizedId] = clampedLevel;
        }

        SyncLegacyUpgradeLevel(normalizedId, clampedLevel);
        Save();
    }

    /// <summary>
    /// Adds to a named generic upgrade level and saves progression.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <param name="amount">Amount to apply.</param>
    public void AddUpgradeLevel(string id, int amount = 1)
    {
        if (amount == 0) return;
        SetUpgradeLevel(id, Mathf.Max(0, GetUpgradeLevel(id) + amount));
    }

    /// <summary>
    /// Checks whether a crew member has been hired or unlocked.
    /// </summary>
    /// <param name="crewId">Crew identifier.</param>
    /// <returns>True when the crew id is stored.</returns>
    public bool IsCrewUnlocked(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        return !string.IsNullOrEmpty(normalizedId) && crewUnlockIds.Contains(normalizedId);
    }

    /// <summary>
    /// Stores a crew unlock for the active save slot.
    /// </summary>
    /// <param name="crewId">Crew identifier.</param>
    public void UnlockCrew(string crewId)
    {
        if (!HasActiveSaveSlot) return;

        string normalizedId = NormalizeId(crewId);
        if (string.IsNullOrEmpty(normalizedId)) return;

        crewUnlockIds.Add(normalizedId);
        Save();
    }

    /// <summary>
    /// Gets all unlocked crew identifiers in sorted order.
    /// </summary>
    /// <returns>The requested list.</returns>
    public List<string> GetUnlockedCrewIds()
    {
        List<string> unlockedCrewIds = new(crewUnlockIds);
        unlockedCrewIds.Sort();
        return unlockedCrewIds;
    }

    /// <summary>
    /// Gets the currently selected ship cosmetic id.
    /// </summary>
    /// <returns>The requested string value.</returns>
    public string GetSelectedShipCosmeticId() => selectedShipCosmeticId;

    /// <summary>
    /// Selects a ship cosmetic and records it as owned.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    public void SetSelectedShipCosmeticId(string id)
    {
        selectedShipCosmeticId = string.IsNullOrWhiteSpace(id) ? "default" : id;
        if (!ownedCosmetics.Contains(selectedShipCosmeticId))
        {
            ownedCosmetics.Add(selectedShipCosmeticId);
        }
        Save();
    }

    /// <summary>
    /// Checks whether a ship cosmetic is owned.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <returns>True for blank/default ids or owned cosmetics.</returns>
    public bool IsCosmeticOwned(string id)
    {
        return string.IsNullOrWhiteSpace(id) || id == "default" || ownedCosmetics.Contains(id);
    }

    /// <summary>
    /// Attempts to buy a ship cosmetic with doubloons.
    /// </summary>
    /// <param name="id">Progression identifier.</param>
    /// <param name="cost">Doubloon cost.</param>
    /// <returns>True if the cosmetic is already available or was purchased; otherwise false.</returns>
    public bool BuyCosmetic(string id, int cost)
    {
        if (string.IsNullOrWhiteSpace(id) || id == "default") return true;
        if (ownedCosmetics.Contains(id)) return true;
        if (!SpendDoubloons(cost)) return false;

        ownedCosmetics.Add(id);
        Save();
        return true;
    }

    /// <summary>
    /// Writes the current progression state to PlayerPrefs for the active save slot.
    /// </summary>
    public void Save()
    {
        int slotId = EnsureActiveSaveSlot(true);
        PlayerPrefs.SetInt(GetSlotKey(slotId, TotalDoubloonsKey), totalDoubloons);
        PlayerPrefs.SetString(GetSlotKey(slotId, SelectedShipCosmeticIdKey), selectedShipCosmeticId);
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentHealthLevelKey), permanentHealthLevel);
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentSpeedLevelKey), permanentSpeedLevel);
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentMagnetLevelKey), permanentMagnetLevel);
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentCannonDamageLevelKey), permanentCannonDamageLevel);
        PlayerPrefs.SetInt(GetSlotKey(slotId, DashUnlockedKey), IsDashUnlocked() ? 1 : 0);
        PlayerPrefs.SetInt(GetSlotKey(slotId, ForceFieldUnlockedKey), IsForceFieldUnlocked() ? 1 : 0);
        PlayerPrefs.SetString(GetSlotKey(slotId, OwnedCosmeticsKey), string.Join(",", ownedCosmetics));
        SaveCrewUnlocks(slotId);
        SaveStageProgression(slotId);
        SaveGenericProgression(slotId);
        PlayerPrefs.SetInt(LegacyHasSaveFileKey, 1);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads the active save slot from PlayerPrefs into the cached progression state.
    /// </summary>
    public void Load()
    {
        if (!HasActiveSaveSlot)
        {
            ResetCachedProgression();
            return;
        }

        int slotId = ActiveSaveSlotId;
        totalDoubloons = Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, TotalDoubloonsKey), 0));
        selectedShipCosmeticId = PlayerPrefs.GetString(GetSlotKey(slotId, SelectedShipCosmeticIdKey), "default");
        permanentHealthLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, PermanentHealthLevelKey), 0));
        permanentSpeedLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, PermanentSpeedLevelKey), 0));
        permanentMagnetLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, PermanentMagnetLevelKey), 0));
        permanentCannonDamageLevel = Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, PermanentCannonDamageLevelKey), 0));
        dashUnlocked = PlayerPrefs.GetInt(GetSlotKey(slotId, DashUnlockedKey), 0) == 1;
        forceFieldUnlocked = PlayerPrefs.GetInt(GetSlotKey(slotId, ForceFieldUnlockedKey), 0) == 1;

        LoadOwnedCosmetics(slotId);
        LoadCrewUnlocks(slotId);
        LoadStageProgression(slotId);
        LoadGenericProgression(slotId);
        SyncGenericUpgradeLevelsIntoLegacyFields();
        MigrateLegacyProgressionIntoGenericKeys();
    }

    private static int EnsureActiveSaveSlot(bool createIfMissing)
    {
        int slotId = ActiveSaveSlotId;
        if (SlotExists(slotId))
        {
            return slotId;
        }

        List<int> slotIds = GetSaveSlotIds();
        if (slotIds.Count > 0)
        {
            PlayerPrefs.SetInt(ActiveSaveSlotIdKey, slotIds[0]);
            return slotIds[0];
        }

        return createIfMissing ? CreateNewSaveSlotWithoutInstanceSave() : NoActiveSaveSlot;
    }

    private static int CreateNewSaveSlotWithoutInstanceSave()
    {
        int slotId = Mathf.Max(0, PlayerPrefs.GetInt(NextSaveSlotIdKey, 0));
        while (SlotExists(slotId))
        {
            slotId++;
        }

        List<int> slotIds = GetSaveSlotIds();
        slotIds.Add(slotId);
        PlayerPrefs.SetString(SaveSlotIdsKey, JoinIds(slotIds));
        PlayerPrefs.SetInt(NextSaveSlotIdKey, slotId + 1);
        PlayerPrefs.SetString(GetSlotKey(slotId, SaveNameKey), $"Save {slotIds.Count}");
        PlayerPrefs.SetInt(ActiveSaveSlotIdKey, slotId);
        PlayerPrefs.SetInt(LegacyHasSaveFileKey, 1);
        Debug.Log($"PlayerProgression: Created save slot {slotId} named '{GetSaveSlotName(slotId)}' and made it active.");
        return slotId;
    }

    private static bool SlotExists(int slotId) => slotId >= 0 && GetSaveSlotIds().Contains(slotId);

    private static List<int> GetSaveSlotIds()
    {
        List<int> slotIds = new();
        string rawIds = PlayerPrefs.GetString(SaveSlotIdsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(rawIds))
        {
            return slotIds;
        }

        string[] ids = rawIds.Split(',');
        for (int i = 0; i < ids.Length; i++)
        {
            if (int.TryParse(ids[i], out int slotId) && slotId >= 0 && !slotIds.Contains(slotId))
            {
                slotIds.Add(slotId);
            }
        }

        return slotIds;
    }

    private static string JoinIds(List<int> slotIds)
    {
        List<string> idStrings = new();
        foreach (int slotId in slotIds)
        {
            idStrings.Add(slotId.ToString());
        }

        return string.Join(",", idStrings);
    }

    private static string GetSaveSlotName(int slotId)
    {
        return PlayerPrefs.GetString(GetSlotKey(slotId, SaveNameKey), $"Save {slotId + 1}");
    }

    private static string GetActiveKey(string key) => GetSlotKey(EnsureActiveSaveSlot(true), key);
    private static string GetSlotKey(int slotId, string key) => $"{SaveSlotPrefix}{slotId}_{key}";
    private static void DeleteActiveKey(string key) => PlayerPrefs.DeleteKey(GetActiveKey(key));

    private static void SaveDefaultProgressionForSlot(int slotId)
    {
        PlayerPrefs.SetInt(GetSlotKey(slotId, TotalDoubloonsKey), 0);
        PlayerPrefs.SetString(GetSlotKey(slotId, SelectedShipCosmeticIdKey), "default");
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentHealthLevelKey), 0);
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentSpeedLevelKey), 0);
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentMagnetLevelKey), 0);
        PlayerPrefs.SetInt(GetSlotKey(slotId, PermanentCannonDamageLevelKey), 0);
        PlayerPrefs.SetInt(GetSlotKey(slotId, DashUnlockedKey), 0);
        PlayerPrefs.SetInt(GetSlotKey(slotId, ForceFieldUnlockedKey), 0);
        PlayerPrefs.SetString(GetSlotKey(slotId, OwnedCosmeticsKey), "default");
        PlayerPrefs.SetInt(GetSlotKey(slotId, HighestUnlockedStageKey), FirstStageNumber);
        PlayerPrefs.SetString(GetSlotKey(slotId, CompletedStagesKey), string.Empty);
        PlayerPrefs.SetString(GetSlotKey(slotId, GenericUnlockIdsKey), string.Empty);
        PlayerPrefs.SetString(GetSlotKey(slotId, GenericUpgradeIdsKey), string.Empty);
        PlayerPrefs.SetString(GetSlotKey(slotId, CrewUnlockIdsKey), string.Empty);
    }

    private static void DeleteSlotProgression(int slotId)
    {
        DeleteSlotProgressionData(slotId);
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, SaveNameKey));
    }

    private static void DeleteSlotProgressionData(int slotId)
    {
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, TotalDoubloonsKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, SelectedShipCosmeticIdKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, PermanentHealthLevelKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, PermanentSpeedLevelKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, PermanentMagnetLevelKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, PermanentCannonDamageLevelKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, DashUnlockedKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, ForceFieldUnlockedKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, OwnedCosmeticsKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, HighestUnlockedStageKey));
        DeleteCompletedStageKeys(slotId);
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, CompletedStagesKey));

        DeleteGenericKeys(slotId, GenericUnlockIdsKey, GenericUnlockKeyPrefix, BuiltInGenericUnlockIds);
        DeleteGenericKeys(slotId, GenericUpgradeIdsKey, GenericUpgradeKeyPrefix, BuiltInGenericUpgradeIds);
        DeleteCrewUnlockKeys(slotId);

        PlayerPrefs.DeleteKey(GetSlotKey(slotId, GenericUnlockIdsKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, GenericUpgradeIdsKey));
        PlayerPrefs.DeleteKey(GetSlotKey(slotId, CrewUnlockIdsKey));
    }

    private static void DeleteGenericKeys(int slotId, string listKey, string keyPrefix, string[] builtInIds)
    {
        HashSet<string> idsToDelete = new();
        AddCsvIds(idsToDelete, PlayerPrefs.GetString(GetSlotKey(slotId, listKey), string.Empty));
        for (int i = 0; i < builtInIds.Length; i++)
        {
            idsToDelete.Add(NormalizeId(builtInIds[i]));
        }

        foreach (string id in idsToDelete)
        {
            if (!string.IsNullOrEmpty(id))
            {
                PlayerPrefs.DeleteKey(GetSlotKey(slotId, keyPrefix + id));
            }
        }
    }

    private static int GetUnlockCount(int slotId)
    {
        HashSet<string> unlockIds = new();
        AddCsvIds(unlockIds, PlayerPrefs.GetString(GetSlotKey(slotId, GenericUnlockIdsKey), string.Empty));
        return unlockIds.Count;
    }

    private static int GetTotalUpgradeCount(int slotId)
    {
        int total = 0;
        HashSet<string> upgradeIds = new();
        AddCsvIds(upgradeIds, PlayerPrefs.GetString(GetSlotKey(slotId, GenericUpgradeIdsKey), string.Empty));
        for (int i = 0; i < BuiltInGenericUpgradeIds.Length; i++)
        {
            upgradeIds.Add(NormalizeId(BuiltInGenericUpgradeIds[i]));
        }
        upgradeIds.Add("health");
        upgradeIds.Add("speed");
        upgradeIds.Add("cannon_damage");
        upgradeIds.Add("magnet_radius");

        foreach (string id in upgradeIds)
        {
            total += Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, GenericUpgradeKeyPrefix + id), 0));
        }

        return total;
    }

    private static void AddCsvIds(HashSet<string> target, string rawIds)
    {
        if (string.IsNullOrWhiteSpace(rawIds)) return;

        string[] ids = rawIds.Split(',');
        for (int i = 0; i < ids.Length; i++)
        {
            string normalizedId = NormalizeId(ids[i]);
            if (!string.IsNullOrEmpty(normalizedId))
            {
                target.Add(normalizedId);
            }
        }
    }

    private static string NormalizeId(string id) => string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim().ToLowerInvariant();

    private void ResetCachedProgression()
    {
        totalDoubloons = 0;
        selectedShipCosmeticId = "default";
        permanentHealthLevel = 0;
        permanentSpeedLevel = 0;
        permanentMagnetLevel = 0;
        permanentCannonDamageLevel = 0;
        dashUnlocked = false;
        forceFieldUnlocked = false;
        ownedCosmetics.Clear();
        ownedCosmetics.Add("default");
        genericUnlockIds.Clear();
        genericUpgradeLevels.Clear();
        crewUnlockIds.Clear();
        completedStages.Clear();
        highestUnlockedStage = FirstStageNumber;
    }

    private void LoadOwnedCosmetics(int slotId)
    {
        ownedCosmetics.Clear();
        string ownedCosmeticsRaw = PlayerPrefs.GetString(GetSlotKey(slotId, OwnedCosmeticsKey), "default");
        string[] ids = ownedCosmeticsRaw.Split(',');
        for (int i = 0; i < ids.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(ids[i]))
            {
                ownedCosmetics.Add(ids[i]);
            }
        }

        if (string.IsNullOrWhiteSpace(selectedShipCosmeticId))
        {
            selectedShipCosmeticId = "default";
        }

        ownedCosmetics.Add("default");
    }


    private void LoadCrewUnlocks(int slotId)
    {
        crewUnlockIds.Clear();
        AddCsvIds(crewUnlockIds, PlayerPrefs.GetString(GetSlotKey(slotId, CrewUnlockIdsKey), string.Empty));
    }

    private void SaveCrewUnlocks(int slotId)
    {
        PlayerPrefs.SetString(GetSlotKey(slotId, CrewUnlockIdsKey), string.Join(",", crewUnlockIds));
        foreach (string crewId in crewUnlockIds)
        {
            PlayerPrefs.SetInt(GetSlotKey(slotId, CrewUnlockKeyPrefix + crewId), 1);
        }
    }

    private static void DeleteCrewUnlockKeys(int slotId)
    {
        HashSet<string> idsToDelete = new();
        AddCsvIds(idsToDelete, PlayerPrefs.GetString(GetSlotKey(slotId, CrewUnlockIdsKey), string.Empty));

        foreach (string crewId in idsToDelete)
        {
            if (!string.IsNullOrEmpty(crewId))
            {
                PlayerPrefs.DeleteKey(GetSlotKey(slotId, CrewUnlockKeyPrefix + crewId));
            }
        }
    }

    private void LoadStageProgression(int slotId)
    {
        highestUnlockedStage = Mathf.Max(FirstStageNumber, PlayerPrefs.GetInt(GetSlotKey(slotId, HighestUnlockedStageKey), FirstStageNumber));
        completedStages.Clear();

        string rawStages = PlayerPrefs.GetString(GetSlotKey(slotId, CompletedStagesKey), string.Empty);
        if (string.IsNullOrWhiteSpace(rawStages)) return;

        string[] stages = rawStages.Split(',');
        for (int i = 0; i < stages.Length; i++)
        {
            if (int.TryParse(stages[i], out int stageNumber) && stageNumber >= FirstStageNumber)
            {
                completedStages.Add(stageNumber);
                highestUnlockedStage = Mathf.Max(highestUnlockedStage, stageNumber + 1);
            }
        }
    }

    private void SaveStageProgression(int slotId)
    {
        highestUnlockedStage = Mathf.Max(FirstStageNumber, highestUnlockedStage);
        PlayerPrefs.SetInt(GetSlotKey(slotId, HighestUnlockedStageKey), highestUnlockedStage);
        PlayerPrefs.SetString(GetSlotKey(slotId, CompletedStagesKey), JoinStageNumbers(completedStages));

        foreach (int stageNumber in completedStages)
        {
            if (stageNumber >= FirstStageNumber)
            {
                PlayerPrefs.SetInt(GetSlotKey(slotId, CompletedStageKeyPrefix + stageNumber), 1);
            }
        }
    }

    private static void DeleteCompletedStageKeys(int slotId)
    {
        string rawStages = PlayerPrefs.GetString(GetSlotKey(slotId, CompletedStagesKey), string.Empty);
        if (string.IsNullOrWhiteSpace(rawStages)) return;

        string[] stages = rawStages.Split(',');
        for (int i = 0; i < stages.Length; i++)
        {
            if (int.TryParse(stages[i], out int stageNumber) && stageNumber >= FirstStageNumber)
            {
                PlayerPrefs.DeleteKey(GetSlotKey(slotId, CompletedStageKeyPrefix + stageNumber));
            }
        }
    }

    private static string JoinStageNumbers(HashSet<int> stageNumbers)
    {
        List<int> sortedStageNumbers = new(stageNumbers);
        sortedStageNumbers.Sort();

        List<string> stageStrings = new();
        foreach (int stageNumber in sortedStageNumbers)
        {
            if (stageNumber >= FirstStageNumber)
            {
                stageStrings.Add(stageNumber.ToString());
            }
        }

        return string.Join(",", stageStrings);
    }

    private void LoadGenericProgression(int slotId)
    {
        // Generic unlock/upgrade keys let newer shop systems share one save format while legacy fields remain supported.
        genericUnlockIds.Clear();
        AddCsvIds(genericUnlockIds, PlayerPrefs.GetString(GetSlotKey(slotId, GenericUnlockIdsKey), string.Empty));

        genericUpgradeLevels.Clear();
        HashSet<string> upgradeIds = new();
        AddCsvIds(upgradeIds, PlayerPrefs.GetString(GetSlotKey(slotId, GenericUpgradeIdsKey), string.Empty));
        for (int i = 0; i < BuiltInGenericUpgradeIds.Length; i++)
        {
            upgradeIds.Add(NormalizeId(BuiltInGenericUpgradeIds[i]));
        }
        upgradeIds.Add("health");
        upgradeIds.Add("speed");
        upgradeIds.Add("cannon_damage");
        upgradeIds.Add("magnet_radius");

        foreach (string id in upgradeIds)
        {
            int level = Mathf.Max(0, PlayerPrefs.GetInt(GetSlotKey(slotId, GenericUpgradeKeyPrefix + id), 0));
            if (level > 0)
            {
                genericUpgradeLevels[id] = level;
            }
        }
    }

    private void SyncGenericUpgradeLevelsIntoLegacyFields()
    {
        MigrateLegacyGenericUpgradeId("health", UpgradeBaseHealthId);
        MigrateLegacyGenericUpgradeId("speed", UpgradeBaseSpeedId);
        MigrateLegacyGenericUpgradeId("cannon_damage", UpgradeBaseCannonDamageId);
        MigrateLegacyGenericUpgradeId("magnet_radius", UpgradeBaseMagnetRadiusId);
        MigrateLegacyGenericUpgradeId("cursed_doubloons_power", UpgradeCursedDoubloonsDamageId);
        MigrateLegacyGenericUpgradeId("force_field_power", UpgradeForceFieldDamageId);

        permanentHealthLevel = Mathf.Max(permanentHealthLevel, GetUpgradeLevel(UpgradeHealthId));
        permanentSpeedLevel = Mathf.Max(permanentSpeedLevel, GetUpgradeLevel(UpgradeSpeedId));
        permanentMagnetLevel = Mathf.Max(permanentMagnetLevel, GetUpgradeLevel(UpgradeMagnetRadiusId));
        permanentCannonDamageLevel = Mathf.Max(permanentCannonDamageLevel, GetUpgradeLevel(UpgradeCannonDamageId));
    }

    private void MigrateLegacyGenericUpgradeId(string oldId, string newId)
    {
        int oldLevel = GetUpgradeLevel(oldId);
        if (oldLevel <= 0) return;

        genericUpgradeLevels[NormalizeId(newId)] = Mathf.Max(GetUpgradeLevel(newId), oldLevel);
        genericUpgradeLevels.Remove(NormalizeId(oldId));
    }

    private void MigrateLegacyProgressionIntoGenericKeys()
    {
        // Keep older saved fields playable by mirroring them into the newer generic progression collections.
        if (dashUnlocked) genericUnlockIds.Add(UnlockDashId);
        if (forceFieldUnlocked) genericUnlockIds.Add(UnlockForceFieldId);
        if (permanentMagnetLevel > 0) genericUnlockIds.Add(UnlockMagnetRadius);

        if (permanentHealthLevel > 0) genericUpgradeLevels[UpgradeHealthId] = Mathf.Max(GetUpgradeLevel(UpgradeHealthId), permanentHealthLevel);
        if (permanentSpeedLevel > 0) genericUpgradeLevels[UpgradeSpeedId] = Mathf.Max(GetUpgradeLevel(UpgradeSpeedId), permanentSpeedLevel);
        if (permanentMagnetLevel > 0) genericUpgradeLevels[UpgradeMagnetRadiusId] = Mathf.Max(GetUpgradeLevel(UpgradeMagnetRadiusId), permanentMagnetLevel);
        if (permanentCannonDamageLevel > 0) genericUpgradeLevels[UpgradeCannonDamageId] = Mathf.Max(GetUpgradeLevel(UpgradeCannonDamageId), permanentCannonDamageLevel);
    }

    private void SaveGenericProgression(int slotId)
    {
        // Save both the id list and individual id keys so counts, lookups, and cleanup all have stable data.
        PlayerPrefs.SetString(GetSlotKey(slotId, GenericUnlockIdsKey), string.Join(",", genericUnlockIds));
        foreach (string id in genericUnlockIds)
        {
            PlayerPrefs.SetInt(GetSlotKey(slotId, GenericUnlockKeyPrefix + id), 1);
        }

        PlayerPrefs.SetString(GetSlotKey(slotId, GenericUpgradeIdsKey), string.Join(",", genericUpgradeLevels.Keys));
        foreach (KeyValuePair<string, int> upgrade in genericUpgradeLevels)
        {
            PlayerPrefs.SetInt(GetSlotKey(slotId, GenericUpgradeKeyPrefix + upgrade.Key), Mathf.Max(0, upgrade.Value));
        }
    }

    private void SyncLegacyUpgradeLevel(string id, int level)
    {
        switch (id)
        {
            case UpgradeHealthId:
                permanentHealthLevel = level;
                break;
            case UpgradeSpeedId:
                permanentSpeedLevel = level;
                break;
            case UpgradeMagnetRadiusId:
                permanentMagnetLevel = level;
                break;
            case UpgradeCannonDamageId:
                permanentCannonDamageLevel = level;
                break;
        }
    }
}
