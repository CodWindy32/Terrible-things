using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Контроллер главного меню с полными настройками в элитном стиле.
/// Управляет навигацией, настройками и переходами между сценами.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Blur Effect")]
    [SerializeField] private BlurEffect blurEffect;

    [Header("Scene Names")]
    [SerializeField] private string videoIntroSceneName = "VideoIntro";

    private VisualElement mainPanel;
    private VisualElement settingsPanel;

    private Button startGameButton;
    private Button settingsButton;
    private Button exitButton;
    private Button settingsBackButton;

    private DropdownField qualityDropdown;
    private DropdownField resolutionDropdown;
    private Toggle fullscreenToggle;
    private Toggle vsyncToggle;

    private Slider masterVolumeSlider;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private Label masterVolumeLabel;
    private Label musicVolumeLabel;
    private Label sfxVolumeLabel;

    private Slider mouseSensitivitySlider;
    private Toggle invertYToggle;
    private Slider cameraShakeSlider;
    private Label mouseSensitivityLabel;
    private Label cameraShakeLabel;

    private List<Resolution> availableResolutions;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument не найден!");
            return;
        }

        InitializeUI();
        LoadSettings();
    }

    private void OnEnable()
    {
        RegisterCallbacks();
        
        if (blurEffect != null)
            blurEffect.EnableBlur();
    }

    private void OnDisable()
    {
        UnregisterCallbacks();
    }

    private void Start()
    {
        ShowMainPanel();
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void InitializeUI()
    {
        var root = uiDocument.rootVisualElement;

        mainPanel = root.Q<VisualElement>("MainPanel");
        settingsPanel = root.Q<VisualElement>("SettingsPanel");

        startGameButton = root.Q<Button>("StartGameButton");
        settingsButton = root.Q<Button>("SettingsButton");
        exitButton = root.Q<Button>("ExitButton");
        settingsBackButton = root.Q<Button>("SettingsBackButton");

        qualityDropdown = root.Q<DropdownField>("QualityDropdown");
        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        vsyncToggle = root.Q<Toggle>("VSyncToggle");

        masterVolumeSlider = root.Q<Slider>("MasterVolumeSlider");
        musicVolumeSlider = root.Q<Slider>("MusicVolumeSlider");
        sfxVolumeSlider = root.Q<Slider>("SfxVolumeSlider");
        masterVolumeLabel = root.Q<Label>("MasterVolumeLabel");
        musicVolumeLabel = root.Q<Label>("MusicVolumeLabel");
        sfxVolumeLabel = root.Q<Label>("SfxVolumeLabel");

        mouseSensitivitySlider = root.Q<Slider>("MouseSensitivitySlider");
        invertYToggle = root.Q<Toggle>("InvertYToggle");
        cameraShakeSlider = root.Q<Slider>("CameraShakeSlider");
        mouseSensitivityLabel = root.Q<Label>("MouseSensitivityLabel");
        cameraShakeLabel = root.Q<Label>("CameraShakeLabel");

        InitializeResolutions();
    }

    private void InitializeResolutions()
    {
        availableResolutions = Screen.resolutions
            .Where(r => r.refreshRateRatio.value >= 59)
            .Distinct()
            .OrderByDescending(r => r.width * r.height)
            .ToList();

        if (resolutionDropdown != null && availableResolutions.Count > 0)
        {
            var resolutionStrings = availableResolutions
                .Select(r => $"{r.width} x {r.height}")
                .ToList();

            resolutionDropdown.choices = resolutionStrings;

            var currentResolution = Screen.currentResolution;
            int currentIndex = availableResolutions.FindIndex(r =>
                r.width == currentResolution.width && r.height == currentResolution.height);

            resolutionDropdown.index = currentIndex >= 0 ? currentIndex : 0;
        }
    }

    private void RegisterCallbacks()
    {
        if (startGameButton != null)
            startGameButton.clicked += OnStartGameClicked;

        if (settingsButton != null)
            settingsButton.clicked += ShowSettingsPanel;

        if (exitButton != null)
            exitButton.clicked += OnExitClicked;

        if (settingsBackButton != null)
            settingsBackButton.clicked += OnSettingsBackClicked;

        if (qualityDropdown != null)
            qualityDropdown.RegisterValueChangedCallback(OnQualityChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);

        if (vsyncToggle != null)
            vsyncToggle.RegisterValueChangedCallback(OnVSyncChanged);

        if (masterVolumeSlider != null)
            masterVolumeSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.RegisterValueChangedCallback(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.RegisterValueChangedCallback(OnSfxVolumeChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.RegisterValueChangedCallback(OnMouseSensitivityChanged);

        if (invertYToggle != null)
            invertYToggle.RegisterValueChangedCallback(OnInvertYChanged);

        if (cameraShakeSlider != null)
            cameraShakeSlider.RegisterValueChangedCallback(OnCameraShakeChanged);
    }

    private void UnregisterCallbacks()
    {
        if (startGameButton != null)
            startGameButton.clicked -= OnStartGameClicked;

        if (settingsButton != null)
            settingsButton.clicked -= ShowSettingsPanel;

        if (exitButton != null)
            exitButton.clicked -= OnExitClicked;

        if (settingsBackButton != null)
            settingsBackButton.clicked -= OnSettingsBackClicked;

        if (qualityDropdown != null)
            qualityDropdown.UnregisterValueChangedCallback(OnQualityChanged);

        if (resolutionDropdown != null)
            resolutionDropdown.UnregisterValueChangedCallback(OnResolutionChanged);

        if (fullscreenToggle != null)
            fullscreenToggle.UnregisterValueChangedCallback(OnFullscreenChanged);

        if (vsyncToggle != null)
            vsyncToggle.UnregisterValueChangedCallback(OnVSyncChanged);

        if (masterVolumeSlider != null)
            masterVolumeSlider.UnregisterValueChangedCallback(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.UnregisterValueChangedCallback(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.UnregisterValueChangedCallback(OnSfxVolumeChanged);

        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.UnregisterValueChangedCallback(OnMouseSensitivityChanged);

        if (invertYToggle != null)
            invertYToggle.UnregisterValueChangedCallback(OnInvertYChanged);

        if (cameraShakeSlider != null)
            cameraShakeSlider.UnregisterValueChangedCallback(OnCameraShakeChanged);
    }

    private void ShowMainPanel()
    {
        if (mainPanel != null)
            mainPanel.RemoveFromClassList("hidden");
        if (settingsPanel != null)
            settingsPanel.AddToClassList("hidden");
    }

    private void ShowSettingsPanel()
    {
        if (mainPanel != null)
            mainPanel.AddToClassList("hidden");
        if (settingsPanel != null)
            settingsPanel.RemoveFromClassList("hidden");
    }

    private void OnStartGameClicked()
    {
        SceneTransitionManager.Instance.LoadScene(videoIntroSceneName);
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnSettingsBackClicked()
    {
        SaveSettings();
        ShowMainPanel();
    }

    private void OnQualityChanged(ChangeEvent<string> evt)
    {
        if (qualityDropdown != null)
        {
            QualitySettings.SetQualityLevel(qualityDropdown.index);
            PlayerPrefs.SetInt("Settings_Quality", qualityDropdown.index);
        }
    }

    private void OnResolutionChanged(ChangeEvent<string> evt)
    {
        if (resolutionDropdown != null && resolutionDropdown.index >= 0 && resolutionDropdown.index < availableResolutions.Count)
        {
            var resolution = availableResolutions[resolutionDropdown.index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
            PlayerPrefs.SetString("Settings_Resolution", $"{resolution.width}x{resolution.height}");
        }
    }

    private void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        Screen.fullScreen = evt.newValue;
        PlayerPrefs.SetInt("Settings_Fullscreen", evt.newValue ? 1 : 0);
    }

    private void OnVSyncChanged(ChangeEvent<bool> evt)
    {
        QualitySettings.vSyncCount = evt.newValue ? 1 : 0;
        PlayerPrefs.SetInt("Settings_VSync", evt.newValue ? 1 : 0);
    }

    private void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        AudioListener.volume = evt.newValue / 100f;
        if (masterVolumeLabel != null)
            masterVolumeLabel.text = $"{Mathf.RoundToInt(evt.newValue)}%";
        PlayerPrefs.SetFloat("Settings_MasterVolume", evt.newValue);
    }

    private void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        if (musicVolumeLabel != null)
            musicVolumeLabel.text = $"{Mathf.RoundToInt(evt.newValue)}%";
        PlayerPrefs.SetFloat("Settings_MusicVolume", evt.newValue);
    }

    private void OnSfxVolumeChanged(ChangeEvent<float> evt)
    {
        if (sfxVolumeLabel != null)
            sfxVolumeLabel.text = $"{Mathf.RoundToInt(evt.newValue)}%";
        PlayerPrefs.SetFloat("Settings_SfxVolume", evt.newValue);
    }

    private void OnMouseSensitivityChanged(ChangeEvent<float> evt)
    {
        if (mouseSensitivityLabel != null)
            mouseSensitivityLabel.text = $"{Mathf.RoundToInt(evt.newValue)}%";
        PlayerPrefs.SetFloat("Settings_MouseSensitivity", evt.newValue);
    }

    private void OnInvertYChanged(ChangeEvent<bool> evt)
    {
        PlayerPrefs.SetInt("Settings_InvertY", evt.newValue ? 1 : 0);
    }

    private void OnCameraShakeChanged(ChangeEvent<float> evt)
    {
        if (cameraShakeLabel != null)
            cameraShakeLabel.text = $"{Mathf.RoundToInt(evt.newValue * 100)}%";
        PlayerPrefs.SetFloat("Settings_CameraShake", evt.newValue);
    }

    private void LoadSettings()
    {
        if (qualityDropdown != null)
            qualityDropdown.index = PlayerPrefs.GetInt("Settings_Quality", 2);

        if (fullscreenToggle != null)
            fullscreenToggle.value = PlayerPrefs.GetInt("Settings_Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        if (vsyncToggle != null)
            vsyncToggle.value = PlayerPrefs.GetInt("Settings_VSync", QualitySettings.vSyncCount) == 1;

        if (masterVolumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("Settings_MasterVolume", 100f);
            masterVolumeSlider.value = volume;
            if (masterVolumeLabel != null)
                masterVolumeLabel.text = $"{Mathf.RoundToInt(volume)}%";
        }

        if (musicVolumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("Settings_MusicVolume", 80f);
            musicVolumeSlider.value = volume;
            if (musicVolumeLabel != null)
                musicVolumeLabel.text = $"{Mathf.RoundToInt(volume)}%";
        }

        if (sfxVolumeSlider != null)
        {
            float volume = PlayerPrefs.GetFloat("Settings_SfxVolume", 100f);
            sfxVolumeSlider.value = volume;
            if (sfxVolumeLabel != null)
                sfxVolumeLabel.text = $"{Mathf.RoundToInt(volume)}%";
        }

        if (mouseSensitivitySlider != null)
        {
            float sensitivity = PlayerPrefs.GetFloat("Settings_MouseSensitivity", 50f);
            mouseSensitivitySlider.value = sensitivity;
            if (mouseSensitivityLabel != null)
                mouseSensitivityLabel.text = $"{Mathf.RoundToInt(sensitivity)}%";
        }

        if (invertYToggle != null)
            invertYToggle.value = PlayerPrefs.GetInt("Settings_InvertY", 0) == 1;

        if (cameraShakeSlider != null)
        {
            float shake = PlayerPrefs.GetFloat("Settings_CameraShake", 1f);
            cameraShakeSlider.value = shake;
            if (cameraShakeLabel != null)
                cameraShakeLabel.text = $"{Mathf.RoundToInt(shake * 100)}%";
        }
    }

    private void SaveSettings()
    {
        PlayerPrefs.Save();
    }
}
