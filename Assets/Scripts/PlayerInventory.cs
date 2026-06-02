using UnityEngine;

/// <summary>
/// Stores the player resources collected during the current run.
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int wood;
    [SerializeField] private int doubloons;

    /// <summary>
    /// Gets the amount of wood collected in the current run.
    /// </summary>
    public int Wood => wood;
    /// <summary>
    /// Gets the amount of doubloons collected in the current run.
    /// </summary>
    public int Doubloons => doubloons;

    /// <summary>
    /// Adds to the wood value.
    /// </summary>
    /// <param name="amount">Amount to add or subtract.</param>
    public void AddWood(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        wood += amount;
    }

    /// <summary>
    /// Attempts to spend wood from the current run inventory.
    /// </summary>
    /// <param name="amount">Amount to add or subtract.</param>
    /// <returns>True when the condition is met; otherwise false.</returns>
    public bool TrySpendWood(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (wood < amount)
        {
            return false;
        }

        wood -= amount;
        return true;
    }

    /// <summary>
    /// Adds to the doubloons value.
    /// </summary>
    /// <param name="amount">Amount to add or subtract.</param>
    public void AddDoubloons(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        doubloons += amount;
        PlayerProgression.Instance.AddDoubloons(amount);
    }
}
