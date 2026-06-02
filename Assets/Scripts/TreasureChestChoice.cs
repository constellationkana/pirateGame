/// <summary>
/// Identifies the kind of reward represented by a treasure chest choice.
/// </summary>
public enum TreasureChestChoiceType
{
    /// <summary>
    /// Represents the upgrade option.
    /// </summary>
    Upgrade,
    /// <summary>
    /// Represents the crew option.
    /// </summary>
    Crew,
    /// <summary>
    /// Represents the crew upgrade option.
    /// </summary>
    CrewUpgrade
}

/// <summary>
/// Stores the data for one treasure chest reward choice.
/// </summary>
public class TreasureChestChoice
{
    /// <summary>
    /// Gets the reward type.
    /// </summary>
    public TreasureChestChoiceType Type { get; }
    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the description text.
    /// </summary>
    public string Description { get; }
    /// <summary>
    /// Gets the upgrade payload when this choice grants a run upgrade.
    /// </summary>
    public UpgradeManager.UpgradeOption UpgradeOption { get; }
    /// <summary>
    /// Gets the crew identifier payload when this choice is crew-related.
    /// </summary>
    public string CrewId { get; }
    /// <summary>
    /// Gets the crew upgrade identifier payload when this choice is crew-upgrade related.
    /// </summary>
    public string CrewUpgradeId { get; }

    /// <summary>
    /// Creates a treasure chest reward choice with optional upgrade or crew payload data.
    /// </summary>
    /// <param name="type">Reward type represented by this choice.</param>
    /// <param name="name">Display name for this choice.</param>
    /// <param name="description">Description shown for this choice.</param>
    /// <param name="upgradeOption">Upgrade payload for an upgrade choice.</param>
    /// <param name="crewId">Crew identifier used by crew-related choices.</param>
    /// <param name="crewUpgradeId">Crew upgrade identifier used by crew-upgrade choices.</param>
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
