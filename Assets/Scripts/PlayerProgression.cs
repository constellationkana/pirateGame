using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    [Serializable]
    public class SaveSlotSummary
    {
        public int slotId;
        public string saveName;
        public int doubloons;
        public int upgradeCount;
        public int unlockCount;
        public bool isActive;
    }

    public const string UnlockHealthRegenId = "health_regen";
    public const string UnlockDashId = "dash";
    public const string UnlockMagnetId = "magnet";
    public const string UnlockForceFieldId = "force_field";
    public const string UnlockCannonballSizeId = "cannonball_size";
    public const string UnlockCannonballSpeedId = "cannonball_speed";
    public const string UnlockCannonballShootRateId = "cannonball_shoot_rate";
    public const string UnlockCannonballExplosionId = "cannonball_explosion";
    public const string UnlockCannonballPierceId = "cannonball_pierce";
    public const string UnlockBarnaclesId = "barnacles";
    public const string UnlockCursedDoubloonsId = "cursed_doubloons";

    public const string UpgradeBaseHealthId = "base_health";
    public const string UpgradeBaseSpeedId = "base_speed";
    public const string UpgradeBaseCannonDamageId = "base_cannon_damage";
    public const string UpgradeBaseCannonballSpeedId = "base_cannonball_speed";
    public const string UpgradeBaseMagnetRadiusId = "base_magnet_radius";
    public const string UpgradeExplosionPowerId = "explosion_power";
    public const string UpgradeBarnaclePowerId = "barnacle_power";
    public const string UpgradeCursedDoubloonsPowerId = "cursed_doubloons_power";
    public const string UpgradeForceFieldPowerId = "force_field_power";

    public const string UnlockMagnetRadius = UnlockMagnetId;
    public const string UpgradeHealthId = UpgradeBaseHealthId;
    public const string UpgradeSpeedId = UpgradeBaseSpeedId;
    public const string UpgradeMagnetRadiusId = UpgradeBaseMagnetRadiusId;
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
        UpgradeCursedDoubloonsPowerId,
        UpgradeForceFieldPowerId
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

    public static int ActiveSaveSlotId => PlayerPrefs.GetInt(ActiveSaveSlotIdKey, NoActiveSaveSlot);
    public static bool HasActiveSaveSlot => SlotExists(ActiveSaveSlotId);
    public static bool HasSaveFile() => GetSaveSlotIds().Count > 0;
    public static string GetActiveSaveName() => HasActiveSaveSlot ? GetSaveSlotName(ActiveSaveSlotId) : "No Active Save";

    public static void MarkSaveExists()
    {
        EnsureActiveSaveSlot(true);
        PlayerPrefs.SetInt(LegacyHasSaveFileKey, HasSaveFile() ? 1 : 0);
        PlayerPrefs.Save();
    }

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

    public static void SaveActiveSlot() => Instance.Save();

    public int GetDoubloons() => totalDoubloons;

    public void AddDoubloons(int amount)
    {
        if (amount <= 0) return;
        totalDoubloons += amount;
        Save();
    }

    public bool SpendDoubloons(int amount)
    {
        if (!HasActiveSaveSlot) return false;
        if (amount <= 0) return true;
        if (totalDoubloons < amount) return false;

        totalDoubloons -= amount;
        Save();
        return true;
    }

    public bool IsStageUnlocked(int stageNumber)
    {
        return stageNumber >= FirstStageNumber && stageNumber <= GetHighestUnlockedStage();
    }

    public void UnlockStage(int stageNumber)
    {
        if (stageNumber < FirstStageNumber) return;

        highestUnlockedStage = Mathf.Max(GetHighestUnlockedStage(), stageNumber);
        Save();
    }

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

    public void CompleteStage(int stageNumber)
    {
        if (stageNumber < FirstStageNumber) return;

        completedStages.Add(stageNumber);
        highestUnlockedStage = Mathf.Max(GetHighestUnlockedStage(), stageNumber + 1);
        Save();
    }

    public bool IsDashUnlocked() => IsUnlocked(UnlockDashId) || dashUnlocked;
    public bool IsForceFieldUnlocked() => IsUnlocked(UnlockForceFieldId) || forceFieldUnlocked;

    public void UnlockDash()
    {
        dashUnlocked = true;
        Unlock(UnlockDashId);
    }

    public void UnlockForceField()
    {
        forceFieldUnlocked = true;
        Unlock(UnlockForceFieldId);
    }

    public int GetPermanentHealthLevel() => permanentHealthLevel;
    public int GetPermanentSpeedLevel() => permanentSpeedLevel;
    public int GetPermanentMagnetLevel() => permanentMagnetLevel;
    public int GetPermanentCannonDamageLevel() => permanentCannonDamageLevel;

    public bool BuyPermanentHealthUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentHealthLevel++;
        SetUpgradeLevel(UpgradeHealthId, permanentHealthLevel);
        return true;
    }

    public bool BuyPermanentSpeedUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentSpeedLevel++;
        SetUpgradeLevel(UpgradeSpeedId, permanentSpeedLevel);
        return true;
    }

    public bool BuyPermanentMagnetUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentMagnetLevel++;
        Unlock(UnlockMagnetRadius);
        SetUpgradeLevel(UpgradeMagnetRadiusId, permanentMagnetLevel);
        return true;
    }

    public bool BuyPermanentCannonDamageUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentCannonDamageLevel++;
        SetUpgradeLevel(UpgradeCannonDamageId, permanentCannonDamageLevel);
        return true;
    }

    public int GetBaseCannonballSpeedLevel() => GetUpgradeLevel(UpgradeBaseCannonballSpeedId);
    public int GetBaseMagnetRadiusLevel() => GetUpgradeLevel(UpgradeBaseMagnetRadiusId);
    public int GetExplosionPowerLevel() => GetUpgradeLevel(UpgradeExplosionPowerId);
    public int GetBarnaclePowerLevel() => GetUpgradeLevel(UpgradeBarnaclePowerId);
    public int GetCursedDoubloonsPowerLevel() => GetUpgradeLevel(UpgradeCursedDoubloonsPowerId);
    public int GetForceFieldPowerLevel() => GetUpgradeLevel(UpgradeForceFieldPowerId);

    public bool TryPurchaseUnlock(string id, int cost)
    {
        if (!HasActiveSaveSlot) return false;

        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId) || IsUnlocked(normalizedId)) return false;
        if (!SpendDoubloons(cost)) return false;

        Unlock(normalizedId);
        return true;
    }

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

    public bool IsUnlocked(string id)
    {
        string normalizedId = NormalizeId(id);
        return !string.IsNullOrEmpty(normalizedId) && genericUnlockIds.Contains(normalizedId);
    }

    public void Unlock(string id)
    {
        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId)) return;

        genericUnlockIds.Add(normalizedId);
        if (normalizedId == UnlockDashId) dashUnlocked = true;
        if (normalizedId == UnlockForceFieldId) forceFieldUnlocked = true;
        Save();
    }

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

    public int GetUpgradeLevel(string id)
    {
        string normalizedId = NormalizeId(id);
        if (string.IsNullOrEmpty(normalizedId)) return 0;

        return genericUpgradeLevels.TryGetValue(normalizedId, out int level) ? Mathf.Max(0, level) : 0;
    }

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

    public void AddUpgradeLevel(string id, int amount = 1)
    {
        if (amount == 0) return;
        SetUpgradeLevel(id, Mathf.Max(0, GetUpgradeLevel(id) + amount));
    }

    public bool IsCrewUnlocked(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        return !string.IsNullOrEmpty(normalizedId) && crewUnlockIds.Contains(normalizedId);
    }

    public void UnlockCrew(string crewId)
    {
        if (!HasActiveSaveSlot) return;

        string normalizedId = NormalizeId(crewId);
        if (string.IsNullOrEmpty(normalizedId)) return;

        crewUnlockIds.Add(normalizedId);
        Save();
    }

    public List<string> GetUnlockedCrewIds()
    {
        List<string> unlockedCrewIds = new(crewUnlockIds);
        unlockedCrewIds.Sort();
        return unlockedCrewIds;
    }

    public string GetSelectedShipCosmeticId() => selectedShipCosmeticId;

    public void SetSelectedShipCosmeticId(string id)
    {
        selectedShipCosmeticId = string.IsNullOrWhiteSpace(id) ? "default" : id;
        if (!ownedCosmetics.Contains(selectedShipCosmeticId))
        {
            ownedCosmetics.Add(selectedShipCosmeticId);
        }
        Save();
    }

    public bool IsCosmeticOwned(string id)
    {
        return string.IsNullOrWhiteSpace(id) || id == "default" || ownedCosmetics.Contains(id);
    }

    public bool BuyCosmetic(string id, int cost)
    {
        if (string.IsNullOrWhiteSpace(id) || id == "default") return true;
        if (ownedCosmetics.Contains(id)) return true;
        if (!SpendDoubloons(cost)) return false;

        ownedCosmetics.Add(id);
        Save();
        return true;
    }

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
