using UnityEngine;

public class BoardShipTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipController2D ship;
    [SerializeField] private Transform deckPoint;
    [SerializeField] private Transform unboardPoint;

    private bool playerInside;
    private PlayerWalk2D player;
    private Rigidbody2D playerRb;
    private Transform originalParent;

    private bool playerInside;
    private PlayerWalk2D player;

    private void Reset()
    {
        ship = GetComponentInParent<ShipController2D>();
    }

    private void Update()
    {
        if (!playerInside || player == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            bool board = !ship.PlayerOnBoard;
            if (board)
            {
                BoardPlayer();
            }
            else
            {
                UnboardPlayer();
            }
        }
    }

    private void BoardPlayer()
    {
        if (deckPoint == null || player == null || ship == null)
        {
            return;
        }

        originalParent = player.transform.parent;
        playerRb = player.GetComponent<Rigidbody2D>();

        player.SetCanMove(false);

        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
            playerRb.simulated = false;
        }

        player.transform.position = deckPoint.position;
        player.transform.SetParent(ship.transform, true);

        ship.SetPlayerOnBoard(true);
    }

    private void UnboardPlayer()
    {
        if (player == null || ship == null)
        {
            return;
        }

        player.transform.SetParent(originalParent, true);
        if (unboardPoint != null)
        {
            player.transform.position = unboardPoint.position;
        }

        if (playerRb != null)
        {
            playerRb.simulated = true;
            playerRb.linearVelocity = Vector2.zero;
            playerRb.angularVelocity = 0f;
        }

        player.SetCanMove(true);
        ship.SetPlayerOnBoard(false);
    }

            ship.SetPlayerOnBoard(board);
            player.SetCanMove(!board);

            if (board && deckPoint != null)
            {
                player.transform.position = deckPoint.position;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        player = other.GetComponent<PlayerWalk2D>();
        if (player == null)
        {
            return;
        }

        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInside = false;
    }
}
