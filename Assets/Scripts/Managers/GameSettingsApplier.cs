using UnityEngine;

/// <summary>
/// Применяет сохраненные настройки при запуске игровой сцены.
/// Добавьте этот компонент на любой GameObject в игровой сцене.
/// </summary>
public class GameSettingsApplier : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HeadBob headBob;

    private void Awake()
    {
        ApplyAllSettings();
    }

    private void Start()
    {
        if (headBob != null)
        {
            headBob.BobIntensity = SettingsManager.CameraShake;
        }
    }

    private void ApplyAllSettings()
    {
        SettingsManager.ApplyGraphicsSettings();
        SettingsManager.ApplyAudioSettings();
    }
}
