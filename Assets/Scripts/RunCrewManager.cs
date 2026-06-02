using System;
using System.Collections.Generic;
using UnityEngine;

public class RunCrewManager : MonoBehaviour
{
    public const string PaulCrewId = "paul";
    public const string PaulFasterFiringUpgradeId = "paul_faster_firing";
    public const string PaulStrongerCannonballsUpgradeId = "paul_stronger_cannonballs";
    public const string PaulCannonMasterUpgradeId = "paul_cannon_master";
    public const string PaulVeteranGunnerUpgradeId = "paul_veteran_gunner";
    public const string PaulBroadsideExpertUpgradeId = "paul_broadside_expert";
    public const string CleanUpCrewId = "cleanup_crew";
    public const string CleanUpCrewFasterRepairsUpgradeId = "cleanup_crew_faster_repairs";
    public const string BirdBoyCrewId = "bird_boy";
    public const string BirdBoyDamageUpgradeId = "bird_boy_damage";
    public const string BirdBoyCooldownUpgradeId = "bird_boy_cooldown";
    public const string EvilBirdBoyCrewId = "evil_bird_boy";
    public const string EvilBirdBoyDamageUpgradeId = "evil_bird_boy_damage";
    public const string EvilBirdBoyCooldownUpgradeId = "evil_bird_boy_cooldown";

    [Serializable]
    public class CrewDefinition
    {
        public string id;
        public string displayName;
        [TextArea] public string joinDescription;
        [TextArea] public string upgradeDescription;
        public int maxUpgradeLevel = DefaultMaxCrewUpgradeLevel;
    }

    [Serializable]
    public class CrewUpgradeDefinition
    {
        public string id;
        public string crewId;
        public string displayName;
        [TextArea] public string description;
        public int maxLevel = 1;
    }

    [Header("Crew Definitions")]
    [SerializeField] private CrewDefinition[] crewDefinitions =
    {
        new CrewDefinition { id = PaulCrewId, displayName = "Paul", joinDescription = "Recruit Paul for this run only. Paul automatically fires cannonballs at nearby enemies.", upgradeDescription = "Improve Paul's current-run cannon support." },
        new CrewDefinition { id = CleanUpCrewId, displayName = "Clean-Up Crew", joinDescription = "Recruit Clean-Up Crew for this run only. They automatically repair your ship over time using collected wood.", upgradeDescription = "Clean-Up Crew repairs happen more often for this run." },
        new CrewDefinition { id = BirdBoyCrewId, displayName = "Bird-Boy", joinDescription = "A strange pirate who trained parrots in the art of warfare. Summons parrots that fire egg missiles at enemy ships for this run only.", upgradeDescription = "Improve Bird-Boy's egg missiles for this run only." },
        new CrewDefinition { id = EvilBirdBoyCrewId, displayName = "Evil-Bird-Boy", joinDescription = "A darker version of Bird-Boy with disgusting tactics and overfed birds. Summons parrots that fire poop missiles at enemy ships for this run only.", upgradeDescription = "Improve Evil-Bird-Boy's poop missiles for this run only." },
        new CrewDefinition { id = "shipprick", displayName = "Shipprick", joinDescription = "Shipprick joins for this run.", upgradeDescription = "Improve Shipprick's current-run support. Placeholder only." },
        new CrewDefinition { id = "poseidon", displayName = "Poseidon", joinDescription = "Poseidon joins for this run.", upgradeDescription = "Improve Poseidon's current-run support. Placeholder only." },
        new CrewDefinition { id = "barrel_joe", displayName = "Barrel Joe", joinDescription = "Barrel Joe joins for this run.", upgradeDescription = "Improve Barrel Joe's current-run support. Placeholder only." },
        new CrewDefinition { id = "map_goblin", displayName = "Map Goblin", joinDescription = "Map Goblin joins for this run.", upgradeDescription = "Improve Map Goblin's current-run support. Placeholder only." },
        new CrewDefinition { id = "zeus", displayName = "Zeus", joinDescription = "Zeus joins for this run.", upgradeDescription = "Improve Zeus's current-run support. Placeholder only." },
        new CrewDefinition { id = "carpenter", displayName = "Carpenter", joinDescription = "Carpenter joins for this run.", upgradeDescription = "Improve Carpenter's current-run support. Placeholder only." }
    };

