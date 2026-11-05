using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

/// <summary>Click the prototype room objects to use their day-aware actions.</summary>
public class PrototypeInteractionController : MonoBehaviour
{
    private static readonly PrototypeInteractionTarget.InteractionType[] DayThreeOrderedInteractions =
    {
        PrototypeInteractionTarget.InteractionType.Shower,
        PrototypeInteractionTarget.InteractionType.Meal,
        PrototypeInteractionTarget.InteractionType.Phone
    };

    [SerializeField] private DayManager dayManager;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private Camera interactionCamera;
    private bool isHeavyInteractionPending;
    private bool isDayNineDoorPending;

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
            if (target != null && !target.TryInteractLocally()) Interact(target.Type);
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
        if (type == PrototypeInteractionTarget.InteractionType.Door && dayManager != null && dayManager.CurrentDay == 9)
        {
            if (!isDayNineDoorPending) StartCoroutine(DelayDayNineDoorInteraction());
            return;
        }

        if (IsHeavyDayMainInteraction(type))
        {
            if (!isHeavyInteractionPending) StartCoroutine(DelayHeavyInteraction(type));
            return;
        }

        PerformInteractionNow(type);
    }

    private IEnumerator DelayHeavyInteraction(PrototypeInteractionTarget.InteractionType type)
    {
        isHeavyInteractionPending = true;
        GameplayNoticeUI.Show("몸이 쉽게 움직이지 않는다...", 0.9f);
        yield return new WaitForSeconds(1f);
        isHeavyInteractionPending = false;
        PerformInteractionNow(type);
    }

    private IEnumerator DelayDayNineDoorInteraction()
    {
        isDayNineDoorPending = true;
        yield return new WaitForSecondsRealtime(0.6f);
        yield return ShakeCamera(0.35f, 0.045f);
        isDayNineDoorPending = false;
        PerformInteractionNow(PrototypeInteractionTarget.InteractionType.Door);
    }

    private IEnumerator ShakeCamera(float duration, float strength)
    {
        if (interactionCamera == null) yield break;

        Transform cameraTransform = interactionCamera.transform;
        Vector3 originalPosition = cameraTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float fade = 1f - Mathf.Clamp01(elapsed / duration);
            cameraTransform.localPosition = originalPosition + (Vector3)Random.insideUnitCircle * strength * fade;
            yield return null;
        }
        cameraTransform.localPosition = originalPosition;
    }

    private bool IsHeavyDayMainInteraction(PrototypeInteractionTarget.InteractionType type)
    {
        return dayManager != null && (dayManager.CurrentDay == 5 || dayManager.CurrentDay == 6) &&
            System.Array.IndexOf(dayManager.GetMainGoalInteractions(), type) >= 0;
    }

    private void PerformInteractionNow(PrototypeInteractionTarget.InteractionType type)
    {
        bool repeatableFinalDoor = type == PrototypeInteractionTarget.InteractionType.Door && dayManager.CurrentDay == 10;
        if (type != PrototypeInteractionTarget.InteractionType.Sleep && !repeatableFinalDoor && dayManager.IsInteractionCompleted(type))
        {
            GameplayNoticeUI.Show("이미 완료한 상호작용입니다.");
            return;
        }

        int dayThreeOrder = GetDayThreeInteractionOrder(type);
        if (dayThreeOrder < 0)
        {
            GameplayNoticeUI.Show("이미 완료한 상호작용입니다.");
            return;
        }

        if (type != PrototypeInteractionTarget.InteractionType.Sleep && !repeatableFinalDoor)
        {
            dayManager.RegisterInteraction(type);
        }

        switch (type)
        {
            case PrototypeInteractionTarget.InteractionType.Sleep:
                if (dayManager.CurrentDay == 10) StartDayDialogue("sleep"); else dayManager.GoToSleep();
                break;
            case PrototypeInteractionTarget.InteractionType.Phone:
                InteractionSoundController.PlayPhoneInteraction();
                StartOneTimeDayDialogue(GetDialogueInteractionId("phone", dayThreeOrder));
                break;
            case PrototypeInteractionTarget.InteractionType.Window:
                FirstPersonPlayerController.UnlockCursor();
                StartDayDialogue("window");
                break;
            case PrototypeInteractionTarget.InteractionType.Meal:
                StartOneTimeDayDialogue(GetDialogueInteractionId("meal", dayThreeOrder));
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
                InteractionSoundController.PlayShowerInteraction();
                StartOneTimeDayDialogue(GetDialogueInteractionId("shower", dayThreeOrder));
                break;
            case PrototypeInteractionTarget.InteractionType.Tidy:
                StartOneTimeDayDialogue("tidy");
                break;
        }
    }

    private int GetDayThreeInteractionOrder(PrototypeInteractionTarget.InteractionType interaction)
    {
        if (dayManager == null || dayManager.CurrentDay != 3 || System.Array.IndexOf(DayThreeOrderedInteractions, interaction) < 0)
        {
            return 0;
        }

        if (dayManager.IsInteractionCompleted(interaction)) return -1;

        int completedCount = 0;
        foreach (PrototypeInteractionTarget.InteractionType candidate in DayThreeOrderedInteractions)
        {
            if (dayManager.IsInteractionCompleted(candidate)) completedCount++;
        }

        return completedCount + 1;
    }

    private static string GetDialogueInteractionId(string interactionId, int dayThreeOrder)
    {
        return dayThreeOrder > 0 ? interactionId + "_" + dayThreeOrder : interactionId;
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
            GameplayNoticeUI.Show("이미 완료한 상호작용입니다.");
            return;
        }

        // Only release the cursor after confirming that a dialogue will open.
        FirstPersonPlayerController.UnlockCursor();
        dialogueManager.StartDialogue(nodeId);
    }
}
