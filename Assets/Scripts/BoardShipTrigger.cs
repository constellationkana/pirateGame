using UnityEngine;
using UnityEngine.InputSystem;

public class BoardShipTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private ShipController2D shipController;
    [SerializeField] private Transform deckPoint;
<<<<<<< HEAD

    [Header("Unboarding")]
    [SerializeField] private Vector3 unboardOffset = new Vector3(0f, -1.5f, 0f);
=======
    [SerializeField] private Transform unboardPoint;

    [Header("Fallback Unboard")]
    [SerializeField] private Vector2 unboardOffset = new Vector2(1.5f, 0f);
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53

    private bool playerInsideZone;
    private bool isBoarded;

    private PlayerWalk2D playerWalk;
    private Rigidbody2D playerRb;
    private Collider2D playerCollider;
<<<<<<< HEAD
    private Rigidbody2D shipRb;
    private Transform cachedPlayerParent;
=======
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53

    private void Awake()
    {
        if (playerObject != null)
        {
            CachePlayerComponents();
        }

<<<<<<< HEAD
        if (shipController != null)
        {
            shipRb = shipController.GetComponent<Rigidbody2D>();
            shipController.SetPlayerOnBoard(false);
        }
        else
=======
        if (shipController == null)
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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
<<<<<<< HEAD
        if (!playerInsideZone && !isBoarded)
        {
            return;
        }

        if (Keyboard.current == null)
=======
        if (!playerInsideZone || Keyboard.current == null)
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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

<<<<<<< HEAD
        // Hard-lock the player visually to the deck point every frame.
=======
        playerObject.transform.SetParent(deckPoint, false);
        playerObject.transform.localPosition = Vector3.zero;
        playerObject.transform.localRotation = Quaternion.identity;
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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

<<<<<<< HEAD
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
=======
        playerObject.transform.position = deckPoint.position;

        playerWalk?.SetCanMove(false);

>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.simulated = false;
        }

<<<<<<< HEAD
        // Always disable the player collider while boarded.
=======
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
        if (playerCollider != null)
        {
            playerCollider.enabled = false;
        }

<<<<<<< HEAD
        // Keep Player separate from the ship hierarchy.
        playerObject.transform.SetParent(null, true);

        // Snap Player to the deck point.
        playerObject.transform.position = deckPoint.position;
        playerObject.transform.rotation = Quaternion.identity;

=======
        playerObject.transform.SetParent(deckPoint, false);
        playerObject.transform.localPosition = Vector3.zero;
        playerObject.transform.localRotation = Quaternion.identity;
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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

<<<<<<< HEAD
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

=======
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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

<<<<<<< HEAD
        if (playerWalk != null)
        {
            playerWalk.enabled = true;
            playerWalk.SetCanMove(true);
        }
=======
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
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
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

<<<<<<< HEAD
        if (shipRb == null)
        {
            shipRb = shipController.GetComponent<Rigidbody2D>();
        }

=======
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
<<<<<<< HEAD
        if (playerObject == null)
        {
            return;
        }

        if (other.gameObject == playerObject)
=======
        if (playerObject != null && other.gameObject == playerObject)
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
        {
            playerInsideZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
<<<<<<< HEAD
        if (playerObject == null)
        {
            return;
        }

        if (other.gameObject == playerObject)
=======
        if (playerObject != null && other.gameObject == playerObject)
>>>>>>> origin/codex/create-development-plan-for-pirate-game-prototype-xnzu53
        {
            playerInsideZone = false;
        }
    }
}
