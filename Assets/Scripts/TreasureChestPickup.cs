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
                runCrewManager?.ApplyCrewUpgrade(choice.CrewId);
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

            pool.Add(new TreasureChestChoice(
                TreasureChestChoiceType.Crew,
                crew.displayName,
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

        foreach (RunCrewManager.CrewDefinition crew in runCrewManager.GetActiveCrew())
        {
            if (crew == null)
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

    private void EnsureReferences()
    {
        if (upgradeManager == null)
        {
            upgradeManager = FindFirstObjectByType<UpgradeManager>();
        }

        if (runCrewManager == null)
        {
            runCrewManager = FindFirstObjectByType<RunCrewManager>();
        }

        if (runCrewManager == null)
        {
            GameObject managerObject = new("Run Crew Manager");
            runCrewManager = managerObject.AddComponent<RunCrewManager>();
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
