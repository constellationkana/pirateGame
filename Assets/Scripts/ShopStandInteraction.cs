using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShopStandInteraction : MonoBehaviour
{
    public enum ShopStandType
    {
        HealthMenu,
        SpeedMenu,
        ArsenalMenu,
        AbilitiesMenu,
        StartRun
    }

    [Header("Stand")]
    [SerializeField] private ShopStandType standType;
    [SerializeField] private ShipShopController shopController;
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
            case ShopStandType.HealthMenu:
                shopController.OpenHealthMenu();
                break;
            case ShopStandType.SpeedMenu:
                shopController.OpenSpeedMenu();
                break;
            case ShopStandType.ArsenalMenu:
                shopController.OpenArsenalMenu();
                break;
            case ShopStandType.AbilitiesMenu:
                shopController.OpenAbilitiesMenu();
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
            ShopStandType.HealthMenu => "Health Stand",
            ShopStandType.SpeedMenu => "Speed Stand",
            ShopStandType.ArsenalMenu => "Arsenal Stand",
            ShopStandType.AbilitiesMenu => "Abilities Stand",
            ShopStandType.StartRun => "Start Run",
            _ => standType.ToString()
        };
    }
}
