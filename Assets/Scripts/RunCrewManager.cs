using System;
using System.Collections.Generic;
using UnityEngine;

public class RunCrewManager : MonoBehaviour
{
    [Serializable]
    public class CrewDefinition
    {
        public string id;
        public string displayName;
        [TextArea] public string joinDescription;
        [TextArea] public string upgradeDescription;
        public int maxUpgradeLevel = DefaultMaxCrewUpgradeLevel;
    }

    [Header("Crew Definitions")]
    [SerializeField] private CrewDefinition[] crewDefinitions =
    {
        new CrewDefinition { id = "paul", displayName = "Paul", joinDescription = "Paul joins for this run.", upgradeDescription = "Improve Paul's current-run support. Placeholder only." },
        new CrewDefinition { id = "cleanup_crew", displayName = "Cleanup Crew", joinDescription = "Cleanup Crew joins for this run.", upgradeDescription = "Improve Cleanup Crew's current-run support. Placeholder only." },
        new CrewDefinition { id = "bird_boy", displayName = "Bird Boy", joinDescription = "Bird Boy joins for this run.", upgradeDescription = "Improve Bird Boy's current-run support. Placeholder only." },
        new CrewDefinition { id = "evil_bird_boy", displayName = "Evil Bird Boy", joinDescription = "Evil Bird Boy joins for this run.", upgradeDescription = "Improve Evil Bird Boy's current-run support. Placeholder only." },
        new CrewDefinition { id = "shipprick", displayName = "Shipprick", joinDescription = "Shipprick joins for this run.", upgradeDescription = "Improve Shipprick's current-run support. Placeholder only." },
        new CrewDefinition { id = "poseidon", displayName = "Poseidon", joinDescription = "Poseidon joins for this run.", upgradeDescription = "Improve Poseidon's current-run support. Placeholder only." },
        new CrewDefinition { id = "barrel_joe", displayName = "Barrel Joe", joinDescription = "Barrel Joe joins for this run.", upgradeDescription = "Improve Barrel Joe's current-run support. Placeholder only." },
        new CrewDefinition { id = "map_goblin", displayName = "Map Goblin", joinDescription = "Map Goblin joins for this run.", upgradeDescription = "Improve Map Goblin's current-run support. Placeholder only." },
        new CrewDefinition { id = "zeus", displayName = "Zeus", joinDescription = "Zeus joins for this run.", upgradeDescription = "Improve Zeus's current-run support. Placeholder only." },
        new CrewDefinition { id = "carpenter", displayName = "Carpenter", joinDescription = "Carpenter joins for this run.", upgradeDescription = "Improve Carpenter's current-run support. Placeholder only." }
    };

    private const int DefaultMaxCrewUpgradeLevel = 3;

    private readonly HashSet<string> activeCrewIds = new();
    private readonly Dictionary<string, int> currentRunCrewUpgradeLevels = new();
    private readonly Dictionary<string, CrewDefinition> definitionsById = new();

    public IReadOnlyCollection<string> ActiveCrewIds => activeCrewIds;
    public IReadOnlyDictionary<string, int> CurrentRunCrewUpgradeLevels => currentRunCrewUpgradeLevels;

    private void Awake()
    {
        RebuildDefinitionLookup();
        ResetRunCrew();
    }

    public void ResetRunCrew()
    {
        activeCrewIds.Clear();
        currentRunCrewUpgradeLevels.Clear();
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
    }

    public void ApplyCrewUpgrade(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        if (!IsCrewUpgradeAvailable(normalizedId))
        {
            return;
        }

        currentRunCrewUpgradeLevels.TryGetValue(normalizedId, out int currentLevel);
        currentRunCrewUpgradeLevels[normalizedId] = currentLevel + 1;
        CrewDefinition definition = GetDefinition(normalizedId);
        Debug.Log($"{definition.displayName} upgrade selected", this);
    }

    public bool IsCrewUpgradeAvailable(string crewId)
    {
        string normalizedId = NormalizeId(crewId);
        return !string.IsNullOrEmpty(normalizedId)
            && activeCrewIds.Contains(normalizedId)
            && GetCrewUpgradeLevel(normalizedId) < GetMaxCrewUpgradeLevel(normalizedId);
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

    private void RebuildDefinitionLookup()
    {
        definitionsById.Clear();
        if (crewDefinitions == null)
        {
            return;
        }

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
