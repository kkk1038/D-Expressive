using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>Click the prototype room objects to use their day-aware actions.</summary>
public class PrototypeInteractionController : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Camera interactionCamera;

    private void Awake()
    {
        if (dayManager == null) dayManager = FindFirstObjectByType<DayManager>();
        if (dialogueManager == null) dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (interactionCamera == null) interactionCamera = Camera.main;
    }

    private void Update()
    {
        if (FindFirstObjectByType<FirstPersonPlayerController>() != null) return;
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = interactionCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PrototypeInteractionTarget target = hit.collider.GetComponentInParent<PrototypeInteractionTarget>();
            if (target != null) Interact(target.Type);
        }
    }

    public static void Interact(PrototypeInteractionTarget.InteractionType type)
    {
        PrototypeInteractionController controller = FindFirstObjectByType<PrototypeInteractionController>();
        if (controller == null) return;

        controller.PerformInteraction(type);
    }

    private void PerformInteraction(PrototypeInteractionTarget.InteractionType type)
    {
        if (type != PrototypeInteractionTarget.InteractionType.Sleep)
        {
            dayManager.RegisterDailyAction();
        }

        switch (type)
        {
            case PrototypeInteractionTarget.InteractionType.Sleep:
                dayManager.GoToSleep();
                break;
            case PrototypeInteractionTarget.InteractionType.Phone:
                FirstPersonPlayerController.UnlockCursor();
                StartDayDialogue("phone");
                break;
            case PrototypeInteractionTarget.InteractionType.Window:
                FirstPersonPlayerController.UnlockCursor();
                StartDayDialogue("window");
                break;
            case PrototypeInteractionTarget.InteractionType.Meal:
                StartOneTimeDayDialogue("meal");
                break;
            case PrototypeInteractionTarget.InteractionType.Medicine:
                StartOneTimeDayDialogue("medicine");
                break;
            case PrototypeInteractionTarget.InteractionType.Door:
                FirstPersonPlayerController.UnlockCursor();
                StartDayDialogue("door");
                break;
            case PrototypeInteractionTarget.InteractionType.Mirror:
                StartOneTimeDayDialogue("mirror");
                break;
            case PrototypeInteractionTarget.InteractionType.Shower:
                StartOneTimeDayDialogue("shower");
                break;
            case PrototypeInteractionTarget.InteractionType.Tidy:
                StartOneTimeDayDialogue("tidy");
                break;
        }
    }

    private void StartDayDialogue(string interactionId)
    {
        dialogueManager.StartDialogue("day" + dayManager.CurrentDay + "_" + interactionId);
    }

    private void StartOneTimeDayDialogue(string interactionId)
    {
        string nodeId = "day" + dayManager.CurrentDay + "_" + interactionId;
        if (GameStateManager.Instance.CurrentState.choiceHistory.Exists(choice => choice.choiceId == nodeId))
        {
            Debug.Log("오늘은 이미 이 행동을 했습니다.");
            return;
        }

        // Only release the cursor after confirming that a dialogue will open.
        FirstPersonPlayerController.UnlockCursor();
        dialogueManager.StartDialogue(nodeId);
    }
}
