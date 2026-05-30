using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class BossDefeatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipHealth bossHealth;
    [SerializeField] private TMP_Text victoryMessageText;

    [Header("Victory")]
    [SerializeField] private string victoryMessage = "Victory! You defeated the Dread Summoner!";
    [SerializeField] private bool completeStageOnVictory = true;
    [SerializeField] private int completedStageNumber = 1;
    [SerializeField] private string requiredSceneName = "MainSea";
    [SerializeField] private bool pauseGameOnVictory;
    [SerializeField] private bool logVictory = true;

    private bool victoryHandled;

    private void Awake()
    {
        if (bossHealth == null)
        {
            bossHealth = GetComponent<ShipHealth>();
        }
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

        if (victoryMessageText != null)
        {
            victoryMessageText.text = victoryMessage;
            victoryMessageText.gameObject.SetActive(true);
        }

        if (logVictory)
        {
            Debug.Log("Boss defeated! Vertical slice complete!", this);
        }

        if (pauseGameOnVictory)
        {
            Time.timeScale = 0f;
        }
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
