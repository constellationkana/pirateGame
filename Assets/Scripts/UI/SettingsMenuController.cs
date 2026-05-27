using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attach to the Settings panel object in MainMenu scene.
/// Persists settings values via PlayerPrefs and applies fullscreen immediately.
/// </summary>
public class SettingsMenuController : MonoBehaviour
{
    private const string MasterVolumeKey = "settings.masterVolume";
    private const string MusicVolumeKey = "settings.musicVolume";
    private const string SfxVolumeKey = "settings.sfxVolume";
    private const string FullscreenKey = "settings.fullscreen";

    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        LoadSettings();
        HookUiEvents();
    }

    private void HookUiEvents()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(SetSfxVolume);
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveListener(SetFullscreen);
    }

    public void LoadSettings()
    {
        float master = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float music = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        float sfx = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(master);
        if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(music);
        if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(sfx);
        if (fullscreenToggle != null) fullscreenToggle.SetIsOnWithoutNotify(fullscreen);

        ApplyMasterVolume(master);
        ApplyFullscreen(fullscreen);
    }

    public void SetMasterVolume(float value)
    {
        ApplyMasterVolume(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat(SfxVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetFullscreen(bool value)
    {
        ApplyFullscreen(value);
        PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private static void ApplyMasterVolume(float value)
    {
        AudioListener.volume = Mathf.Clamp01(value);
    }

    private static void ApplyFullscreen(bool value)
    {
        Screen.fullScreen = value;
    }
}
