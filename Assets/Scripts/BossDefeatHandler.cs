using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class BossDefeatHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipHealth bossHealth;
    [SerializeField] private TMP_Text victoryMessageText;

    [Header("Victory")]
    [SerializeField] private string victoryMessage = "Boss defeated! Vertical slice complete!";
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
}
