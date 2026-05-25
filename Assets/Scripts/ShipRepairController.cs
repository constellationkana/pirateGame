using UnityEngine;

public class ShipRepairController : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ShipHealth shipHealth;

    [Header("Repair Settings")]
    [SerializeField] private int woodCostPerRepair = 1;
    [SerializeField] private int repairAmount = 2;
    [SerializeField] private float repairCooldown = 0.5f;

    private float nextRepairTime;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
        }
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.R))
        {
            return;
        }

        TryRepair();
    }

    public bool TryRepair()
    {
        if (Time.time < nextRepairTime || playerInventory == null || shipHealth == null || shipHealth.IsDead)
        {
            return false;
        }

        if (shipHealth.CurrentHealth >= shipHealth.MaxHealth)
        {
            return false;
        }

        if (!playerInventory.TrySpendWood(woodCostPerRepair))
        {
            return false;
        }

        shipHealth.Heal(repairAmount);
        nextRepairTime = Time.time + repairCooldown;
        return true;
    }
}
