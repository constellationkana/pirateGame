using UnityEngine;

public class BoardShipTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipController2D ship;
    [SerializeField] private Transform deckPoint;

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
