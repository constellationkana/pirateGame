using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int wood;
    [SerializeField] private int doubloons;

    public int Wood => wood;
    public int Doubloons => doubloons;

    public void AddWood(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        wood += amount;
    }

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

    public void AddDoubloons(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        doubloons += amount;
    }
}
