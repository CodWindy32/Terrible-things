using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float acceleration = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask = ~0;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float verticalLookLimit = 85f;
    [SerializeField] private Transform cameraHolder;

    [Header("Input")]
    [SerializeField] private InputActionAsset inputActions;

    private CharacterController controller;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Vector3 velocity;
    private float verticalRotation;
    private float currentSpeed;
    private bool isGrounded;

    public Vector3 HorizontalVelocity { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        var playerMap = inputActions.FindActionMap("Player", true);
        moveAction = playerMap.FindAction("Move", true);
        lookAction = playerMap.FindAction("Look", true);
        jumpAction = playerMap.FindAction("Jump", true);
        sprintAction = playerMap.FindAction("Sprint", true);
    }

    private void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        UpdateGroundCheck();
        UpdateMouseLook();
        UpdateMovement();
        UpdateGravityAndJump();

        controller.Move(velocity * Time.deltaTime);
    }

    private void UpdateGroundCheck()
    {
        isGrounded = Physics.SphereCast(
            transform.position + Vector3.up * controller.radius,
            controller.radius * 0.95f,
            Vector3.down,
            out _,
            groundCheckDistance + controller.radius - controller.radius * 0.95f,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void UpdateMouseLook()
    {
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();

        float mouseX = lookDelta.x * mouseSensitivity;
        float mouseY = lookDelta.y * mouseSensitivity;

        transform.Rotate(Vector3.up, mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -verticalLookLimit, verticalLookLimit);
        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void UpdateMovement()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        IsSprinting = sprintAction.IsPressed() && input.y > 0.1f;

        float targetSpeed = IsSprinting ? sprintSpeed : walkSpeed;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);

        Vector3 moveDir = transform.right * input.x + transform.forward * input.y;
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        Vector3 horizontalMove = moveDir * currentSpeed;
        HorizontalVelocity = horizontalMove;

        velocity.x = horizontalMove.x;
        velocity.z = horizontalMove.z;

        if (input.sqrMagnitude < 0.01f)
            currentSpeed = 0f;
    }

    private void UpdateGravityAndJump()
    {
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        if (isGrounded && jumpAction.WasPerformedThisFrame())
            velocity.y = jumpForce;

        velocity.y += gravity * Time.deltaTime;
    }
}
