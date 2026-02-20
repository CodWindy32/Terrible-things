using UnityEngine;

public class HeadBob : MonoBehaviour
{
    private const float MovementSpeedThreshold = 0.2f;

    [Header("Bob Settings")]
    [SerializeField] private float walkBobSpeed = 10f;
    [SerializeField] private float sprintBobSpeed = 14f;
    [SerializeField] private float walkBobAmountY = 0.035f;
    [SerializeField] private float walkBobAmountX = 0.02f;
    [SerializeField] private float sprintBobAmountY = 0.05f;
    [SerializeField] private float sprintBobAmountX = 0.035f;

    [Header("Intensity")]
    [SerializeField, Range(0f, 2f)] private float bobIntensity = 1f;

    [Header("Smoothing")]
    [SerializeField] private float bobSmoothing = 12f;
    [SerializeField] private float resetSmoothing = 6f;

    [Header("Landing Bob")]
    [SerializeField] private float landingDropAmount = 0.08f;
    [SerializeField] private float landingRecoverySpeed = 8f;

    [Header("References")]
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private CrouchHandler crouchHandler;

    private float bobTimer;
    private Vector3 targetBobOffset;
    private Vector3 currentBobOffset;
    private Vector3 baseRestPosition;
    private float landingOffset;
    private bool wasGrounded;

    public Vector3 RestPosition => new Vector3(baseRestPosition.x, crouchHandler != null ? crouchHandler.CurrentRestY : baseRestPosition.y, baseRestPosition.z);

    public float BobIntensity
    {
        get => bobIntensity;
        set => bobIntensity = Mathf.Clamp(value, 0f, 2f);
    }

    private void Start()
    {
        if (crouchHandler == null)
            crouchHandler = GetComponentInParent<CrouchHandler>();
        baseRestPosition = transform.localPosition;
    }

    private void Update()
    {
        HandleLandingBob();
        HandleHeadBob();
        ApplyBob();
    }

    private void HandleHeadBob()
    {
        float speed = controller.HorizontalVelocity.magnitude;
        bool isMoving = speed > MovementSpeedThreshold && controller.IsGrounded;

        if (isMoving)
        {
            bool sprinting = controller.IsSprinting;
            float bobSpeed = sprinting ? sprintBobSpeed : walkBobSpeed;
            float amountY = (sprinting ? sprintBobAmountY : walkBobAmountY) * bobIntensity;
            float amountX = (sprinting ? sprintBobAmountX : walkBobAmountX) * bobIntensity;

            bobTimer += Time.deltaTime * bobSpeed;

            float yOffset = Mathf.Sin(bobTimer) * amountY;
            float xOffset = Mathf.Sin(bobTimer * 0.5f) * amountX;

            targetBobOffset = new Vector3(xOffset, yOffset, 0f);
        }
        else
        {
            bobTimer = Mathf.Lerp(bobTimer, 0f, Time.deltaTime * resetSmoothing);
            targetBobOffset = Vector3.zero;
        }
    }

    private void HandleLandingBob()
    {
        if (controller.IsGrounded && !wasGrounded)
            landingOffset = -landingDropAmount;

        landingOffset = Mathf.Lerp(landingOffset, 0f, Time.deltaTime * landingRecoverySpeed);
        wasGrounded = controller.IsGrounded;
    }

    private void ApplyBob()
    {
        float smoothing = targetBobOffset.sqrMagnitude > currentBobOffset.sqrMagnitude
            ? bobSmoothing
            : resetSmoothing;

        currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * smoothing);
        transform.localPosition = RestPosition + currentBobOffset + Vector3.up * landingOffset;
    }

    public void ResetBobState()
    {
        bobTimer = 0f;
        targetBobOffset = Vector3.zero;
        currentBobOffset = Vector3.zero;
        landingOffset = 0f;
    }
}
