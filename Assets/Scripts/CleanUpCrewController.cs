using UnityEngine;

/// <summary>
/// Destroys temporary crew-related objects after their configured lifetime.
/// </summary>
public class CleanUpCrewController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RunCrewManager runCrewManager;
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private ShipHealth shipHealth;

    [Header("Repair Settings")]
    [SerializeField] private float repairInterval = 5f;
    [SerializeField] private int woodCostPerRepair = 1;
    [SerializeField] private int healthRepairedPerRepair = 2;

    [Header("Faster Repairs Upgrade")]
    [SerializeField] private float repairIntervalReductionPerLevel = 0.5f;
    [SerializeField] private float minimumRepairInterval = 1.5f;
    [SerializeField] private int maxFasterRepairLevel = 3;

    private float repairTimer;

    private void Awake()
    {
        EnsureReferences();
    }

    private void OnEnable()
    {
        EnsureReferences();
        if (runCrewManager != null)
        {
            runCrewManager.CrewStateChanged += HandleCrewStateChanged;
        }
    }

    private void OnDisable()
    {
        if (runCrewManager != null)
        {
            runCrewManager.CrewStateChanged -= HandleCrewStateChanged;
        }
    }

    private void Update()
    {
        EnsureReferences();
        if (!CanRepairOverTime())
        {
            repairTimer = 0f;
            return;
        }

        repairTimer += Time.deltaTime;
        float currentRepairInterval = GetCurrentRepairInterval();
        if (repairTimer < currentRepairInterval)
        {
            return;
        }

        repairTimer = 0f;
        TryRepairShip();
    }

    private void HandleCrewStateChanged()
    {
        repairTimer = Mathf.Min(repairTimer, GetCurrentRepairInterval());
    }

    private bool CanRepairOverTime()
    {
        return runCrewManager != null
            && runCrewManager.IsCrewActive(RunCrewManager.CleanUpCrewId)
            && playerInventory != null
            && shipHealth != null
            && !shipHealth.IsDead;
    }

    private void TryRepairShip()
    {
        if (shipHealth.CurrentHealth >= shipHealth.MaxHealth)
        {
            return;
        }

        int woodCost = Mathf.Max(1, woodCostPerRepair);
        if (playerInventory.Wood < woodCost || !playerInventory.TrySpendWood(woodCost))
        {
            return;
        }

        shipHealth.Heal(Mathf.Max(1, healthRepairedPerRepair));
    }

    private float GetCurrentRepairInterval()
    {
        float baseInterval = Mathf.Max(0.1f, repairInterval);
        float minInterval = Mathf.Max(0.1f, minimumRepairInterval);
        int fasterRepairLevel = Mathf.Min(
            Mathf.Max(0, maxFasterRepairLevel),
            runCrewManager == null ? 0 : runCrewManager.GetCleanUpCrewUpgradeLevel(RunCrewManager.CleanUpCrewFasterRepairsUpgradeId));
        float upgradedInterval = baseInterval - fasterRepairLevel * Mathf.Max(0f, repairIntervalReductionPerLevel);
        return Mathf.Max(minInterval, upgradedInterval);
    }

    private void EnsureReferences()
    {
        if (runCrewManager == null)
        {
            runCrewManager = FindFirstObjectByType<RunCrewManager>();
        }

        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (shipHealth == null)
        {
            ShipController2D playerShip = FindFirstObjectByType<ShipController2D>();
            if (playerShip != null)
            {
                shipHealth = playerShip.GetComponent<ShipHealth>();
            }
        }

        if (shipHealth == null)
        {
            GameObject playerShipObject = GameObject.FindGameObjectWithTag("PlayerShip");
            if (playerShipObject != null)
            {
                shipHealth = playerShipObject.GetComponentInParent<ShipHealth>();
                if (shipHealth == null)
                {
                    shipHealth = playerShipObject.GetComponentInChildren<ShipHealth>();
                }
            }
        }
    }
}
