using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ResourcePickup : MonoBehaviour
{
    public enum ResourceType
    {
        Wood,
        Doubloon
    }

    [SerializeField] private ResourceType resourceType = ResourceType.Wood;
    [SerializeField] private int amount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory == null)
        {
            inventory = other.GetComponentInParent<PlayerInventory>();
        }

        if (inventory == null)
        {
            return;
        }

        if (resourceType == ResourceType.Wood)
        {
            inventory.AddWood(amount);
        }
        else
        {
            inventory.AddDoubloons(amount);
        }

        Destroy(gameObject);
    }
}
