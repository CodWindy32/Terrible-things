using UnityEngine;
using DG.Tweening;

public class IdleBreathingEffect : MonoBehaviour
{
    [Header("Idle Detection")]
    [SerializeField] private float idleTimeToStart = 1.5f;
    [SerializeField] private float movementThreshold = 0.15f;

    [Header("Breathing")]
    [SerializeField] private float breathAmplitude = 0.025f;
    [SerializeField] private float breathHalfDuration = 1.4f;

    [Header("References")]
    [SerializeField] private HeadBob headBob;
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private PlayerStateMachine stateMachine;

    private float idleTimer;
    private float breathPhase;
    private Tween breathTween;

    private bool IsMoving => controller.HorizontalVelocity.sqrMagnitude >= movementThreshold * movementThreshold;

    private bool CanBreathe =>
        controller.IsGrounded &&
        !IsMoving &&
        stateMachine.HasMovement &&
        !stateMachine.IsInState(PlayerState.Climbing);

    private void Awake()
    {
        if (headBob == null) headBob = GetComponent<HeadBob>();
        if (controller == null) controller = GetComponentInParent<FirstPersonController>();
        if (stateMachine == null) stateMachine = GetComponentInParent<PlayerStateMachine>();
    }

    private void Update()
    {
        UpdateIdleState();
    }

    private void LateUpdate()
    {
        if (stateMachine.IsInState(PlayerState.IdleBreathing) && headBob != null)
            ApplyBreathing();
    }

    private void OnDestroy()
    {
        breathTween?.Kill();
    }

    private void UpdateIdleState()
    {
        if (!CanBreathe)
        {
            idleTimer = 0f;
            if (stateMachine.IsInState(PlayerState.IdleBreathing))
            {
                StopBreathing();
                stateMachine.TryEnterState(PlayerState.Normal);
            }
            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTimeToStart && stateMachine.TryEnterState(PlayerState.IdleBreathing))
            StartBreathing();
    }

    private void StartBreathing()
    {
        breathTween?.Kill();
        breathPhase = 0f;

        breathTween = DOTween.To(() => breathPhase, x => breathPhase = x, 1f, breathHalfDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StopBreathing()
    {
        breathTween?.Kill();
        breathTween = null;
        breathPhase = 0.5f;
    }

    private void ApplyBreathing()
    {
        float offsetY = (breathPhase - 0.5f) * 2f * breathAmplitude;
        Vector3 basePos = headBob.RestPosition;
        transform.localPosition = basePos + Vector3.up * offsetY;
    }
}
