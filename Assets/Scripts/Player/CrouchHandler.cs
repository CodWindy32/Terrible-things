using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CrouchHandler : MonoBehaviour
{
    [Header("Height")]
    [SerializeField] private float crouchHeightRatio = 0.55f;

    [Header("Transition")]
    [SerializeField] private float transitionDuration = 0.2f;

    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private InputActionAsset inputActions;

    private float standingHeight;
    private float crouchHeight;
    private float standingCameraY;
    private float crouchCameraY;
    private float currentHeightFactor;
    private Tween heightTween;
    private InputAction crouchAction;

    public bool IsCrouching { get; private set; }

    /// <summary>Current camera rest height (Y). Use for HeadBob/IdleBreathing base position.</summary>
    public float CurrentRestY => cameraPivot != null ? Mathf.Lerp(crouchCameraY, standingCameraY, currentHeightFactor) : standingCameraY;

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (stateMachine == null)
            stateMachine = GetComponent<PlayerStateMachine>();
        if (cameraPivot == null)
        {
            var cam = GetComponentInChildren<Camera>();
            if (cam != null)
                cameraPivot = cam.transform.parent != transform ? cam.transform.parent : cam.transform;
        }

        standingHeight = characterController.height;
        crouchHeight = standingHeight * Mathf.Clamp01(crouchHeightRatio);
        currentHeightFactor = 1f;

        if (cameraPivot != null)
        {
            standingCameraY = cameraPivot.localPosition.y;
            crouchCameraY = standingCameraY * Mathf.Clamp01(crouchHeightRatio);
        }
        else
        {
            standingCameraY = standingHeight * 0.9f;
            crouchCameraY = standingCameraY * Mathf.Clamp01(crouchHeightRatio);
        }

        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player", true);
            crouchAction = playerMap.FindAction("Crouch", true);
        }
    }

    private void OnEnable()
    {
        crouchAction?.Enable();
    }

    private void OnDisable()
    {
        crouchAction?.Disable();
    }

    private void Update()
    {
        if (stateMachine != null && !stateMachine.HasMovement)
        {
            if (IsCrouching)
            {
                IsCrouching = false;
                AnimateHeight(1f);
            }
            return;
        }

        bool wantCrouch = crouchAction != null && crouchAction.IsPressed();

        if (wantCrouch != IsCrouching)
        {
            IsCrouching = wantCrouch;
            AnimateHeight(IsCrouching ? 0f : 1f);
        }
    }

    private void AnimateHeight(float targetFactor)
    {
        heightTween?.Kill();

        heightTween = DOTween.To(() => currentHeightFactor, x => currentHeightFactor = x, targetFactor, transitionDuration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(ApplyHeight);
    }

    private void ApplyHeight()
    {
        float h = Mathf.Lerp(crouchHeight, standingHeight, currentHeightFactor);
        characterController.height = h;
        characterController.center = new Vector3(0f, h * 0.5f, 0f);
    }

    private void OnDestroy()
    {
        heightTween?.Kill();
    }
}
