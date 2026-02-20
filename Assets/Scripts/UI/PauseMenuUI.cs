using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject bgPause;
    [SerializeField] private RectTransform groupSnakingSettings;

    [Header("References")]
    [SerializeField] private HeadBob headBob;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private InputActionAsset inputActions;

    private InputAction pauseAction;
    private Slider shakeSlider;
    private TMP_Text valueTextTmp;
    private Text valueTextLegacy;
    private bool isOpen;

    private void Awake()
    {
        if (stateMachine == null)
            stateMachine = FindAnyObjectByType<PlayerStateMachine>();

        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player", true);
            pauseAction = playerMap.FindAction("Pause", true);
        }

        if (groupSnakingSettings != null)
        {
            shakeSlider = groupSnakingSettings.GetComponentInChildren<Slider>();
            if (shakeSlider != null)
            {
                shakeSlider.minValue = 0f;
                shakeSlider.maxValue = 2f;
                shakeSlider.value = headBob != null ? headBob.BobIntensity : 1f;
                shakeSlider.onValueChanged.AddListener(OnSliderChanged);

                valueTextTmp = groupSnakingSettings.GetComponentInChildren<TMP_Text>();
                valueTextLegacy = valueTextTmp == null ? groupSnakingSettings.GetComponentInChildren<Text>() : null;
                UpdateValueText(shakeSlider.value);
            }
        }
    }

    private void OnEnable()
    {
        pauseAction?.Enable();
    }

    private void OnDisable()
    {
        pauseAction?.Disable();
    }

    private void Start()
    {
        if (bgPause != null)
            bgPause.SetActive(false);
        isOpen = false;
    }

    private void Update()
    {
        if (pauseAction != null && pauseAction.WasPerformedThisFrame())
        {
            if (isOpen)
                ClosePause();
            else if (stateMachine != null && stateMachine.CanOpenUI)
                OpenPause();
        }
    }

    private void OpenPause()
    {
        if (stateMachine != null && !stateMachine.TryEnterState(PlayerState.InUI))
            return;

        isOpen = true;
        if (bgPause != null)
            bgPause.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>Закрывает паузу. Вызывается по Esc или кнопкой «Продолжить игру» (привязать в OnClick).</summary>
    public void ClosePause()
    {
        stateMachine?.TryEnterState(PlayerState.Normal);
        isOpen = false;
        if (bgPause != null)
            bgPause.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnSliderChanged(float value)
    {
        if (headBob != null)
            headBob.BobIntensity = value;
        UpdateValueText(value);
    }

    private void UpdateValueText(float value)
    {
        string text = $"{Mathf.RoundToInt(value * 100)}%";
        if (valueTextTmp != null)
            valueTextTmp.text = text;
        else if (valueTextLegacy != null)
            valueTextLegacy.text = text;
    }
}
