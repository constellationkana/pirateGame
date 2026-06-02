using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TreasureChestPickup : MonoBehaviour
{
    [SerializeField] private TreasureChestChoiceUI choiceUI;
    [SerializeField] private UpgradeManager upgradeManager;
    [SerializeField] private RunCrewManager runCrewManager;

    private TreasureChestSpawner spawner;
    private bool collected;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    public void Initialize(TreasureChestSpawner owner)
    {
        spawner = owner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !IsPlayer(other))
        {
            return;
        }

        Collect();
    }

    private void Collect()
    {
        collected = true;
        if (TryGetComponent(out Collider2D chestCollider))
        {
            chestCollider.enabled = false;
        }

        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>())
        {
            spriteRenderer.enabled = false;
        }

        EnsureReferences();
        List<TreasureChestChoice> choices = BuildChoices(5);
        if (choices.Count == 0)
        {
            Debug.LogWarning("TreasureChestPickup: No treasure chest choices are available.", this);
            FinishCollection();
            return;
        }

        choiceUI.ShowChoices(choices, ApplyChoice, FinishCollection);
    }

    private void ApplyChoice(TreasureChestChoice choice)
    {
        if (choice == null)
        {
            return;
        }

        EnsureReferences();
        switch (choice.Type)
        {
            case TreasureChestChoiceType.Crew:
                runCrewManager?.ActivateCrew(choice.CrewId);
                break;
            case TreasureChestChoiceType.CrewUpgrade:
                runCrewManager?.ApplyCrewUpgrade(choice.CrewId, choice.CrewUpgradeId);
                break;
            default:
                upgradeManager?.ApplyFreeUpgrade(choice.UpgradeOption);
                break;
        }
    }

    private List<TreasureChestChoice> BuildChoices(int choiceCount)
    {
        List<TreasureChestChoice> choices = new();
        if (choiceCount <= 0)
        {
            return choices;
        }

        List<TreasureChestChoice> crewRecruitChoices = new();
        AddAvailableCrewChoices(crewRecruitChoices);

        List<TreasureChestChoice> crewUpgradeChoices = new();
        AddActiveCrewUpgradeChoices(crewUpgradeChoices);

        AddRandomChoices(choices, crewRecruitChoices, choiceCount);
        AddRandomChoices(choices, crewUpgradeChoices, choiceCount);

        if (choices.Count < choiceCount)
        {
            List<TreasureChestChoice> upgradeChoices = new();
            AddUpgradeChoices(upgradeChoices, Mathf.Max(choiceCount - choices.Count, 10));
            AddRandomChoices(choices, upgradeChoices, choiceCount);
        }

        return choices;
    }

    private static void AddRandomChoices(List<TreasureChestChoice> choices, List<TreasureChestChoice> pool, int choiceCount)
    {
        if (choices == null || pool == null || choiceCount <= 0)
        {
            return;
        }

        for (int i = 0; i < pool.Count && choices.Count < choiceCount; i++)
        {
            int index = Random.Range(i, pool.Count);
            (pool[i], pool[index]) = (pool[index], pool[i]);
            choices.Add(pool[i]);
        }
    }

    private void AddUpgradeChoices(List<TreasureChestChoice> pool, int requestedCount)
    {
        if (upgradeManager == null)
        {
            return;
        }

        List<UpgradeManager.UpgradeOption> upgradeChoices = upgradeManager.GetRandomUpgradeChoices(requestedCount);
        foreach (UpgradeManager.UpgradeOption upgradeOption in upgradeChoices)
        {
            if (upgradeOption == null)
            {
                continue;
            }

            pool.Add(new TreasureChestChoice(
                TreasureChestChoiceType.Upgrade,
                upgradeOption.displayName,
                upgradeOption.description,
                upgradeOption));
        }
    }

    private bool AddAvailableCrewChoices(List<TreasureChestChoice> pool)
    {
        if (runCrewManager == null)
        {
            return false;
        }

        bool addedCrew = false;
        foreach (RunCrewManager.CrewDefinition crew in runCrewManager.GetAvailableUnlockedCrew())
        {
            if (crew == null)
            {
                continue;
            }

            string choiceName = crew.id switch
            {
                RunCrewManager.PaulCrewId => "Recruit Paul",
                RunCrewManager.CleanUpCrewId => "Recruit Clean-Up Crew",
                RunCrewManager.BirdBoyCrewId => "Recruit Bird-Boy",
                RunCrewManager.EvilBirdBoyCrewId => "Recruit Evil-Bird-Boy",
                _ => crew.displayName
            };
            pool.Add(new TreasureChestChoice(
                TreasureChestChoiceType.Crew,
                choiceName,
                string.IsNullOrWhiteSpace(crew.joinDescription) ? $"{crew.displayName} joins for this run only." : crew.joinDescription,
                crewId: crew.id));
            addedCrew = true;
        }

        return addedCrew;
    }

    private void AddActiveCrewUpgradeChoices(List<TreasureChestChoice> pool)
    {
        if (runCrewManager == null)
        {
            return;
        }

        AddPaulUpgradeChoices(pool);
        AddCleanUpCrewUpgradeChoices(pool);
        AddBirdCrewUpgradeChoices(pool, RunCrewManager.BirdBoyCrewId);
        AddBirdCrewUpgradeChoices(pool, RunCrewManager.EvilBirdBoyCrewId);

        foreach (RunCrewManager.CrewDefinition crew in runCrewManager.GetActiveCrew())
        {
            if (crew == null || crew.id == RunCrewManager.PaulCrewId || crew.id == RunCrewManager.CleanUpCrewId || crew.id == RunCrewManager.BirdBoyCrewId || crew.id == RunCrewManager.EvilBirdBoyCrewId)
            {
                continue;
            }

            if (!runCrewManager.IsCrewUpgradeAvailable(crew.id))
            {
                continue;
            }

            int nextLevel = runCrewManager.GetCrewUpgradeLevel(crew.id) + 1;
            string description = string.IsNullOrWhiteSpace(crew.upgradeDescription)
                ? $"Upgrade {crew.displayName} for this run only. Placeholder only."
                : crew.upgradeDescription;
            pool.Add(new TreasureChestChoice(
                TreasureChestChoiceType.CrewUpgrade,
                $"{crew.displayName} Upgrade Lv. {nextLevel}",
                description,
                crewId: crew.id));
        }
    }

    private void AddPaulUpgradeChoices(List<TreasureChestChoice> pool)
    {
        foreach (RunCrewManager.CrewUpgradeDefinition upgrade in runCrewManager.GetAvailablePaulUpgrades())
        {
            if (upgrade == null)
            {
                continue;
            }

            int nextLevel = runCrewManager.GetPaulUpgradeLevel(upgrade.id) + 1;
            int maxLevel = runCrewManager.GetMaxPaulUpgradeLevel(upgrade.id);
            string levelSuffix = maxLevel > 1 ? $" Lv. {nextLevel}" : string.Empty;
            pool.Add(new TreasureChestChoice(
                TreasureChestChoiceType.CrewUpgrade,
                $"{upgrade.displayName}{levelSuffix}",
                upgrade.description,
                crewId: RunCrewManager.PaulCrewId,
                crewUpgradeId: upgrade.id));
        }
    }

    private void AddCleanUpCrewUpgradeChoices(List<TreasureChestChoice> pool)
    {
        foreach (RunCrewManager.CrewUpgradeDefinition upgrade in runCrewManager.GetAvailableCleanUpCrewUpgrades())
        {
            if (upgrade == null)
            {
                continue;
            }

            int nextLevel = runCrewManager.GetCleanUpCrewUpgradeLevel(upgrade.id) + 1;
            int maxLevel = runCrewManager.GetMaxCleanUpCrewUpgradeLevel(upgrade.id);
            string levelSuffix = maxLevel > 1 ? $" Lv. {nextLevel}" : string.Empty;
            pool.Add(new TreasureChestChoice(
                TreasureChestChoiceType.CrewUpgrade,
                $"{upgrade.displayName}{levelSuffix}",
                upgrade.description,
                crewId: RunCrewManager.CleanUpCrewId,
                crewUpgradeId: upgrade.id));
        }
    }

    private void AddBirdCrewUpgradeChoices(List<TreasureChestChoice> pool, string crewId)
    {
        foreach (RunCrewManager.CrewUpgradeDefinition upgrade in runCrewManager.GetAvailableBirdCrewUpgrades(crewId))
        {
            if (upgrade == null)
            {
                continue;
            }

            int nextLevel = runCrewManager.GetBirdCrewUpgradeLevel(upgrade.id) + 1;
            int maxLevel = runCrewManager.GetMaxBirdCrewUpgradeLevel(upgrade.id);
            string levelSuffix = maxLevel > 1 ? $" Lv. {nextLevel}" : string.Empty;
            pool.Add(new TreasureChestChoice(
                TreasureChestChoiceType.CrewUpgrade,
                $"{upgrade.displayName}{levelSuffix}",
                upgrade.description,
                crewId: upgrade.crewId,
                crewUpgradeId: upgrade.id));
        }
    }

    private void EnsureReferences()
    {
        if (upgradeManager == null)
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
        }

        runCrewManager = TreasureChestRunBootstrap.EnsureActiveRunSceneServices();
        if (runCrewManager == null)
        {
            runCrewManager = FindFirstObjectByType<RunCrewManager>();
        }

        if (choiceUI == null)
        {
            choiceUI = FindFirstObjectByType<TreasureChestChoiceUI>();
        }

        if (choiceUI == null)
        {
            GameObject uiObject = new("Treasure Chest Choice UI");
            choiceUI = uiObject.AddComponent<TreasureChestChoiceUI>();
        }
    }

    private void FinishCollection()
    {
        spawner?.NotifyChestRemoved(this);
        Destroy(gameObject);
    }

    private static bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponentInParent<ShipController2D>() != null || other.GetComponentInParent<PlayerWalk2D>() != null;
    }
}
