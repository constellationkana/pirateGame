using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ShipController2D : MonoBehaviour
{
    [Header("Ship Movement")]
    [SerializeField] private float thrust = 6f;
    [SerializeField] private float turnSpeed = 130f;
    [SerializeField] private float maxSpeed = 5f;

    [Header("State")]
    [SerializeField] private bool playerOnBoard;

    private Rigidbody2D rb;

    public bool PlayerOnBoard => playerOnBoard;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void FixedUpdate()
    {
        if (!playerOnBoard)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, 0.05f);
            rb.angularVelocity = Mathf.Lerp(rb.angularVelocity, 0f, 0.1f);
            return;
        }

        float forwardInput = Input.GetAxis("Vertical");
        float turnInput = -Input.GetAxis("Horizontal");

        Vector2 force = (Vector2)transform.up * (forwardInput * thrust);
        rb.AddForce(force);

        float targetAngularVelocity = turnInput * turnSpeed;
        rb.angularVelocity = targetAngularVelocity;

        rb.linearVelocity = Vector2.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }

    public void SetPlayerOnBoard(bool value)
    {
        playerOnBoard = value;
    }
}