    [Header("Paul Upgrades")]
    [SerializeField] private CrewUpgradeDefinition[] paulUpgradeDefinitions =
    {
        new CrewUpgradeDefinition { id = PaulFasterFiringUpgradeId, crewId = PaulCrewId, displayName = "Paul: Faster Firing", description = "Paul fires cannonballs more often for this run.", maxLevel = 3 },
        new CrewUpgradeDefinition { id = PaulStrongerCannonballsUpgradeId, crewId = PaulCrewId, displayName = "Paul: Stronger Cannonballs", description = "Paul's cannonballs deal more damage for this run.", maxLevel = 3 },
        new CrewUpgradeDefinition { id = PaulCannonMasterUpgradeId, crewId = PaulCrewId, displayName = "Paul: Cannon Master", description = "Paul fires 2 cannonballs instead of 1 for this run.", maxLevel = 1 },
        new CrewUpgradeDefinition { id = PaulVeteranGunnerUpgradeId, crewId = PaulCrewId, displayName = "Paul: Veteran Gunner", description = "Paul's cannonballs pierce 1 enemy for this run.", maxLevel = 1 },
        new CrewUpgradeDefinition { id = PaulBroadsideExpertUpgradeId, crewId = PaulCrewId, displayName = "Paul: Broadside Expert", description = "Paul fires a 3-shot spread for this run.", maxLevel = 1 }
    };

    [Header("Clean-Up Crew Upgrades")]
    [SerializeField] private CrewUpgradeDefinition[] cleanUpCrewUpgradeDefinitions =
    {
        new CrewUpgradeDefinition { id = CleanUpCrewFasterRepairsUpgradeId, crewId = CleanUpCrewId, displayName = "Clean-Up Crew: Faster Repairs", description = "Clean-Up Crew repairs happen more often for this run, using wood faster.", maxLevel = 3 }
    };

    [Header("Bird Crew Upgrades")]
    [SerializeField] private CrewUpgradeDefinition[] birdCrewUpgradeDefinitions =
    {
        new CrewUpgradeDefinition { id = BirdBoyDamageUpgradeId, crewId = BirdBoyCrewId, displayName = "Bird-Boy: Increase Bird Damage", description = "Egg missiles deal more damage for this run.", maxLevel = 3 },
        new CrewUpgradeDefinition { id = BirdBoyCooldownUpgradeId, crewId = BirdBoyCrewId, displayName = "Bird-Boy: Reduce Bird Cooldown", description = "Parrots shoot egg missiles faster for this run.", maxLevel = 3 },
        new CrewUpgradeDefinition { id = EvilBirdBoyDamageUpgradeId, crewId = EvilBirdBoyCrewId, displayName = "Evil-Bird-Boy: Increase Damage", description = "Poop missiles deal more damage for this run.", maxLevel = 3 },
        new CrewUpgradeDefinition { id = EvilBirdBoyCooldownUpgradeId, crewId = EvilBirdBoyCrewId, displayName = "Evil-Bird-Boy: Reduce Cooldown", description = "Parrots throw poop missiles faster for this run.", maxLevel = 3 }
    };

    private const int DefaultMaxCrewUpgradeLevel = 3;

    private readonly HashSet<string> activeCrewIds = new();
    private readonly Dictionary<string, int> currentRunCrewUpgradeLevels = new();
    private readonly Dictionary<string, int> currentRunPaulUpgradeLevels = new();
    private readonly Dictionary<string, int> currentRunCleanUpCrewUpgradeLevels = new();
    private readonly Dictionary<string, int> currentRunBirdCrewUpgradeLevels = new();
    private readonly Dictionary<string, CrewDefinition> definitionsById = new();
    private readonly Dictionary<string, CrewUpgradeDefinition> paulUpgradesById = new();
    private readonly Dictionary<string, CrewUpgradeDefinition> cleanUpCrewUpgradesById = new();
    private readonly Dictionary<string, CrewUpgradeDefinition> birdCrewUpgradesById = new();

