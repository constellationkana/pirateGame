using UnityEngine;

public class PickupMagnetController : MonoBehaviour
{
    [Header("Magnet Settings")]
    [SerializeField] private float magnetRadius = 3f;
    [SerializeField] private float magnetPullSpeed = 6f;
    [SerializeField] private LayerMask pickupLayer;
    [SerializeField] private bool showDebugRadius = false;

    private readonly Collider2D[] overlapResults = new Collider2D[128];

    public float MagnetRadius => magnetRadius;

    public void AddMagnetRadius(float amount)
    {
        magnetRadius = Mathf.Max(0f, magnetRadius + amount);
    }

    private void FixedUpdate()
    {
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, magnetRadius, overlapResults, pickupLayer);

        for (int i = 0; i < count; i++)
        {
            Collider2D pickupCollider = overlapResults[i];
            if (pickupCollider == null)
            {
                continue;
            }

            ResourcePickup resourcePickup = pickupCollider.GetComponent<ResourcePickup>();
            if (resourcePickup == null)
            {
                resourcePickup = pickupCollider.GetComponentInParent<ResourcePickup>();
            }

            if (resourcePickup == null)
            {
                continue;
            }

            Rigidbody2D pickupRigidbody = resourcePickup.GetComponent<Rigidbody2D>();
            if (pickupRigidbody == null)
            {
                pickupRigidbody = resourcePickup.GetComponentInParent<Rigidbody2D>();
            }

            if (pickupRigidbody != null)
            {
                pickupRigidbody.bodyType = RigidbodyType2D.Kinematic;
                pickupRigidbody.gravityScale = 0f;
                pickupRigidbody.linearVelocity = Vector2.zero;
                pickupRigidbody.angularVelocity = 0f;
            }

            Transform pickupTransform = resourcePickup.transform;
            Vector2 currentPosition = pickupTransform.position;
            Vector2 targetPosition = transform.position;
            float step = magnetPullSpeed * Time.fixedDeltaTime;

            pickupTransform.position = Vector2.MoveTowards(currentPosition, targetPosition, step);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebugRadius)
        {
            return;
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, magnetRadius);
    }
}
