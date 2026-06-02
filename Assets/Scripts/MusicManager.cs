using UnityEngine;

/// <summary>
/// Persistent singleton responsible for global background music and saved music volume.
/// Add this to the MainMenu scene only; it survives scene changes through DontDestroyOnLoad.
/// </summary>
public class MusicManager : MonoBehaviour
{
    private const string LegacySettingsMusicVolumeKey = "settings.musicVolume";

    /// <summary>
    /// Gets the persistent singleton instance.
    /// </summary>
    public static MusicManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private float defaultVolume = 0.6f;
    [SerializeField] private string volumePrefsKey = "MusicVolume";

    /// <summary>
    /// Gets the saved music volume in the range used by the AudioSource.
    /// </summary>
    public float MusicVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        EnsureMusicSource();
        MusicVolume = LoadSavedVolume();
        ApplyMusicVolume(MusicVolume);
        PlayerPrefs.SetFloat(volumePrefsKey, MusicVolume);
        SyncLegacySettingsVolume(MusicVolume);
        PlayerPrefs.Save();
        ConfigureAndPlayDefaultMusic();
    }

    /// <summary>
    /// Sets the global music volume, updates the AudioSource, and saves it to PlayerPrefs.
    /// </summary>
    /// <param name="volume">Requested volume, clamped between 0 and 1.</param>
    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        ApplyMusicVolume(MusicVolume);

        PlayerPrefs.SetFloat(volumePrefsKey, MusicVolume);
        SyncLegacySettingsVolume(MusicVolume);
        PlayerPrefs.Save();
    }

    private float LoadSavedVolume()
    {
        if (PlayerPrefs.HasKey(volumePrefsKey))
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(volumePrefsKey));
        }

        if (PlayerPrefs.HasKey(LegacySettingsMusicVolumeKey))
        {
            return Mathf.Clamp01(PlayerPrefs.GetFloat(LegacySettingsMusicVolumeKey));
        }

        return Mathf.Clamp01(defaultVolume);
    }

    private void EnsureMusicSource()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
    }

    private void ConfigureAndPlayDefaultMusic()
    {
        if (defaultMusic != null && musicSource.clip == null)
        {
            musicSource.clip = defaultMusic;
        }

        if (musicSource.clip != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private void ApplyMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }

    private static void SyncLegacySettingsVolume(float volume)
    {
        PlayerPrefs.SetFloat(LegacySettingsMusicVolumeKey, Mathf.Clamp01(volume));
    }
}
