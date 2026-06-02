using UnityEngine;

/// <summary>
/// Repairs the ship by spending configured inventory resources.
/// </summary>
public class ShipRepairController : MonoBehaviour
{
    [SerializeField] private ShipHealth shipHealth;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private int woodCostPerRepair = 1;
    [SerializeField] private int repairAmount = 1;
    [SerializeField] private float repairCooldown = 0.5f;

    private float nextRepairTime;

    private void Awake()
    {
        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
        }

        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.R) || Time.time < nextRepairTime)
        {
            return;
        }

        if (shipHealth == null || playerInventory == null)
        {
            return;
        }

        if (!playerInventory.TrySpendWood(woodCostPerRepair))
        {
            return;
        }

        shipHealth.Heal(repairAmount);
        nextRepairTime = Time.time + repairCooldown;
    }
}
