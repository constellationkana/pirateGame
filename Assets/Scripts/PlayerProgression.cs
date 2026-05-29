using System.Collections.Generic;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    public const string UnlockMagnetRadius = "magnet_radius";
    public const string UnlockDashId = "dash";
    public const string UnlockForceFieldId = "force_field";
    public const string UnlockHealthRegenId = "health_regen";
    public const string UnlockCannonballSizeId = "cannonball_size";
    public const string UnlockCannonballSpeedId = "cannonball_speed";
    public const string UnlockCannonballPierceId = "cannonball_pierce";
    public const string UnlockCannonballExplosionId = "cannonball_explosion";
    public const string UnlockBarnaclesId = "barnacles";
    public const string UnlockCursedDoubloonsId = "cursed_doubloons";

    public const string UpgradeHealthId = "health";
    public const string UpgradeSpeedId = "speed";
    public const string UpgradeMagnetRadiusId = "magnet_radius";
    public const string UpgradeCannonDamageId = "cannon_damage";

    private const string HasSaveFileKey = "HasSaveFile";
    private const string TotalDoubloonsKey = "PlayerProgression_TotalDoubloons";
    private const string SelectedShipCosmeticIdKey = "PlayerProgression_SelectedShipCosmeticId";
    private const string PermanentHealthLevelKey = "PlayerProgression_PermanentHealthLevel";
    private const string PermanentSpeedLevelKey = "PlayerProgression_PermanentSpeedLevel";
    private const string PermanentMagnetLevelKey = "PlayerProgression_PermanentMagnetLevel";
    private const string PermanentCannonDamageLevelKey = "PlayerProgression_PermanentCannonDamageLevel";
    private const string DashUnlockedKey = "PlayerProgression_DashUnlocked";
    private const string ForceFieldUnlockedKey = "PlayerProgression_ForceFieldUnlocked";
    private const string OwnedCosmeticsKey = "PlayerProgression_OwnedCosmetics";
    private const string GenericUnlockIdsKey = "PlayerProgression_GenericUnlockIds";
    private const string GenericUpgradeIdsKey = "PlayerProgression_GenericUpgradeIds";
    private const string GenericUnlockKeyPrefix = "PlayerProgression_Unlock_";
    private const string GenericUpgradeKeyPrefix = "PlayerProgression_Upgrade_";

    private static readonly string[] BuiltInGenericUnlockIds =
    {
        UnlockMagnetRadius,
        UnlockDashId,
        UnlockForceFieldId,
        UnlockHealthRegenId,
        UnlockCannonballSizeId,
        UnlockCannonballSpeedId,
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
        UpgradeCannonDamageId
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

    public static bool HasSaveFile() => PlayerPrefs.GetInt(HasSaveFileKey, 0) == 1;

    public static void MarkSaveExists()
    {
        PlayerPrefs.SetInt(HasSaveFileKey, 1);
        PlayerPrefs.Save();
    }

    public static void ResetAllProgression()
    {
        PlayerPrefs.DeleteKey(HasSaveFileKey);
        PlayerPrefs.DeleteKey(TotalDoubloonsKey);
        PlayerPrefs.DeleteKey(SelectedShipCosmeticIdKey);
        PlayerPrefs.DeleteKey(PermanentHealthLevelKey);
        PlayerPrefs.DeleteKey(PermanentSpeedLevelKey);
        PlayerPrefs.DeleteKey(PermanentMagnetLevelKey);
        PlayerPrefs.DeleteKey(PermanentCannonDamageLevelKey);
        PlayerPrefs.DeleteKey(DashUnlockedKey);
        PlayerPrefs.DeleteKey(ForceFieldUnlockedKey);
        PlayerPrefs.DeleteKey(OwnedCosmeticsKey);

        DeleteGenericKeys(GenericUnlockIdsKey, GenericUnlockKeyPrefix, BuiltInGenericUnlockIds);
        DeleteGenericKeys(GenericUpgradeIdsKey, GenericUpgradeKeyPrefix, BuiltInGenericUpgradeIds);

        PlayerPrefs.DeleteKey(GenericUnlockIdsKey);
        PlayerPrefs.DeleteKey(GenericUpgradeIdsKey);
        PlayerPrefs.Save();

        if (instance != null)
        {
            instance.ResetCachedProgression();
        }
    }

    public int GetDoubloons() => totalDoubloons;

    public void AddDoubloons(int amount)
    {
        if (amount <= 0) return;
        totalDoubloons += amount;
        Save();
    }

    public bool SpendDoubloons(int amount)
    {
        if (amount <= 0) return true;
        if (totalDoubloons < amount) return false;

        totalDoubloons -= amount;
        Save();
        return true;
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
        PlayerPrefs.DeleteKey(GenericUnlockKeyPrefix + normalizedId);
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
            PlayerPrefs.DeleteKey(GenericUpgradeKeyPrefix + normalizedId);
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
        MarkSaveExists();
        PlayerPrefs.SetInt(TotalDoubloonsKey, totalDoubloons);
        PlayerPrefs.SetString(SelectedShipCosmeticIdKey, selectedShipCosmeticId);
        PlayerPrefs.SetInt(PermanentHealthLevelKey, permanentHealthLevel);
        PlayerPrefs.SetInt(PermanentSpeedLevelKey, permanentSpeedLevel);
        PlayerPrefs.SetInt(PermanentMagnetLevelKey, permanentMagnetLevel);
        PlayerPrefs.SetInt(PermanentCannonDamageLevelKey, permanentCannonDamageLevel);
        PlayerPrefs.SetInt(DashUnlockedKey, IsDashUnlocked() ? 1 : 0);
        PlayerPrefs.SetInt(ForceFieldUnlockedKey, IsForceFieldUnlocked() ? 1 : 0);
        PlayerPrefs.SetString(OwnedCosmeticsKey, string.Join(",", ownedCosmetics));
        SaveGenericProgression();
        PlayerPrefs.Save();
    }

    public void Load()
    {
        totalDoubloons = Mathf.Max(0, PlayerPrefs.GetInt(TotalDoubloonsKey, 0));
        selectedShipCosmeticId = PlayerPrefs.GetString(SelectedShipCosmeticIdKey, "default");
        permanentHealthLevel = Mathf.Max(0, PlayerPrefs.GetInt(PermanentHealthLevelKey, 0));
        permanentSpeedLevel = Mathf.Max(0, PlayerPrefs.GetInt(PermanentSpeedLevelKey, 0));
        permanentMagnetLevel = Mathf.Max(0, PlayerPrefs.GetInt(PermanentMagnetLevelKey, 0));
        permanentCannonDamageLevel = Mathf.Max(0, PlayerPrefs.GetInt(PermanentCannonDamageLevelKey, 0));
        dashUnlocked = PlayerPrefs.GetInt(DashUnlockedKey, 0) == 1;
        forceFieldUnlocked = PlayerPrefs.GetInt(ForceFieldUnlockedKey, 0) == 1;

        LoadOwnedCosmetics();
        LoadGenericProgression();
        SyncGenericUpgradeLevelsIntoLegacyFields();
        MigrateLegacyProgressionIntoGenericKeys();
    }

    private static void DeleteGenericKeys(string listKey, string keyPrefix, string[] builtInIds)
    {
        HashSet<string> idsToDelete = new();
        AddCsvIds(idsToDelete, PlayerPrefs.GetString(listKey, string.Empty));
        for (int i = 0; i < builtInIds.Length; i++)
        {
            idsToDelete.Add(NormalizeId(builtInIds[i]));
        }

        foreach (string id in idsToDelete)
        {
            if (!string.IsNullOrEmpty(id))
            {
                PlayerPrefs.DeleteKey(keyPrefix + id);
            }
        }
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
    }

    private void LoadOwnedCosmetics()
    {
        ownedCosmetics.Clear();
        string ownedCosmeticsRaw = PlayerPrefs.GetString(OwnedCosmeticsKey, "default");
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

    private void LoadGenericProgression()
    {
        genericUnlockIds.Clear();
        AddCsvIds(genericUnlockIds, PlayerPrefs.GetString(GenericUnlockIdsKey, string.Empty));

        genericUpgradeLevels.Clear();
        HashSet<string> upgradeIds = new();
        AddCsvIds(upgradeIds, PlayerPrefs.GetString(GenericUpgradeIdsKey, string.Empty));
        for (int i = 0; i < BuiltInGenericUpgradeIds.Length; i++)
        {
            upgradeIds.Add(NormalizeId(BuiltInGenericUpgradeIds[i]));
        }

        foreach (string id in upgradeIds)
        {
            int level = Mathf.Max(0, PlayerPrefs.GetInt(GenericUpgradeKeyPrefix + id, 0));
            if (level > 0)
            {
                genericUpgradeLevels[id] = level;
            }
        }
    }

    private void SyncGenericUpgradeLevelsIntoLegacyFields()
    {
        permanentHealthLevel = Mathf.Max(permanentHealthLevel, GetUpgradeLevel(UpgradeHealthId));
        permanentSpeedLevel = Mathf.Max(permanentSpeedLevel, GetUpgradeLevel(UpgradeSpeedId));
        permanentMagnetLevel = Mathf.Max(permanentMagnetLevel, GetUpgradeLevel(UpgradeMagnetRadiusId));
        permanentCannonDamageLevel = Mathf.Max(permanentCannonDamageLevel, GetUpgradeLevel(UpgradeCannonDamageId));
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

    private void SaveGenericProgression()
    {
        PlayerPrefs.SetString(GenericUnlockIdsKey, string.Join(",", genericUnlockIds));
        foreach (string id in genericUnlockIds)
        {
            PlayerPrefs.SetInt(GenericUnlockKeyPrefix + id, 1);
        }

        PlayerPrefs.SetString(GenericUpgradeIdsKey, string.Join(",", genericUpgradeLevels.Keys));
        foreach (KeyValuePair<string, int> upgrade in genericUpgradeLevels)
        {
            PlayerPrefs.SetInt(GenericUpgradeKeyPrefix + upgrade.Key, Mathf.Max(0, upgrade.Value));
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
