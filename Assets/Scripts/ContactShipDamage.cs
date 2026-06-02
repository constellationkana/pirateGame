using UnityEngine;

/// <summary>
/// Applies damage when this object contacts a ship health target.
/// </summary>
public class ContactShipDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageCooldown = 1f;

    private float nextDamageTime;

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (Time.time < nextDamageTime)
        {
            return;
        }

        ShipController2D playerShipController = other.GetComponentInParent<ShipController2D>();
        if (playerShipController == null)
        {
            return;
        }

        ShipHealth playerHealth = other.GetComponentInParent<ShipHealth>();
        if (playerHealth == null)
        {
            return;
        }

        playerHealth.TakeDamage(damage);
        nextDamageTime = Time.time + damageCooldown;
    }
}