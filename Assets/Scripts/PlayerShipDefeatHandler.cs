using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Connects player ship death to run-end summary behavior.
/// </summary>
[DisallowMultipleComponent]
public class PlayerShipDefeatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipHealth shipHealth;

    [Header("Defeat")]
    [SerializeField] private bool showRunSummaryBeforeSceneLoad = true;
    [SerializeField] private string defeatSceneName = "ShipShop";
    [SerializeField] private float defeatLoadDelay = 0.5f;
    [SerializeField] private bool logDefeatTransition = true;

    private bool hasTriggeredDefeat;
    private Coroutine defeatRoutine;

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

        if (showRunSummaryBeforeSceneLoad && TryShowRunSummary(deadShip))
        {
            return;
        }

        if (defeatRoutine != null)
        {
            StopCoroutine(defeatRoutine);
        }

        defeatRoutine = StartCoroutine(LoadDefeatSceneAfterDelay());
    }

    private bool TryShowRunSummary(ShipHealth deadShip)
    {
        RunSummaryController runSummaryController = FindFirstObjectByType<RunSummaryController>();
        if (runSummaryController == null)
        {
            runSummaryController = gameObject.AddComponent<RunSummaryController>();
        }

        return runSummaryController != null && runSummaryController.TryShowDeathSummary(deadShip);
    }

    private IEnumerator LoadDefeatSceneAfterDelay()
    {
        if (logDefeatTransition)
        {
            Debug.Log($"PlayerShip defeated, loading {defeatSceneName}", this);
        }

        float delay = Mathf.Max(0f, defeatLoadDelay);
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (string.IsNullOrWhiteSpace(defeatSceneName))
        {
            Debug.LogWarning("PlayerShipDefeatHandler: defeatSceneName is empty.", this);
            yield break;
        }

        SceneManager.LoadScene(defeatSceneName);
    }
}
