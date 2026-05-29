using UnityEngine;

public enum Difficulty
{
    Easy,
    Normal,
    Hard
}

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    // Volume 0..1
    public float SfxVolume { get; private set; } = 1f;
    public Difficulty CurrentDifficulty { get; private set; } = Difficulty.Normal;

    private const string VOLUME_KEY = "Settings_Volume";
    private const string DIFFICULTY_KEY = "Settings_Difficulty";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void LoadSettings()
    {
        SfxVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        int diffIndex = PlayerPrefs.GetInt(DIFFICULTY_KEY, 1); // default Normal
        CurrentDifficulty = (Difficulty)Mathf.Clamp(diffIndex, 0, 2);
    }

    public void SetVolume(float value)
    {
        SfxVolume = Mathf.Clamp01(value);
        PlayerPrefs.SetFloat(VOLUME_KEY, SfxVolume);
        PlayerPrefs.Save();
        UpdateAudioSources();
    }

    public void SetDifficulty(int index)
    {
        CurrentDifficulty = (Difficulty)Mathf.Clamp(index, 0, 2);
        PlayerPrefs.SetInt(DIFFICULTY_KEY, (int)CurrentDifficulty);
        PlayerPrefs.Save();
    }

    public void SetDifficulty(Difficulty diff)
    {
        CurrentDifficulty = diff;
        PlayerPrefs.SetInt(DIFFICULTY_KEY, (int)diff);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Applies current volume to all AudioSources tagged with "SFX" in the active scene.
    /// Called after volume change or on scene load.
    /// </summary>
    public void UpdateAudioSources()
    {
        foreach (var src in FindObjectsOfType<AudioSource>())
        {
            if (src.CompareTag("SFX"))
            {
                src.volume = SfxVolume;
            }
        }
    }

    // Difficulty multipliers for gameplay
    public float GetEnemySpeedMultiplier()
    {
        switch (CurrentDifficulty)
        {
            case Difficulty.Easy:   return 0.5f;
            case Difficulty.Hard:   return 1.8f;
            default:                return 1.0f;
        }
    }

    public float GetPlayerSpeedMultiplier()
    {
        switch (CurrentDifficulty)
        {
            case Difficulty.Easy:   return 1.3f;
            case Difficulty.Hard:   return 0.8f;
            default:                return 1.0f;
        }
    }

    public float GetJumpForceMultiplier()
    {
        switch (CurrentDifficulty)
        {
            case Difficulty.Easy:   return 1.2f;
            case Difficulty.Hard:   return 0.8f;
            default:                return 1.0f;
        }
    }

    public string GetDifficultyName()
    {
        switch (CurrentDifficulty)
        {
            case Difficulty.Easy:   return "Easy";
            case Difficulty.Hard:   return "Hard";
            default:                return "Normal";
        }
    }
}