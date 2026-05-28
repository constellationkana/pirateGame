using System.Collections.Generic;
using UnityEngine;

public class PlayerProgression : MonoBehaviour
{
    private const string TotalDoubloonsKey = "PlayerProgression_TotalDoubloons";
    private const string SelectedShipCosmeticIdKey = "PlayerProgression_SelectedShipCosmeticId";
    private const string PermanentHealthLevelKey = "PlayerProgression_PermanentHealthLevel";
    private const string PermanentSpeedLevelKey = "PlayerProgression_PermanentSpeedLevel";
    private const string PermanentMagnetLevelKey = "PlayerProgression_PermanentMagnetLevel";
    private const string PermanentCannonDamageLevelKey = "PlayerProgression_PermanentCannonDamageLevel";
    private const string DashUnlockedKey = "PlayerProgression_DashUnlocked";
    private const string ForceFieldUnlockedKey = "PlayerProgression_ForceFieldUnlocked";
    private const string OwnedCosmeticsKey = "PlayerProgression_OwnedCosmetics";

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

    public bool IsDashUnlocked() => dashUnlocked;
    public bool IsForceFieldUnlocked() => forceFieldUnlocked;

    public void UnlockDash()
    {
        dashUnlocked = true;
        Save();
    }

    public void UnlockForceField()
    {
        forceFieldUnlocked = true;
        Save();
    }

    public int GetPermanentHealthLevel() => permanentHealthLevel;
    public int GetPermanentSpeedLevel() => permanentSpeedLevel;
    public int GetPermanentMagnetLevel() => permanentMagnetLevel;
    public int GetPermanentCannonDamageLevel() => permanentCannonDamageLevel;

    public bool BuyPermanentHealthUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentHealthLevel++;
        Save();
        return true;
    }

    public bool BuyPermanentSpeedUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentSpeedLevel++;
        Save();
        return true;
    }

    public bool BuyPermanentMagnetUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentMagnetLevel++;
        Save();
        return true;
    }

    public bool BuyPermanentCannonDamageUpgrade(int cost)
    {
        if (!SpendDoubloons(cost)) return false;
        permanentCannonDamageLevel++;
        Save();
        return true;
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
        PlayerPrefs.SetInt(TotalDoubloonsKey, totalDoubloons);
        PlayerPrefs.SetString(SelectedShipCosmeticIdKey, selectedShipCosmeticId);
        PlayerPrefs.SetInt(PermanentHealthLevelKey, permanentHealthLevel);
        PlayerPrefs.SetInt(PermanentSpeedLevelKey, permanentSpeedLevel);
        PlayerPrefs.SetInt(PermanentMagnetLevelKey, permanentMagnetLevel);
        PlayerPrefs.SetInt(PermanentCannonDamageLevelKey, permanentCannonDamageLevel);
        PlayerPrefs.SetInt(DashUnlockedKey, dashUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(ForceFieldUnlockedKey, forceFieldUnlocked ? 1 : 0);
        PlayerPrefs.SetString(OwnedCosmeticsKey, string.Join(",", ownedCosmetics));
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
}
