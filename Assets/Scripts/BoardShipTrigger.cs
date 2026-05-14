using UnityEngine;
using UnityEngine.InputSystem;

public class BoardShipTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private ShipController2D shipController;
    [SerializeField] private Transform deckPoint;

    [Header("Options")]
    [SerializeField] private bool disablePlayerPhysicsWhileBoarded = true;

    private bool playerInsideZone;
    private bool isBoarded;

    private PlayerWalk2D playerWalk;
    private Rigidbody2D playerRb;
    private Transform cachedPlayerParent;

    private void Awake()
    {
        if (playerObject != null)
        {
            CachePlayerComponents();
        }

        if (shipController == null)
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
        if (!playerInsideZone)
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

    private void CachePlayerComponents()
    {
        playerWalk = playerObject.GetComponent<PlayerWalk2D>();
        playerRb = playerObject.GetComponent<Rigidbody2D>();

        if (playerWalk == null)
        {
            Debug.LogWarning("BoardShipTrigger: PlayerWalk2D not found on Player object.", this);
        }

        if (playerRb == null)
        {
            Debug.LogWarning("BoardShipTrigger: Rigidbody2D not found on Player object.", this);
        }
    }

    private void BoardPlayer()
    {
        if (!ValidateReferences())
        {
            return;
        }

        cachedPlayerParent = playerObject.transform.parent;

        playerWalk?.SetCanMove(false);

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;

            if (disablePlayerPhysicsWhileBoarded)
            {
                playerRb.simulated = false;
            }
        }

        playerObject.transform.position = deckPoint.position;
        playerObject.transform.rotation = deckPoint.rotation;
        playerObject.transform.SetParent(shipController.transform, true);

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

        playerObject.transform.SetParent(cachedPlayerParent, true);

        if (playerRb != null)
        {
            if (disablePlayerPhysicsWhileBoarded)
            {
                playerRb.simulated = true;
            }

            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        playerWalk?.SetCanMove(true);
        shipController.SetPlayerOnBoard(false);
        isBoarded = false;
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

        if (playerWalk == null || playerRb == null)
        {
            CachePlayerComponents();
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
