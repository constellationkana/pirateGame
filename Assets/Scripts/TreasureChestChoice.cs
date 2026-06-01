public enum TreasureChestChoiceType
{
    Upgrade,
    Crew,
    CrewUpgrade
}

public class TreasureChestChoice
{
    public TreasureChestChoiceType Type { get; }
    public string Name { get; }
    public string Description { get; }
    public UpgradeManager.UpgradeOption UpgradeOption { get; }
    public string CrewId { get; }

    public TreasureChestChoice(TreasureChestChoiceType type, string name, string description, UpgradeManager.UpgradeOption upgradeOption = null, string crewId = null)
    {
        Type = type;
        Name = name;
        Description = description;
        UpgradeOption = upgradeOption;
        CrewId = crewId;
    }
}
