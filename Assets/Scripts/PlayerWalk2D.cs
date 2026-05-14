using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerWalk2D : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    
    [Header("Directional Sprites")]
    public Sprite frontSprite;
    public Sprite backSprite;
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

    public void SetCanMove(bool value)
    {
        canMove = value;

        if (!canMove)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
