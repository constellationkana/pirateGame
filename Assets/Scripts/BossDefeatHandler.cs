using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles boss death by marking stage completion and optionally showing victory UI or run summaries.
/// </summary>
[DisallowMultipleComponent]
public class BossDefeatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipHealth bossHealth;
    [SerializeField] private TMP_Text victoryMessageText;
    [SerializeField] private RunSummaryController runSummaryController;

    [Header("Victory")]
    [SerializeField] private string victoryMessage = "Victory! You defeated the Dread Summoner!";
    [SerializeField] private bool completeStageOnVictory = true;
    [SerializeField] private int completedStageNumber = 1;
    [SerializeField] private string requiredSceneName = "MainSea";
    [SerializeField] private bool pauseGameOnVictory;
    [SerializeField] private bool logVictory = true;

    [Header("Stage Complete Summary")]
    [SerializeField] private bool showStageCompleteSummary = true;
    [SerializeField] private bool hideVictoryMessageWhenSummaryShows = true;

    private bool victoryHandled;

    private void Awake()
    {
        if (bossHealth == null)
        {
            bossHealth = GetComponent<ShipHealth>();
        }

        ResolveRunSummaryController();
    }

    private void OnEnable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDeath += HandleBossDeath;
        }
    }

    private void OnDisable()
    {
        if (bossHealth != null)
        {
            bossHealth.OnDeath -= HandleBossDeath;
        }
    }

    /// <summary>
    /// Assigns the UI text used for victory messages.
    /// </summary>
    /// <param name="text">Text component to assign.</param>
    public void SetVictoryMessageText(TMP_Text text)
    {
        victoryMessageText = text;
    }

    private void HandleBossDeath(ShipHealth _)
    {
        if (victoryHandled)
        {
            return;
        }

        victoryHandled = true;

        CompleteStageOnVictory();

        bool summaryShown = TryShowStageCompleteSummary();

        if (victoryMessageText != null && (!summaryShown || !hideVictoryMessageWhenSummaryShows))
        {
            victoryMessageText.text = victoryMessage;
            victoryMessageText.gameObject.SetActive(true);
        }

        if (logVictory)
        {
            Debug.Log("Boss defeated! Vertical slice complete!", this);
        }

        if (pauseGameOnVictory && !summaryShown)
        {
            Time.timeScale = 0f;
        }
    }


    private bool TryShowStageCompleteSummary()
    {
        if (!showStageCompleteSummary)
        {
            return false;
        }

        ResolveRunSummaryController();
        return runSummaryController != null && runSummaryController.TryShowStageCompleteSummary(bossHealth);
    }

    private void ResolveRunSummaryController()
    {
        if (runSummaryController != null)
        {
            return;
        }

        runSummaryController = FindFirstObjectByType<RunSummaryController>();
    }

    private void CompleteStageOnVictory()
    {
        if (!completeStageOnVictory || completedStageNumber < 1)
        {
            return;
        }

        Scene activeScene = SceneManager.GetActiveScene();
        if (!string.IsNullOrWhiteSpace(requiredSceneName) && activeScene.name != requiredSceneName)
        {
            return;
        }

        PlayerProgression.Instance.CompleteStage(completedStageNumber);

        if (logVictory)
        {
            int unlockedStage = completedStageNumber + 1;
            Debug.Log($"Stage {completedStageNumber} complete. Stage {unlockedStage} unlocked on the Map.", this);
        }
    }
}
