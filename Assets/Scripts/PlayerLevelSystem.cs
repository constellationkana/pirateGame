using System;
using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    [SerializeField] private int startingXPRequired = 10;
    [SerializeField] private float xpRequirementMultiplier = 1.35f;

    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentXP;
    [SerializeField] private int xpRequiredForNextLevel;

    private int pendingLevelUps;

    public event Action OnXPChanged;
    public event Action<int> OnLevelUp;

    public int CurrentLevel => currentLevel;
    public int CurrentXP => currentXP;
    public int XPRequiredForNextLevel => xpRequiredForNextLevel;
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