    public IReadOnlyCollection<string> ActiveCrewIds => activeCrewIds;
    public IReadOnlyDictionary<string, int> CurrentRunCrewUpgradeLevels => currentRunCrewUpgradeLevels;
    public IReadOnlyDictionary<string, int> CurrentRunPaulUpgradeLevels => currentRunPaulUpgradeLevels;
    public IReadOnlyDictionary<string, int> CurrentRunCleanUpCrewUpgradeLevels => currentRunCleanUpCrewUpgradeLevels;
    public IReadOnlyDictionary<string, int> CurrentRunBirdCrewUpgradeLevels => currentRunBirdCrewUpgradeLevels;

    public event Action CrewStateChanged;

    private void Awake()
    {
        RebuildDefinitionLookup();
        ResetRunCrew();
    }

    public void ResetRunCrew()
    {
        activeCrewIds.Clear();
        currentRunCrewUpgradeLevels.Clear();
        currentRunPaulUpgradeLevels.Clear();
        currentRunCleanUpCrewUpgradeLevels.Clear();
        currentRunBirdCrewUpgradeLevels.Clear();
        CrewStateChanged?.Invoke();
    }

    public List<CrewDefinition> GetAvailableUnlockedCrew()
    {
        List<CrewDefinition> availableCrew = new();
        PlayerProgression progression = PlayerProgression.Instance;
        if (progression == null)
        {
            return availableCrew;
        }

        foreach (string crewId in progression.GetUnlockedCrewIds())
        {
            string normalizedId = NormalizeId(crewId);
            if (string.IsNullOrEmpty(normalizedId) || activeCrewIds.Contains(normalizedId))
            {
                continue;
            }

            availableCrew.Add(GetDefinition(normalizedId));
        }

        return availableCrew;
    }

    public List<CrewDefinition> GetActiveCrew()
    {
        List<CrewDefinition> activeCrew = new();
        foreach (string crewId in activeCrewIds)
        {
            activeCrew.Add(GetDefinition(crewId));
        }

        return activeCrew;
    }

