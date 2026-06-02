using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Persistent manager for global UI audio feedback. Place one instance in MainMenu only.
/// It automatically registers every Unity UI Button in loaded scenes without changing their existing listeners.
/// </summary>
public class UIAudioManager : MonoBehaviour
{
    /// <summary>
    /// Gets the persistent singleton instance.
    /// </summary>
    public static UIAudioManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField, Range(0f, 1f)] private float buttonVolume = 1f;
    [SerializeField] private float buttonScanInterval = 1f;

    private float nextButtonScanTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureAudioSource();
        RegisterButtonsInScene();
    }

    private void OnEnable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }
    }

    private void Start()
    {
        RegisterButtonsInScene();
    }

    private void Update()
    {
        if (Time.unscaledTime >= nextButtonScanTime)
        {
            RegisterButtonsInScene();
        }
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }

    /// <summary>
    /// Plays the configured UI button click sound when available.
    /// </summary>
    public void PlayButtonClick()
    {
        if (audioSource == null || buttonClickClip == null)
        {
            return;
        }

        audioSource.PlayOneShot(buttonClickClip, Mathf.Clamp01(buttonVolume));
    }

    /// <summary>
    /// Registers every active Button in the current scene for UI click audio.
    /// </summary>
    public void RegisterButtonsInScene()
    {
        Button[] buttons = FindButtonsIncludingInactive();
        foreach (Button button in buttons)
        {
            if (button != null && button.GetComponent<UIButtonClickSound>() == null)
            {
                button.gameObject.AddComponent<UIButtonClickSound>();
            }
        }

        nextButtonScanTime = Time.unscaledTime + Mathf.Max(0.1f, buttonScanInterval);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterButtonsInScene();
    }

    private void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private static Button[] FindButtonsIncludingInactive()
    {
        return FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
}
