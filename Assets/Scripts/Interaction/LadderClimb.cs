using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class LadderClimb : MonoBehaviour
{
    [Header("Climbing")]
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float sprintClimbSpeed = 5f;

    [Header("Jump Off")]
    [SerializeField] private float jumpOffForce = 5f;
    [SerializeField] private float jumpOffUpForce = 3f;

    [Header("Interaction")]
    [SerializeField] private string promptMessage = "Нажмите [F] чтобы взобраться";

    [Header("References")]
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private PlayerStateMachine stateMachine;
    [SerializeField] private InputActionAsset inputActions;
    [SerializeField] private TMP_Text promptMessageUI;

    private InputAction interactAction;
    private LadderZone nearbyLadder;
    private LadderZone currentLadder;
    private bool isClimbing;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<FirstPersonController>();
        if (stateMachine == null)
            stateMachine = GetComponent<PlayerStateMachine>();

        HidePrompt();
        if (inputActions != null)
        {
            var playerMap = inputActions.FindActionMap("Player", true);
            interactAction = playerMap.FindAction("Interact", true);
        }
    }

    private void OnEnable()
    {
        interactAction?.Enable();
    }

    private void OnDisable()
    {
        interactAction?.Disable();
        HidePrompt();
    }

    private void Update()
    {
        if (isClimbing)
        {
            UpdateClimbing();
            return;
        }

        if (nearbyLadder != null && stateMachine != null && stateMachine.CanOpenUI)
        {
            ShowPrompt(promptMessage);

            if (interactAction != null && interactAction.WasPerformedThisFrame())
                StartClimbing(nearbyLadder);
        }
        else
        {
            HidePrompt();
        }
    }

    private void StartClimbing(LadderZone ladder)
    {
        if (stateMachine != null && !stateMachine.TryEnterState(PlayerState.Climbing))
            return;

        currentLadder = ladder;
        nearbyLadder = null;
        isClimbing = true;

        controller.ResetVelocity();
        controller.Controller.enabled = false;

        HidePrompt();

        float clampedY = Mathf.Clamp(
            transform.position.y,
            ladder.LadderBottom.position.y,
            ladder.LadderTop.position.y);

        transform.position = ladder.GetClimbPosition(clampedY);

        Vector3 faceDir = -ladder.OutwardDirection;
        faceDir.y = 0f;
        if (faceDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(faceDir);
    }

    private void UpdateClimbing()
    {
        if (currentLadder == null)
        {
            StopClimbing();
            return;
        }

        if (controller.JumpAction.WasPerformedThisFrame())
        {
            JumpOff();
            return;
        }

        Vector2 moveInput = controller.MoveAction.ReadValue<Vector2>();
        bool sprint = controller.SprintAction.IsPressed();
        float speed = sprint ? sprintClimbSpeed : climbSpeed;
        float deltaY = moveInput.y * speed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.y += deltaY;

        float bottomY = currentLadder.LadderBottom.position.y;
        float topY = currentLadder.LadderTop.position.y;

        if (pos.y >= topY)
        {
            DismountTop();
            return;
        }

        if (pos.y <= bottomY && moveInput.y < -0.1f)
        {
            StopClimbing();
            return;
        }

        pos.y = Mathf.Clamp(pos.y, bottomY, topY);
        transform.position = currentLadder.GetClimbPosition(pos.y);
    }

    private void DismountTop()
    {
        if (currentLadder.DismountPoint != null)
            transform.position = currentLadder.DismountPoint.position;
        else
            transform.position = currentLadder.LadderTop.position
                                 + currentLadder.OutwardDirection * 0.8f
                                 + Vector3.up * 0.1f;

        StopClimbing();
    }

    private void JumpOff()
    {
        Vector3 kickDir = currentLadder.OutwardDirection.normalized;
        StopClimbing();
        controller.SetVelocity(kickDir * jumpOffForce + Vector3.up * jumpOffUpForce);
    }

    private void StopClimbing()
    {
        isClimbing = false;
        currentLadder = null;
        controller.Controller.enabled = true;
        stateMachine?.TryEnterState(PlayerState.Normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isClimbing) return;
        var ladder = other.GetComponent<LadderZone>();
        if (ladder != null)
            nearbyLadder = ladder;
    }

    private void OnTriggerExit(Collider other)
    {
        if (isClimbing) return;
        var ladder = other.GetComponent<LadderZone>();
        if (ladder != null && ladder == nearbyLadder)
            nearbyLadder = null;
    }

    private void ShowPrompt(string text)
    {
        if (promptMessageUI != null)
        {
            promptMessageUI.text = text;
            promptMessageUI.gameObject.SetActive(true);
        }
    }

    private void HidePrompt()
    {
        if (promptMessageUI != null)
            promptMessageUI.gameObject.SetActive(false);
    }
}