    public List<CrewUpgradeDefinition> GetAvailablePaulUpgrades()
    {
        List<CrewUpgradeDefinition> availableUpgrades = new();
        if (!IsCrewActive(PaulCrewId))
        {
            return availableUpgrades;
        }

        if (paulUpgradesById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        foreach (CrewUpgradeDefinition definition in paulUpgradesById.Values)
        {
            if (definition != null && IsPaulUpgradeAvailable(definition.id))
            {
                availableUpgrades.Add(definition);
            }
        }

        return availableUpgrades;
    }

    public List<CrewUpgradeDefinition> GetAvailableCleanUpCrewUpgrades()
    {
        List<CrewUpgradeDefinition> availableUpgrades = new();
        if (!IsCrewActive(CleanUpCrewId))
        {
            return availableUpgrades;
        }

        if (cleanUpCrewUpgradesById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        foreach (CrewUpgradeDefinition definition in cleanUpCrewUpgradesById.Values)
        {
            if (definition != null && IsCleanUpCrewUpgradeAvailable(definition.id))
            {
                availableUpgrades.Add(definition);
            }
        }

        return availableUpgrades;
    }

    public List<CrewUpgradeDefinition> GetAvailableBirdCrewUpgrades(string crewId)
    {
        List<CrewUpgradeDefinition> availableUpgrades = new();
        string normalizedCrewId = NormalizeId(crewId);
        if (!IsBirdCrewId(normalizedCrewId) || !IsCrewActive(normalizedCrewId))
        {
            return availableUpgrades;
        }

        if (birdCrewUpgradesById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        foreach (CrewUpgradeDefinition definition in birdCrewUpgradesById.Values)
        {
            if (definition != null && definition.crewId == normalizedCrewId && IsBirdCrewUpgradeAvailable(definition.id))
            {
                availableUpgrades.Add(definition);
            }
        }

        return availableUpgrades;
    }

    public bool IsCrewActive(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        return !string.IsNullOrEmpty(normalizedId) && activeCrewIds.Contains(normalizedId);
    }

    public void ActivateCrew(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        if (string.IsNullOrEmpty(normalizedId) || activeCrewIds.Contains(normalizedId))
        {
            return;
        }

        activeCrewIds.Add(normalizedId);
        CrewDefinition definition = GetDefinition(normalizedId);
        Debug.Log($"{definition.displayName} joined the crew", this);
        if (normalizedId == BirdBoyCrewId)
        {
            Debug.Log("[RunCrewManager] Bird-Boy recruited.", this);
        }
        else if (normalizedId == EvilBirdBoyCrewId)
        {
            Debug.Log("[RunCrewManager] Evil-Bird-Boy recruited.", this);
        }

        CrewStateChanged?.Invoke();
    }

    public void ApplyCrewUpgrade(string crewId, string upgradeId = null)
    {
        string normalizedCrewId = NormalizeId(crewId);
        string normalizedUpgradeId = NormalizeId(upgradeId);
        if (normalizedCrewId == PaulCrewId && !string.IsNullOrEmpty(normalizedUpgradeId))
        {
            ApplyPaulUpgrade(normalizedUpgradeId);
            return;
        }

        if (normalizedCrewId == CleanUpCrewId && !string.IsNullOrEmpty(normalizedUpgradeId))
        {
            ApplyCleanUpCrewUpgrade(normalizedUpgradeId);
            return;
        }

        if (IsBirdCrewId(normalizedCrewId) && !string.IsNullOrEmpty(normalizedUpgradeId))
        {
            ApplyBirdCrewUpgrade(normalizedUpgradeId);
            return;
        }

        if (!IsCrewUpgradeAvailable(normalizedCrewId))
        {
            return;
        }

        currentRunCrewUpgradeLevels.TryGetValue(normalizedCrewId, out int currentLevel);
        currentRunCrewUpgradeLevels[normalizedCrewId] = currentLevel + 1;
        CrewDefinition definition = GetDefinition(normalizedCrewId);
        Debug.Log($"{definition.displayName} upgrade selected", this);
        CrewStateChanged?.Invoke();
    }

    public bool IsCrewUpgradeAvailable(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        return !string.IsNullOrEmpty(normalizedId)
            && activeCrewIds.Contains(normalizedId)
            && normalizedId != PaulCrewId
            && normalizedId != CleanUpCrewId
            && !IsBirdCrewId(normalizedId)
            && GetCrewUpgradeLevel(normalizedId) < GetMaxCrewUpgradeLevel(normalizedId);
    }

    public bool IsPaulUpgradeAvailable(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        return IsCrewActive(PaulCrewId)
            && !string.IsNullOrEmpty(normalizedUpgradeId)
            && GetPaulUpgradeLevel(normalizedUpgradeId) < GetMaxPaulUpgradeLevel(normalizedUpgradeId);
    }

    public bool IsCleanUpCrewUpgradeAvailable(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        return IsCrewActive(CleanUpCrewId)
            && !string.IsNullOrEmpty(normalizedUpgradeId)
            && GetCleanUpCrewUpgradeLevel(normalizedUpgradeId) < GetMaxCleanUpCrewUpgradeLevel(normalizedUpgradeId);
    }

    public bool IsBirdCrewUpgradeAvailable(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        CrewUpgradeDefinition definition = GetBirdCrewUpgradeDefinition(normalizedUpgradeId);
        return definition != null
            && IsCrewActive(definition.crewId)
            && GetBirdCrewUpgradeLevel(normalizedUpgradeId) < GetMaxBirdCrewUpgradeLevel(normalizedUpgradeId);
    }

    public int GetMaxCrewUpgradeLevel(string crewId)
    {
        CrewDefinition definition = GetDefinition(crewId);
        return definition != null && definition.maxUpgradeLevel > 0 ? definition.maxUpgradeLevel : DefaultMaxCrewUpgradeLevel;
    }

    public int GetCrewUpgradeLevel(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        return !string.IsNullOrEmpty(normalizedId) && currentRunCrewUpgradeLevels.TryGetValue(normalizedId, out int level) ? level : 0;
    }

    public int GetPaulUpgradeLevel(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        return !string.IsNullOrEmpty(normalizedUpgradeId) && currentRunPaulUpgradeLevels.TryGetValue(normalizedUpgradeId, out int level) ? level : 0;
    }

    public int GetCleanUpCrewUpgradeLevel(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        return !string.IsNullOrEmpty(normalizedUpgradeId) && currentRunCleanUpCrewUpgradeLevels.TryGetValue(normalizedUpgradeId, out int level) ? level : 0;
    }

    public int GetBirdCrewUpgradeLevel(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        return !string.IsNullOrEmpty(normalizedUpgradeId) && currentRunBirdCrewUpgradeLevels.TryGetValue(normalizedUpgradeId, out int level) ? level : 0;
    }

    public int GetMaxPaulUpgradeLevel(string upgradeId)
    {
        CrewUpgradeDefinition definition = GetPaulUpgradeDefinition(upgradeId);
        return definition != null && definition.maxLevel > 0 ? definition.maxLevel : 1;
    }

    public CrewUpgradeDefinition GetPaulUpgradeDefinition(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        if (paulUpgradesById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        return !string.IsNullOrEmpty(normalizedUpgradeId) && paulUpgradesById.TryGetValue(normalizedUpgradeId, out CrewUpgradeDefinition definition) ? definition : null;
    }

    public int GetMaxCleanUpCrewUpgradeLevel(string upgradeId)
    {
        CrewUpgradeDefinition definition = GetCleanUpCrewUpgradeDefinition(upgradeId);
        return definition != null && definition.maxLevel > 0 ? definition.maxLevel : 1;
    }

    public CrewUpgradeDefinition GetCleanUpCrewUpgradeDefinition(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        if (cleanUpCrewUpgradesById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        return !string.IsNullOrEmpty(normalizedUpgradeId) && cleanUpCrewUpgradesById.TryGetValue(normalizedUpgradeId, out CrewUpgradeDefinition definition) ? definition : null;
    }

    public int GetMaxBirdCrewUpgradeLevel(string upgradeId)
    {
        CrewUpgradeDefinition definition = GetBirdCrewUpgradeDefinition(upgradeId);
        return definition != null && definition.maxLevel > 0 ? definition.maxLevel : 1;
    }

    public CrewUpgradeDefinition GetBirdCrewUpgradeDefinition(string upgradeId)
    {
        string normalizedUpgradeId = NormalizeId(upgradeId);
        if (birdCrewUpgradesById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        return !string.IsNullOrEmpty(normalizedUpgradeId) && birdCrewUpgradesById.TryGetValue(normalizedUpgradeId, out CrewUpgradeDefinition definition) ? definition : null;
    }

    public void SetBirdCrewUpgradeMaxLevels(string crewId, int damageMaxLevel, int cooldownMaxLevel)
    {
        string normalizedCrewId = NormalizeId(crewId);
        if (!IsBirdCrewId(normalizedCrewId))
        {
            return;
        }

        if (birdCrewUpgradesById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        string damageUpgradeId = normalizedCrewId == BirdBoyCrewId ? BirdBoyDamageUpgradeId : EvilBirdBoyDamageUpgradeId;
        string cooldownUpgradeId = normalizedCrewId == BirdBoyCrewId ? BirdBoyCooldownUpgradeId : EvilBirdBoyCooldownUpgradeId;
        if (birdCrewUpgradesById.TryGetValue(damageUpgradeId, out CrewUpgradeDefinition damageDefinition))
        {
            damageDefinition.maxLevel = Mathf.Max(1, damageMaxLevel);
        }

        if (birdCrewUpgradesById.TryGetValue(cooldownUpgradeId, out CrewUpgradeDefinition cooldownDefinition))
        {
            cooldownDefinition.maxLevel = Mathf.Max(1, cooldownMaxLevel);
        }
    }

    public CrewDefinition GetDefinition(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        if (definitionsById.Count == 0)
        {
            RebuildDefinitionLookup();
        }

        if (!string.IsNullOrEmpty(normalizedId) && definitionsById.TryGetValue(normalizedId, out CrewDefinition definition))
        {
            return definition;
        }

        string fallbackName = ToDisplayName(normalizedId);
        return new CrewDefinition
        {
            id = normalizedId,
            displayName = fallbackName,
            joinDescription = $"{fallbackName} joins for this run.",
            upgradeDescription = $"Improve {fallbackName}'s current-run support. Placeholder only.",
            maxUpgradeLevel = DefaultMaxCrewUpgradeLevel
        };
    }

    private void ApplyPaulUpgrade(string upgradeId)
    {
        if (!IsPaulUpgradeAvailable(upgradeId))
        {
            return;
        }

        currentRunPaulUpgradeLevels.TryGetValue(upgradeId, out int currentLevel);
        currentRunPaulUpgradeLevels[upgradeId] = currentLevel + 1;
        CrewUpgradeDefinition definition = GetPaulUpgradeDefinition(upgradeId);
        Debug.Log($"{(definition == null ? ToDisplayName(upgradeId) : definition.displayName)} selected", this);
        CrewStateChanged?.Invoke();
    }

    private void ApplyCleanUpCrewUpgrade(string upgradeId)
    {
        if (!IsCleanUpCrewUpgradeAvailable(upgradeId))
        {
            return;
        }

        currentRunCleanUpCrewUpgradeLevels.TryGetValue(upgradeId, out int currentLevel);
        currentRunCleanUpCrewUpgradeLevels[upgradeId] = currentLevel + 1;
        CrewUpgradeDefinition definition = GetCleanUpCrewUpgradeDefinition(upgradeId);
        Debug.Log($"{(definition == null ? ToDisplayName(upgradeId) : definition.displayName)} selected", this);
        CrewStateChanged?.Invoke();
    }

    private void ApplyBirdCrewUpgrade(string upgradeId)
    {
        if (!IsBirdCrewUpgradeAvailable(upgradeId))
        {
            return;
        }

        currentRunBirdCrewUpgradeLevels.TryGetValue(upgradeId, out int currentLevel);
        currentRunBirdCrewUpgradeLevels[upgradeId] = currentLevel + 1;
        CrewUpgradeDefinition definition = GetBirdCrewUpgradeDefinition(upgradeId);
        Debug.Log($"{(definition == null ? ToDisplayName(upgradeId) : definition.displayName)} selected", this);
        CrewStateChanged?.Invoke();
    }

    private void RebuildDefinitionLookup()
    {
        definitionsById.Clear();
        if (crewDefinitions != null)
        {
            foreach (CrewDefinition definition in crewDefinitions)
            {
                if (definition == null)
                {
                    continue;
                }

                string normalizedId = NormalizeId(definition.id);
                if (string.IsNullOrEmpty(normalizedId))
                {
                    continue;
                }

                definition.id = normalizedId;
                if (string.IsNullOrWhiteSpace(definition.displayName))
                {
                    definition.displayName = ToDisplayName(normalizedId);
                }

                definitionsById[normalizedId] = definition;
            }
        }

        paulUpgradesById.Clear();
        RebuildUpgradeLookup(paulUpgradeDefinitions, PaulCrewId, paulUpgradesById);

        cleanUpCrewUpgradesById.Clear();
        RebuildUpgradeLookup(cleanUpCrewUpgradeDefinitions, CleanUpCrewId, cleanUpCrewUpgradesById);

        birdCrewUpgradesById.Clear();
        RebuildUpgradeLookup(birdCrewUpgradeDefinitions, string.Empty, birdCrewUpgradesById);
    }

    private static void RebuildUpgradeLookup(CrewUpgradeDefinition[] sourceDefinitions, string defaultCrewId, Dictionary<string, CrewUpgradeDefinition> targetLookup)
    {
        if (sourceDefinitions == null || targetLookup == null)
        {
            return;
        }

        foreach (CrewUpgradeDefinition definition in sourceDefinitions)
        {
            if (definition == null)
            {
                continue;
            }

            string normalizedUpgradeId = NormalizeId(definition.id);
            if (string.IsNullOrEmpty(normalizedUpgradeId))
            {
                continue;
            }

            definition.id = normalizedUpgradeId;
            definition.crewId = NormalizeId(string.IsNullOrWhiteSpace(definition.crewId) ? defaultCrewId : definition.crewId);
            definition.maxLevel = Mathf.Max(1, definition.maxLevel);
            if (string.IsNullOrWhiteSpace(definition.displayName))
            {
                definition.displayName = ToDisplayName(normalizedUpgradeId);
            }

            targetLookup[normalizedUpgradeId] = definition;
        }
    }

    private static bool IsBirdCrewId(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        return normalizedId == BirdBoyCrewId || normalizedId == EvilBirdBoyCrewId;
    }

    private static string NormalizeId(string id)
    {
        return string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim().ToLowerInvariant();
    }

    private static string ToDisplayName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return "Crew Member";
        }

        string[] words = id.Replace('-', '_').Split('_', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < words.Length; i++)
        {
            words[i] = char.ToUpperInvariant(words[i][0]) + words[i].Substring(1);
        }

        return string.Join(" ", words);
    }
}
