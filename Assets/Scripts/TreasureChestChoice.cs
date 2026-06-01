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
    public string CrewUpgradeId { get; }

    public TreasureChestChoice(TreasureChestChoiceType type, string name, string description, UpgradeManager.UpgradeOption upgradeOption = null, string crewId = null, string crewUpgradeId = null)
    {
        Type = type;
        Name = name;
        Description = description;
        UpgradeOption = upgradeOption;
        CrewId = crewId;
        CrewUpgradeId = crewUpgradeId;
    }
}
