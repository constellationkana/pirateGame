using TMPro;
using UnityEngine;

/// <summary>
/// Provides crew shop interaction data and opens the crew hiring UI when the player interacts.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CrewNPCInteraction : MonoBehaviour
{
    [Header("Crew")]
    [SerializeField] private string crewId;
    [SerializeField] private string crewName;
    [TextArea]
    [SerializeField] private string description;
    [TextArea]
    [SerializeField] private string abilityDescription;
    [SerializeField] private int price;
    [SerializeField] private Sprite crewSprite;
    [SerializeField] private Sprite hireScrollSprite;

    [Header("Interaction")]
    [SerializeField] private ShipShopController shopController;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string interactionPrompt = "Press E to talk";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange;

    /// <summary>
    /// Gets the crew identifier payload when this choice is crew-related.
    /// </summary>
    public string CrewId => crewId;
    /// <summary>
    /// Gets the display name for this crew member.
    /// </summary>
    public string CrewName => string.IsNullOrWhiteSpace(crewName) ? crewId : crewName;
    /// <summary>
    /// Gets the description text.
    /// </summary>
    public string Description => description;
    /// <summary>
    /// Gets the crew ability description text.
    /// </summary>
    public string AbilityDescription => abilityDescription;
    /// <summary>
    /// Gets the hiring price for this crew member.
    /// </summary>
    public int Price => price > 0 ? price : GetDefaultPrice(crewId);
    /// <summary>
    /// Gets the portrait sprite for this crew member.
    /// </summary>
    public Sprite CrewSprite => crewSprite;
    /// <summary>
    /// Gets the optional scroll background sprite for this crew member.
    /// </summary>
    public Sprite HireScrollSprite => hireScrollSprite;

    private void Awake()
    {
        EnsureController();
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
        if (shopController != null)
        {
            shopController.OpenCrewMenu(this);
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
        shopController?.SetGlobalPrompt($"{interactionPrompt}: {CrewName}");
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

    private static int GetDefaultPrice(string id)
    {
        return NormalizeId(id) switch
        {
            "paul" => 7000,
            "cleanup_crew" => 3500,
            "bird_boy" => 5000,
            "evil_bird_boy" => 5000,
            "shipprick" => 4000,
            "poseidon" => 6500,
            "barrel_joe" => 5000,
            "map_goblin" => 6500,
            "zeus" => 100000,
            "carpenter" => 5000,
            _ => 0
        };
    }

    private static string NormalizeId(string id)
    {
        return string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim().ToLowerInvariant();
    }

    private void SetPromptVisible(bool visible)
    {
        if (promptText != null)
        {
            promptText.text = visible ? interactionPrompt : string.Empty;
            promptText.gameObject.SetActive(visible);
        }
    }
}
