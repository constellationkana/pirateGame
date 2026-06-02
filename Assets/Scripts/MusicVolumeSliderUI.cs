using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Connects the MainMenu music volume slider to the persistent MusicManager instance at runtime.
/// </summary>
[RequireComponent(typeof(Slider))]
public class MusicVolumeSliderUI : MonoBehaviour
{
    [SerializeField] private Slider musicVolumeSlider;

    private void Awake()
    {
        if (musicVolumeSlider == null)
        {
            musicVolumeSlider = GetComponent<Slider>();
        }
    }

    private void Start()
    {
        if (musicVolumeSlider == null)
        {
            return;
        }

        MusicManager manager = MusicManager.Instance;
        if (manager != null)
        {
            musicVolumeSlider.SetValueWithoutNotify(manager.MusicVolume);
        }
        else if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolumeSlider.SetValueWithoutNotify(Mathf.Clamp01(PlayerPrefs.GetFloat("MusicVolume")));
        }

        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
    }

    private void OnDestroy()
    {
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        }
    }

    private static void SetMusicVolume(float volume)
    {
        MusicManager manager = MusicManager.Instance;
        if (manager != null)
        {
            manager.SetMusicVolume(volume);
        }
    }
}
