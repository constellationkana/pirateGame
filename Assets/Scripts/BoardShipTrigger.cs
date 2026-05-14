using UnityEngine;
using UnityEngine.InputSystem;

public class BoardShipTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private ShipController2D shipController;
    [SerializeField] private Transform deckPoint;
    [SerializeField] private Transform unboardPoint;

    [Header("Fallback Unboard")]
    [SerializeField] private Vector2 unboardOffset = new Vector2(1.5f, 0f);

    private bool playerInsideZone;
    private bool isBoarded;

    private PlayerWalk2D playerWalk;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;

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
        if (!playerInsideZone || Keyboard.current == null)
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

        playerObject.transform.SetParent(deckPoint, false);
        playerObject.transform.localPosition = Vector3.zero;
        playerObject.transform.localRotation = Quaternion.identity;
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

        playerObject.transform.position = deckPoint.position;

        playerWalk?.SetCanMove(false);

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.simulated = false;
        }

        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

        playerObject.transform.SetParent(deckPoint, false);
        playerObject.transform.localPosition = Vector3.zero;
        playerObject.transform.localRotation = Quaternion.identity;
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

        if (unboardPoint != null)
        {
            playerObject.transform.SetParent(null, true);
            playerObject.transform.position = unboardPoint.position;
        }
        else
        {
            playerObject.transform.SetParent(null, true);
            playerObject.transform.position = shipController.transform.position + (Vector3)unboardOffset;
        }

        playerWalk?.SetCanMove(true);
        shipController.SetPlayerOnBoard(false);
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

        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerObject != null && other.gameObject == playerObject)
        {
            playerInsideZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playerObject != null && other.gameObject == playerObject)
        {
            playerInsideZone = false;
        }
    }
}
