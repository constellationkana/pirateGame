using System;
using UnityEngine;

/// <summary>
/// Tracks player experience, level-ups, and upgrade-choice events for a run.
/// </summary>
public class PlayerLevelSystem : MonoBehaviour
{
    [SerializeField] private int startingXPRequired = 10;
    [SerializeField] private float xpRequirementMultiplier = 1.35f;

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP;
    [SerializeField] private int xpRequiredForNextLevel;

    private int pendingLevelUps;

    /// <summary>
    /// Raised when the current XP value changes.
    /// </summary>
    public event Action OnXPChanged;
    /// <summary>
    /// Raised when the player gains a level.
    /// </summary>
    public event Action<int> OnLevelUp;

    /// <summary>
    /// Gets the current level value.
    /// </summary>
    public int CurrentLevel => currentLevel;
    /// <summary>
    /// Gets the current XP value.
    /// </summary>
    public int CurrentXP => currentXP;
    /// <summary>
    /// Gets the XP required to reach the next level.
    /// </summary>
    public int XPRequiredForNextLevel => xpRequiredForNextLevel;
    /// <summary>
    /// Gets the current XP progress as a 0-to-1 percentage.
    /// </summary>
    public float XPPercent => xpRequiredForNextLevel <= 0 ? 0f : (float)currentXP / xpRequiredForNextLevel;

    private void Awake()
    {
        currentLevel = 1;
        currentXP = 0;
        xpRequiredForNextLevel = Mathf.Max(1, startingXPRequired);
    }

    private void Start()
    {
        OnXPChanged?.Invoke();
    }

    /// <summary>
    /// Adds to the XP value.
    /// </summary>
    /// <param name="amount">Amount to add or subtract.</param>
    public void AddXP(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentXP += amount;
        pendingLevelUps += CalculateAdditionalPendingLevelUps();

        if (pendingLevelUps > 0)
        {
            ProcessOneLevelUp();
        }

        OnXPChanged?.Invoke();
    }

    /// <summary>
    /// Notifies listeners that level up choice completed occurred.
    /// </summary>
    public void NotifyLevelUpChoiceCompleted()
    {
        if (pendingLevelUps > 0)
        {
            ProcessOneLevelUp();
        }

        OnXPChanged?.Invoke();
    }

    private int CalculateAdditionalPendingLevelUps()
    {
        int simulatedXP = currentXP;
        int simulatedRequiredXP = xpRequiredForNextLevel;
        int queuedLevels = 0;

        while (simulatedXP >= simulatedRequiredXP)
        {
            simulatedXP -= simulatedRequiredXP;
            simulatedRequiredXP = Mathf.Max(1, Mathf.CeilToInt(simulatedRequiredXP * xpRequirementMultiplier));
            queuedLevels++;
        }

        return queuedLevels - pendingLevelUps;
    }

    private void ProcessOneLevelUp()
    {
        if (pendingLevelUps <= 0 || currentXP < xpRequiredForNextLevel)
        {
            return;
        }

        pendingLevelUps--;
        currentXP -= xpRequiredForNextLevel;
        currentLevel++;
        xpRequiredForNextLevel = Mathf.Max(1, Mathf.CeilToInt(xpRequiredForNextLevel * xpRequirementMultiplier));

        OnLevelUp?.Invoke(currentLevel);
    }
}
