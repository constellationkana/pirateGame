using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResourcePickup : MonoBehaviour
{
    public enum ResourceType
    {
        Wood,
        Doubloon,
        XP
    }

    [SerializeField] private ResourceType resourceType = ResourceType.Wood;
    [SerializeField] private int amount = 1;

    public void SetAmount(int newAmount)
    {
        amount = Mathf.Max(1, newAmount);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = other.GetComponentInParent<PlayerInventory>();
        }

        if (resourceType != ResourceType.XP && inventory == null)
        {
            return;
        }

        switch (resourceType)
        {
            case ResourceType.Wood:
                inventory.AddWood(amount);
                Destroy(gameObject);
                break;
            case ResourceType.Doubloon:
                inventory.AddDoubloons(amount);
                Destroy(gameObject);
                break;
            case ResourceType.XP:
                PlayerLevelSystem levelSystem = other.GetComponent<PlayerLevelSystem>();
                if (levelSystem == null)
                {
                    levelSystem = other.GetComponentInParent<PlayerLevelSystem>();
                }

                if (levelSystem != null)
                {
                    levelSystem.AddXP(amount);
                    Destroy(gameObject);
                }
                break;
        }
    }
}