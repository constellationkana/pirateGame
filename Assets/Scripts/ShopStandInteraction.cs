using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShopStandInteraction : MonoBehaviour
{
    public enum ShopStandType
    {
        HealthUpgrade,
        SpeedUpgrade,
        MagnetUpgrade,
        CannonDamageUpgrade,
        DashUnlock,
        ForceFieldUnlock,
        GenericUnlock,
        Cosmetics,
        StartRun
    }

    [Header("Stand")]
    [SerializeField] private ShopStandType standType;
    [SerializeField] private ShipShopController shopController;
    [SerializeField] private string genericUnlockId;
    [SerializeField] private string interactionPrompt = "Press E";
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool playerInRange;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text standLabelText;
    [SerializeField] private bool logInteraction;

    private void Awake()
    {
        EnsureController();
        ApplyStandLabel();
        SetPromptVisible(false);

        Collider2D trigger = GetComponent<Collider2D>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void Update()
    {
        if (!playerInRange || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        EnsureController();
        if (shopController == null)
        {
            return;
        }

        if (logInteraction)
        {
            Debug.Log($"Interacted with {standType} stand.", this);
        }

        switch (standType)
        {
            case ShopStandType.HealthUpgrade:
                shopController.BuyHealthUpgrade();
                break;
            case ShopStandType.SpeedUpgrade:
                shopController.BuySpeedUpgrade();
                break;
            case ShopStandType.MagnetUpgrade:
                shopController.BuyMagnetUpgrade();
                break;
            case ShopStandType.CannonDamageUpgrade:
                shopController.BuyCannonDamageUpgrade();
                break;
            case ShopStandType.DashUnlock:
                shopController.UnlockDash();
                break;
            case ShopStandType.ForceFieldUnlock:
                shopController.UnlockForceField();
                break;
            case ShopStandType.GenericUnlock:
                shopController.BuyGenericUnlock(genericUnlockId);
                break;
            case ShopStandType.Cosmetics:
                shopController.CycleCosmetic();
                break;
            case ShopStandType.StartRun:
                shopController.StartRun();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInRange = true;
        SetPromptVisible(true);
        shopController?.SetGlobalPrompt($"{interactionPrompt}: {GetStandLabel()}");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        playerInRange = false;
        SetPromptVisible(false);
        shopController?.SetGlobalPrompt(string.Empty);
    }

    private bool IsPlayer(Collider2D other)
    {
        return other.CompareTag("Player") || other.GetComponent<PlayerWalk2D>() != null;
    }

    private void EnsureController()
    {
        if (shopController == null)
        {
            shopController = FindFirstObjectByType<ShipShopController>();
        }
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText != null)
        {
            promptText.text = visible ? interactionPrompt : string.Empty;
            promptText.gameObject.SetActive(visible);
        }
    }

    private void ApplyStandLabel()
    {
        if (standLabelText != null)
        {
            standLabelText.text = GetStandLabel();
        }
    }

    private string GetStandLabel()
    {
        return standType switch
        {
            ShopStandType.HealthUpgrade => "Health Upgrade",
            ShopStandType.SpeedUpgrade => "Speed Upgrade",
            ShopStandType.MagnetUpgrade => "Magnet Upgrade",
            ShopStandType.CannonDamageUpgrade => "Cannon Damage",
            ShopStandType.DashUnlock => "Unlock Dash",
            ShopStandType.ForceFieldUnlock => "Unlock Force Field",
            ShopStandType.GenericUnlock => string.IsNullOrWhiteSpace(genericUnlockId) ? "Unlock Upgrade" : $"Unlock {genericUnlockId}",
            ShopStandType.Cosmetics => "Ship Looks",
            ShopStandType.StartRun => "Start Run",
            _ => standType.ToString()
        };
    }
}
