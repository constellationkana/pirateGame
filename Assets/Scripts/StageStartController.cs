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
    [SerializeField] private bool lockPlayerOnShipForRun = true;

    [Header("UI")]
    [SerializeField] private GameObject startRunPanel;
    [SerializeField] private Button startRunButton;
    [SerializeField] private TMP_Text startRunText;
    [SerializeField] private string startRunLabel = "Start Run";

    private bool startRunClicked;

    private void Awake()
    {
        ResolveReferences();
        ApplyUnboardingLock();
        WireButton();
        RefreshStartRunUI();
    }

    private void Start()
    {
        ResolveReferences();
        ApplyUnboardingLock();

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
        ApplyUnboardingLock();

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

        startRunClicked = true;
        RefreshStartRunUI();
    }

    private void ForceBoardPlayerForStageStart()
    {
        if (walkingPlayerObject != null && !walkingPlayerObject.activeSelf)
        {
            walkingPlayerObject.SetActive(true);
        }

        if (boardShipTrigger != null)
        {
            boardShipTrigger.ForceBoardPlayer();
        }
        else if (playerShipController != null)
        {
            playerShipController.ForceBoardPlayer();
        }

        if (walkingPlayerObject != null && disableWalkingPlayerWhileBoarded)
        {
            walkingPlayerObject.SetActive(false);
        }
    }

    private void ApplyUnboardingLock()
    {
        if (playerShipController != null)
        {
            playerShipController.SetUnboardingLocked(lockPlayerOnShipForRun);
        }
    }

    private void RefreshStartRunUI()
    {
        bool shouldShow = showStartRunButton && !startRunClicked;

        if (runTimerDirector != null && runTimerDirector.CurrentRunStartMode == RunTimerDirector.RunStartMode.StartImmediately)
        {
            shouldShow = false;
        }

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
