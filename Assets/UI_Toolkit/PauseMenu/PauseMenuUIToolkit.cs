using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

/// <summary>
/// Контроллер меню паузы на UI Toolkit в элитном стиле.
/// Поддерживает навигацию между главным меню и настройками с blur эффектом.
/// </summary>
public class PauseMenuUIToolkit : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("References")]
    [SerializeField] private HeadBob headBob;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private BlurEffect blurEffect;

    private InputAction pauseAction;
    private VisualElement pauseContainer;
    private VisualElement mainPausePanel;
    private VisualElement settingsPanel;
    private Slider cameraShakeSlider;
    private Label shakeValueLabel;
    private Button resumeButton;
    private Button settingsButton;
    private Button mainMenuButton;
    private Button settingsBackButton;
    private bool isOpen;

    private void Awake()
    {
        if (uiDocument == null)
            uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("UIDocument не найден! Добавьте компонент UIDocument к объекту.");
            return;
        }

        if (stateMachine == null)
            stateMachine = FindAnyObjectByType<PlayerStateMachine>();

        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player", true);
            pauseAction = playerMap?.FindAction("Pause", true);
        }

        InitializeUI();
    }

    private void InitializeUI()
    {
        var root = uiDocument.rootVisualElement;

        pauseContainer = root.Q<VisualElement>("PauseContainer");
        mainPausePanel = root.Q<VisualElement>("MainPausePanel");
        settingsPanel = root.Q<VisualElement>("SettingsPanel");
        cameraShakeSlider = root.Q<Slider>("CameraShakeSlider");
        shakeValueLabel = root.Q<Label>("ShakeValueLabel");
        resumeButton = root.Q<Button>("ResumeButton");
        settingsButton = root.Q<Button>("SettingsButton");
        mainMenuButton = root.Q<Button>("MainMenuButton");
        settingsBackButton = root.Q<Button>("SettingsBackButton");

        if (cameraShakeSlider != null)
        {
            cameraShakeSlider.lowValue = 0f;
            cameraShakeSlider.highValue = 2f;
            cameraShakeSlider.value = headBob != null ? headBob.BobIntensity : 1f;
            UpdateShakeValueLabel(cameraShakeSlider.value);
        }
    }

    private void OnEnable()
    {
        pauseAction?.Enable();
        RegisterCallbacks();
    }

    private void OnDisable()
    {
        pauseAction?.Disable();
        UnregisterCallbacks();
    }

    private void Start()
    {
        if (pauseContainer != null)
            pauseContainer.AddToClassList("hidden");
        ShowMainPausePanel();
        isOpen = false;
    }

    private void Update()
    {
        bool pausePressed = false;

        if (pauseAction != null && pauseAction.WasPerformedThisFrame())
            pausePressed = true;
        else if (Input.GetKeyDown(KeyCode.Escape))
            pausePressed = true;

        if (pausePressed)
        {
            if (isOpen)
                ClosePause();
            else if (stateMachine == null || stateMachine.CanOpenUI)
                OpenPause();
        }
    }

    private void RegisterCallbacks()
    {
        if (cameraShakeSlider != null)
            cameraShakeSlider.RegisterValueChangedCallback(OnShakeSliderChanged);

        if (resumeButton != null)
            resumeButton.clicked += ClosePause;

        if (settingsButton != null)
            settingsButton.clicked += ShowSettingsPanel;

        if (mainMenuButton != null)
            mainMenuButton.clicked += OnMainMenuClicked;

        if (settingsBackButton != null)
            settingsBackButton.clicked += ShowMainPausePanel;
    }

    private void UnregisterCallbacks()
    {
        if (cameraShakeSlider != null)
            cameraShakeSlider.UnregisterValueChangedCallback(OnShakeSliderChanged);

        if (resumeButton != null)
            resumeButton.clicked -= ClosePause;

        if (settingsButton != null)
            settingsButton.clicked -= ShowSettingsPanel;

        if (mainMenuButton != null)
            mainMenuButton.clicked -= OnMainMenuClicked;

        if (settingsBackButton != null)
            settingsBackButton.clicked -= ShowMainPausePanel;
    }

    private void OpenPause()
    {
        if (stateMachine != null && !stateMachine.TryEnterState(PlayerState.InUI))
            return;

        isOpen = true;
        if (pauseContainer != null)
            pauseContainer.RemoveFromClassList("hidden");

        if (blurEffect != null)
            blurEffect.EnableBlur();

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    /// <summary>
    /// Закрывает меню паузы и возвращает игрока в нормальное состояние.
    /// </summary>
    public void ClosePause()
    {
        stateMachine?.TryEnterState(PlayerState.Normal);
        isOpen = false;
        
        if (pauseContainer != null)
            pauseContainer.AddToClassList("hidden");

        if (blurEffect != null)
            blurEffect.DisableBlur();

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void OnShakeSliderChanged(ChangeEvent<float> evt)
    {
        if (headBob != null)
            headBob.BobIntensity = evt.newValue;
        
        UpdateShakeValueLabel(evt.newValue);
    }

    private void UpdateShakeValueLabel(float value)
    {
        if (shakeValueLabel != null)
            shakeValueLabel.text = $"{Mathf.RoundToInt(value * 100)}%";
    }

    private void ShowMainPausePanel()
    {
        if (mainPausePanel != null)
            mainPausePanel.RemoveFromClassList("hidden");
        if (settingsPanel != null)
            settingsPanel.AddToClassList("hidden");
    }

    private void ShowSettingsPanel()
    {
        if (mainPausePanel != null)
            mainPausePanel.AddToClassList("hidden");
        if (settingsPanel != null)
            settingsPanel.RemoveFromClassList("hidden");
    }

    private void OnMainMenuClicked()
    {
        ClosePause();
        SceneTransitionManager.Instance.LoadScene("MainMenu");
    }
}
