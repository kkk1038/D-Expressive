using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>First-person movement and F-key interaction for the room.</summary>
[RequireComponent(typeof(CharacterController))]
public class FirstPersonPlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float lookSensitivity = 0.12f;
    [SerializeField] private float interactionDistance = 2.4f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private TMP_Text interactionHint;

    private CharacterController characterController;
    private float pitch;
    private float verticalVelocity;
    private PrototypeInteractionTarget focusedTarget;
    private PrototypeInteractionTarget previousFocusedTarget;
    private ChoiceUI choiceUI;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
        choiceUI = FindFirstObjectByType<ChoiceUI>();
        UnlockCursor();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // A dialogue owns all input until the player makes a choice.
        if (choiceUI != null && choiceUI.IsShowing)
        {
            if (interactionHint != null) interactionHint.text = "선택지를 클릭하거나 1 / 2 키를 누르세요";
            return;
        }

        Move();
        LookForInteraction();
    }

    private void Move()
    {
        Vector2 input = Keyboard.current == null ? Vector2.zero : new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0));

        Vector3 movement = (transform.right * input.x + transform.forward * input.y).normalized
            * moveSpeed * GetMentalStateSpeedMultiplier();
        if (characterController.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    private static float GetMentalStateSpeedMultiplier()
    {
        if (GameStateManager.Instance == null) return 1f;

        GameStateData state = GameStateManager.Instance.CurrentState;
        int mentalState = state.dayStartMentalState > 0 ? state.dayStartMentalState : state.mentalState;
        if (state.day == 6) mentalState = 10;
        if (mentalState <= 10) return 0.30f;
        if (mentalState <= 50) return 0.70f;
        return 1f;
    }

    private void LookForInteraction()
    {
        if (playerCamera == null) return;

        // Normal play uses direct mouse look. The cursor is released only
        // when dialogue or a menu needs a mouse click.
        if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * lookSensitivity;
            pitch = Mathf.Clamp(pitch - mouseDelta.y, -80f, 80f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            transform.Rotate(Vector3.up * mouseDelta.x);
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        focusedTarget = FindInteractionTarget(ray);

        if (previousFocusedTarget != focusedTarget)
        {
            if (previousFocusedTarget != null) previousFocusedTarget.SetFocused(false);
            if (focusedTarget != null) focusedTarget.SetFocused(true);
            previousFocusedTarget = focusedTarget;
        }

        if (interactionHint != null)
        {
            interactionHint.text = focusedTarget == null
                ? "WASD: 이동    마우스: 시점 이동"
                : "[ F ]  " + GetPrompt(focusedTarget.Type);
        }

        if (focusedTarget != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            PrototypeInteractionController.Interact(focusedTarget.Type);
        }
    }

    private PrototypeInteractionTarget FindInteractionTarget(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance);
        if (hits.Length == 0) return null;

        System.Array.Sort(hits, (left, right) => left.distance.CompareTo(right.distance));
        PrototypeInteractionTarget sleepFallback = null;
        foreach (RaycastHit hit in hits)
        {
            PrototypeInteractionTarget target = hit.collider.GetComponentInParent<PrototypeInteractionTarget>();
            if (target == null) continue;

            // Imported bed colliders are broad enough to overlap props lying
            // on the mattress. A visible prop such as the phone must win over
            // the bed's sleep target when both are under the crosshair.
            if (target.Type != PrototypeInteractionTarget.InteractionType.Sleep)
            {
                return target;
            }

            sleepFallback ??= target;
        }

        return sleepFallback;
    }

    private static string GetPrompt(PrototypeInteractionTarget.InteractionType type)
    {
        return type switch
        {
            PrototypeInteractionTarget.InteractionType.Sleep => "잠자기",
            PrototypeInteractionTarget.InteractionType.Phone => "휴대폰 확인하기",
            PrototypeInteractionTarget.InteractionType.Window => "창밖 보기",
            PrototypeInteractionTarget.InteractionType.Meal => "식사하기",
            PrototypeInteractionTarget.InteractionType.Medicine => "약 복용하기",
            PrototypeInteractionTarget.InteractionType.Door => "현관문 확인하기",
            PrototypeInteractionTarget.InteractionType.Mirror => "거울 보기",
            PrototypeInteractionTarget.InteractionType.Shower => "세수하기",
            PrototypeInteractionTarget.InteractionType.Tidy => "물건 정리하기",
            _ => "상호작용"
        };
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>Resets the local camera pitch when a new day begins in bed.</summary>
    public void ResetViewPitch()
    {
        pitch = 0f;
        if (playerCamera != null) playerCamera.transform.localRotation = Quaternion.identity;
    }

    private void OnDisable()
    {
        if (previousFocusedTarget != null) previousFocusedTarget.SetFocused(false);
    }
}
