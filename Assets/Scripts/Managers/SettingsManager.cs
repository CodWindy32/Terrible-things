using UnityEngine;

/// <summary>
/// Статический класс для доступа к настройкам игры из любого места.
/// Все настройки сохраняются в PlayerPrefs через главное меню.
/// </summary>
public static class SettingsManager
{
    private const string KEY_QUALITY = "Settings_Quality";
    private const string KEY_RESOLUTION = "Settings_Resolution";
    private const string KEY_FULLSCREEN = "Settings_Fullscreen";
    private const string KEY_VSYNC = "Settings_VSync";
    private const string KEY_MASTER_VOLUME = "Settings_MasterVolume";
    private const string KEY_MUSIC_VOLUME = "Settings_MusicVolume";
    private const string KEY_SFX_VOLUME = "Settings_SfxVolume";
    private const string KEY_MOUSE_SENSITIVITY = "Settings_MouseSensitivity";
    private const string KEY_INVERT_Y = "Settings_InvertY";
    private const string KEY_CAMERA_SHAKE = "Settings_CameraShake";

    public static int Quality => PlayerPrefs.GetInt(KEY_QUALITY, 2);
    
    public static bool Fullscreen => PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;
    
    public static bool VSync => PlayerPrefs.GetInt(KEY_VSYNC, 1) == 1;
    
    public static float MasterVolume => PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, 100f) / 100f;
    
    public static float MusicVolume => PlayerPrefs.GetFloat(KEY_MUSIC_VOLUME, 80f) / 100f;
    
    public static float SfxVolume => PlayerPrefs.GetFloat(KEY_SFX_VOLUME, 100f) / 100f;
    
    public static float MouseSensitivity => PlayerPrefs.GetFloat(KEY_MOUSE_SENSITIVITY, 50f) / 100f;
    
    public static bool InvertY => PlayerPrefs.GetInt(KEY_INVERT_Y, 0) == 1;
    
    public static float CameraShake => PlayerPrefs.GetFloat(KEY_CAMERA_SHAKE, 1f);

    public static void ApplyGraphicsSettings()
    {
        QualitySettings.SetQualityLevel(Quality);
        QualitySettings.vSyncCount = VSync ? 1 : 0;
        Screen.fullScreen = Fullscreen;
    }

    public static void ApplyAudioSettings()
    {
        AudioListener.volume = MasterVolume;
    }

    public static void ResetToDefaults()
    {
        PlayerPrefs.DeleteKey(KEY_QUALITY);
        PlayerPrefs.DeleteKey(KEY_RESOLUTION);
        PlayerPrefs.DeleteKey(KEY_FULLSCREEN);
        PlayerPrefs.DeleteKey(KEY_VSYNC);
        PlayerPrefs.DeleteKey(KEY_MASTER_VOLUME);
        PlayerPrefs.DeleteKey(KEY_MUSIC_VOLUME);
        PlayerPrefs.DeleteKey(KEY_SFX_VOLUME);
        PlayerPrefs.DeleteKey(KEY_MOUSE_SENSITIVITY);
        PlayerPrefs.DeleteKey(KEY_INVERT_Y);
        PlayerPrefs.DeleteKey(KEY_CAMERA_SHAKE);
        PlayerPrefs.Save();
    }
}
