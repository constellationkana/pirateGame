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

    private void Awake()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
        }

        Rigidbody2D pickupRigidbody = GetComponent<Rigidbody2D>();
        if (pickupRigidbody != null)
        {
            pickupRigidbody.bodyType = RigidbodyType2D.Kinematic;
            pickupRigidbody.gravityScale = 0f;
            pickupRigidbody.linearVelocity = Vector2.zero;
            pickupRigidbody.angularVelocity = 0f;
        }
    }

    public void SetAmount(int newAmount)
    {
        amount = Mathf.Max(1, newAmount);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.isTrigger)
        {
            return;
        }

        if (other.GetComponent<Cannonball>() != null || other.GetComponentInParent<Cannonball>() != null)
        {
            return;
        }

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory == null)
        {
            inventory = other.GetComponentInParent<PlayerInventory>();
        }

        PlayerLevelSystem levelSystem = other.GetComponent<PlayerLevelSystem>();
        if (levelSystem == null)
        {
            levelSystem = other.GetComponentInParent<PlayerLevelSystem>();
        }

        if (inventory == null && levelSystem == null)
        {
            return;
        }

        switch (resourceType)
        {
            case ResourceType.Wood:
                if (inventory == null)
                {
                    return;
                }

                inventory.AddWood(amount);
                Destroy(gameObject);
                break;
            case ResourceType.Doubloon:
                if (inventory == null)
                {
                    return;
                }

                inventory.AddDoubloons(amount);
                Destroy(gameObject);
                break;
            case ResourceType.XP:
                if (levelSystem == null)
                {
                    return;
                }

                levelSystem.AddXP(amount);
                Destroy(gameObject);
                break;
        }
    }
}
