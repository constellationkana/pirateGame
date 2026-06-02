using UnityEngine;

/// <summary>
/// Handles walking-player movement and interaction input outside the ship.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWalk2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    
    /// <summary>
    /// Sprite shown when the walking player faces downward/front.
    /// </summary>
    [Header("Directional Sprites")]
    public Sprite frontSprite;
    /// <summary>
    /// Sprite shown when the walking player faces upward/back.
    /// </summary>
    public Sprite backSprite;
    /// <summary>
    /// Sprite shown when the walking player faces left or right.
    /// </summary>
    public Sprite sideSprite;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private bool canMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
            return;
        }

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        movement = new Vector2(x, y).normalized;
        UpdateDirectionalSprite(x, y);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = movement * moveSpeed;
    }

    private void UpdateDirectionalSprite(float x, float y)
    {
        if (spriteRenderer == null || (x == 0f && y == 0f))
        {
            return;
        }

        if (Mathf.Abs(x) > Mathf.Abs(y))
        {
            spriteRenderer.sprite = sideSprite;
            spriteRenderer.flipX = x < 0f;
        }
        else if (y > 0f)
        {
            spriteRenderer.sprite = backSprite;
            spriteRenderer.flipX = false;
        }
        else if (y < 0f)
        {
            spriteRenderer.sprite = frontSprite;
            spriteRenderer.flipX = false;
        }
    }

    /// <summary>
    /// Sets the can move value.
    /// </summary>
    /// <param name="value">True to allow movement; false to stop walking movement.</param>
    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
