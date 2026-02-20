using UnityEngine;
using DG.Tweening;

public class FallImpactEffect : MonoBehaviour
{
    [Header("Fall Detection")]
    [SerializeField] private float fallSpeedThreshold = 15f;

    [Header("Kneel")]
    [SerializeField] private float kneelDrop = 0.5f;
    [SerializeField] private float kneelDuration = 0.18f;

    [Header("Head Shake")]
    [SerializeField] private float shakeDuration = 0.7f;
    [SerializeField] private float shakeStrength = 8f;
    [SerializeField] private int shakeVibrato = 6;

    [Header("Stand Up")]
    [SerializeField] private float standUpDuration = 0.65f;

    [Header("References")]
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private HeadBob headBob;

    private float lastAirborneVelocityY;
    private bool wasGrounded;

    private void Awake()
    {
        if (stateMachine == null)
            stateMachine = GetComponentInParent<PlayerStateMachine>();
        if (controller == null)
            controller = GetComponentInParent<FirstPersonController>();
        if (cameraHolder == null)
            cameraHolder = transform;
        if (headBob == null)
            headBob = GetComponent<HeadBob>();
    }

    private void Update()
    {
        if (stateMachine != null && !stateMachine.IsInState(PlayerState.Normal))
            return;

        if (!controller.IsGrounded)
            lastAirborneVelocityY = controller.VerticalVelocity;

        if (controller.IsGrounded && !wasGrounded)
        {
            if (Mathf.Abs(lastAirborneVelocityY) >= fallSpeedThreshold)
                PlayFallImpact();

            lastAirborneVelocityY = 0f;
        }

        wasGrounded = controller.IsGrounded;
    }

    private void PlayFallImpact()
    {
        if (stateMachine != null && !stateMachine.TryEnterState(PlayerState.FallImpact))
            return;

        if (headBob != null)
            headBob.enabled = false;

        Vector3 startPos = cameraHolder.localPosition;
        Quaternion startRot = cameraHolder.localRotation;

        var seq = DOTween.Sequence();

        seq.Append(
            cameraHolder.DOLocalMoveY(startPos.y - kneelDrop, kneelDuration)
                .SetEase(Ease.InQuad)
        );

        seq.Append(
            cameraHolder.DOPunchRotation(
                new Vector3(shakeStrength, 0f, shakeStrength * 0.4f),
                shakeDuration,
                shakeVibrato,
                0.3f
            )
        );

        seq.Append(
            cameraHolder.DOLocalMoveY(startPos.y, standUpDuration)
                .SetEase(Ease.OutCubic)
        );

        seq.OnComplete(() =>
        {
            cameraHolder.localPosition = startPos;
            cameraHolder.localRotation = startRot;

            if (headBob != null)
            {
                headBob.ResetBobState();
                headBob.enabled = true;
            }

            stateMachine?.TryEnterState(PlayerState.Normal);
        });
    }

    private void OnDestroy()
    {
        DOTween.Kill(cameraHolder);
    }
}
