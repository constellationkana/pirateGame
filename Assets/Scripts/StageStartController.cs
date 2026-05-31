using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StageStartController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipController2D playerShipController;
    [SerializeField] private BoardShipTrigger boardShipTrigger;
    [SerializeField] private GameObject walkingPlayerObject;
    [SerializeField] private Transform playerShip;
    [SerializeField] private RunTimerDirector runTimerDirector;

    [Header("Stage Start")]
    [SerializeField] private bool startPlayerBoarded = true;
    [SerializeField] private bool disableWalkingPlayerWhileBoarded = true;
    [SerializeField] private bool showStartRunButton = true;

    [Header("UI")]
    [SerializeField] private GameObject startRunPanel;
    [SerializeField] private Button startRunButton;
    [SerializeField] private TMP_Text startRunText;
    [SerializeField] private string startRunLabel = "Start Run";

    private void Awake()
    {
        ResolveReferences();
        WireButton();
        RefreshStartRunUI();
    }

    private void Start()
    {
        ResolveReferences();

        if (startPlayerBoarded)
        {
            ForceBoardPlayerForStageStart();
        }

        if (runTimerDirector != null && runTimerDirector.CurrentRunStartMode == RunTimerDirector.RunStartMode.StartFromButton)
        {
            runTimerDirector.StopTimer();
        }

        RefreshStartRunUI();
    }

    private void OnEnable()
    {
        WireButton();
    }

    private void OnDisable()
    {
        if (startRunButton != null)
        {
            startRunButton.onClick.RemoveListener(StartRun);
        }
    }

    public void StartRun()
    {
        ResolveReferences();

        if (startPlayerBoarded && playerShipController != null && !playerShipController.PlayerOnBoard)
        {
            ForceBoardPlayerForStageStart();
        }

        if (runTimerDirector != null)
        {
            runTimerDirector.StartRun();
        }
        else
        {
            Debug.LogWarning("StageStartController: RunTimerDirector reference is missing, so the run could not be started.", this);
        }

        if (startRunPanel != null)
        {
            startRunPanel.SetActive(false);
        }
    }

    private void ForceBoardPlayerForStageStart()
    {
        if (boardShipTrigger != null)
        {
            boardShipTrigger.ForceBoardPlayer();
            return;
        }

        if (playerShipController != null)
        {
            playerShipController.ForceBoardPlayer();
        }

        if (walkingPlayerObject != null && disableWalkingPlayerWhileBoarded)
        {
            walkingPlayerObject.SetActive(false);
        }
    }

    private void RefreshStartRunUI()
    {
        bool shouldShow = showStartRunButton && (runTimerDirector == null || !runTimerDirector.RunStarted);

        if (startRunPanel != null)
        {
            startRunPanel.SetActive(shouldShow);
        }

        if (startRunText != null)
        {
            startRunText.text = startRunLabel;
        }
    }

    private void WireButton()
    {
        if (startRunButton == null)
        {
            return;
        }

        startRunButton.onClick.RemoveListener(StartRun);
        startRunButton.onClick.AddListener(StartRun);
    }

    private void ResolveReferences()
    {
        playerShipController ??= FindFirstObjectByType<ShipController2D>();
        boardShipTrigger ??= FindFirstObjectByType<BoardShipTrigger>();
        runTimerDirector ??= FindFirstObjectByType<RunTimerDirector>();

        if (playerShip == null && playerShipController != null)
        {
            playerShip = playerShipController.transform;
        }

        if (walkingPlayerObject == null)
        {
            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                walkingPlayerObject = taggedPlayer;
            }
        }
    }
}
