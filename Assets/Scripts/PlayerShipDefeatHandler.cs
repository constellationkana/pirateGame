using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class PlayerShipDefeatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipHealth shipHealth;

    [Header("Defeat")]
    [SerializeField] private string defeatSceneName = "ShipShop";

    private bool hasTriggeredDefeat;

    private void Awake()
    {
        if (shipHealth == null)
        {
            shipHealth = GetComponent<ShipHealth>();
        }

        if (shipHealth == null)
        {
            Debug.LogWarning("PlayerShipDefeatHandler: ShipHealth reference is missing on PlayerShip.", this);
        }
    }

    private void OnEnable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnDeath += HandleShipDeath;
        }
    }

    private void OnDisable()
    {
        if (shipHealth != null)
        {
            shipHealth.OnDeath -= HandleShipDeath;
        }
    }

    private void HandleShipDeath(ShipHealth deadShip)
    {
        if (hasTriggeredDefeat)
        {
            return;
        }

        hasTriggeredDefeat = true;
        SceneManager.LoadScene(defeatSceneName);
    }
}
