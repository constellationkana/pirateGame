using UnityEngine;
using UnityEngine.InputSystem;

public class BoardShipTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private ShipController2D shipController;
    [SerializeField] private Transform deckPoint;

    [Header("Unboarding")]
    [SerializeField] private Vector3 unboardOffset = new Vector3(0f, -1.5f, 0f);

    private bool playerInsideZone;
    private bool isBoarded;

    private PlayerWalk2D playerWalk;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;
    private Rigidbody2D shipRb;
    private Transform cachedPlayerParent;

    private void Awake()
    {
        if (playerObject != null)
        {
            CachePlayerComponents();
        }

        if (shipController != null)
        {
            shipRb = shipController.GetComponent<Rigidbody2D>();
            shipController.SetPlayerOnBoard(false);
        }
        else
        {
            Debug.LogWarning("BoardShipTrigger: ShipController2D reference is missing.", this);
        }

        if (deckPoint == null)
        {
            Debug.LogWarning("BoardShipTrigger: DeckPoint reference is missing.", this);
        }
    }

    private void Reset()
    {
        shipController = GetComponentInParent<ShipController2D>();
    }

    private void Update()
    {
        if (!playerInsideZone && !isBoarded)
        {
            return;
        }

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (!isBoarded)
            {
                BoardPlayer();
            }
            else
            {
                UnboardPlayer();
            }
        }
    }

    private void LateUpdate()
    {
        if (!isBoarded || playerObject == null || deckPoint == null)
        {
            return;
        }

        // Hard-lock the player visually to the deck point every frame.
        playerObject.transform.position = deckPoint.position;
        playerObject.transform.rotation = Quaternion.identity;
    }

    private void CachePlayerComponents()
    {
        playerWalk = playerObject.GetComponent<PlayerWalk2D>();
        playerRb = playerObject.GetComponent<Rigidbody2D>();
        playerCollider = playerObject.GetComponent<Collider2D>();

        if (playerWalk == null)
        {
            Debug.LogWarning("BoardShipTrigger: PlayerWalk2D not found on Player object.", this);
        }

        if (playerRb == null)
        {
            Debug.LogWarning("BoardShipTrigger: Rigidbody2D not found on Player object.", this);
        }

        if (playerCollider == null)
        {
            Debug.LogWarning("BoardShipTrigger: Collider2D not found on Player object.", this);
        }
    }

    private void BoardPlayer()
    {
        if (!ValidateReferences())
        {
            return;
        }

        cachedPlayerParent = playerObject.transform.parent;

        // Stop the ship before boarding to prevent physics launch.
        if (shipRb != null)
        {
            shipRb.linearVelocity = Vector2.zero;
            shipRb.angularVelocity = 0f;
        }

        // Disable player movement first.
        if (playerWalk != null)
        {
            playerWalk.SetCanMove(false);
            playerWalk.enabled = false;
        }

        // Always disable player physics while boarded.
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.simulated = false;
        }

        // Always disable the player collider while boarded.
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        // Keep Player separate from the ship hierarchy.
        playerObject.transform.SetParent(null, true);

        // Snap Player to the deck point.
        playerObject.transform.position = deckPoint.position;
        playerObject.transform.rotation = Quaternion.identity;

        shipController.SetPlayerOnBoard(true);
        isBoarded = true;
    }

    private void UnboardPlayer()
    {
        if (playerObject == null || shipController == null)
        {
            Debug.LogWarning("BoardShipTrigger: Cannot unboard because playerObject or shipController is missing.", this);
            return;
        }

        isBoarded = false;

        shipController.SetPlayerOnBoard(false);

        if (shipRb != null)
        {
            shipRb.linearVelocity = Vector2.zero;
            shipRb.angularVelocity = 0f;
        }

        playerObject.transform.SetParent(cachedPlayerParent, true);

        if (deckPoint != null)
        {
            playerObject.transform.position = deckPoint.position + unboardOffset;
        }

        playerObject.transform.rotation = Quaternion.identity;

        if (playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = true;
        }

        if (playerWalk != null)
        {
            playerWalk.enabled = true;
            playerWalk.SetCanMove(true);
        }
    }

    private bool ValidateReferences()
    {
        if (playerObject == null)
        {
            Debug.LogWarning("BoardShipTrigger: Player GameObject reference is missing.", this);
            return false;
        }

        if (shipController == null)
        {
            Debug.LogWarning("BoardShipTrigger: ShipController2D reference is missing.", this);
            return false;
        }

        if (deckPoint == null)
        {
            Debug.LogWarning("BoardShipTrigger: DeckPoint reference is missing.", this);
            return false;
        }

        if (playerWalk == null || playerRb == null || playerCollider == null)
        {
            CachePlayerComponents();
        }

        if (shipRb == null)
        {
            shipRb = shipController.GetComponent<Rigidbody2D>();
        }

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerObject == null)
        {
            return;
        }

        if (other.gameObject == playerObject)
        {
            playerInsideZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playerObject == null)
        {
            return;
        }

        if (other.gameObject == playerObject)
        {
            playerInsideZone = false;
        }
    }
}
