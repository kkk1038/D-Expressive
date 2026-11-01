using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Minimal first-person movement and E-key interaction for the prototype room.</summary>
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

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main;
        UnlockCursor();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        Move();
        LookForInteraction();
    }

    private void Move()
    {
        Vector2 input = Keyboard.current == null ? Vector2.zero : new Vector2(
            (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
            (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0));

        Vector3 movement = (transform.right * input.x + transform.forward * input.y).normalized * moveSpeed;
        if (characterController.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        verticalVelocity += Physics.gravity.y * Time.deltaTime;
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    private void LookForInteraction()
    {
        if (playerCamera == null) return;

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame)
        {
            UnlockCursor();
        }

        if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
        {
            Vector2 mouseDelta = Mouse.current.delta.ReadValue() * lookSensitivity;
            pitch = Mathf.Clamp(pitch - mouseDelta.y, -80f, 80f);
            playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
            transform.Rotate(Vector3.up * mouseDelta.x);
        }

        focusedTarget = null;
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            focusedTarget = hit.collider.GetComponent<PrototypeInteractionTarget>();
        }

        if (interactionHint != null)
        {
            interactionHint.text = focusedTarget == null
                ? "WASD: 이동    마우스 오른쪽 버튼: 시점 이동"
                : "F : " + GetPrompt(focusedTarget.Type);
        }

        if (focusedTarget != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            PrototypeInteractionController.Interact(focusedTarget.Type);
        }
    }

    private static string GetPrompt(PrototypeInteractionTarget.InteractionType type)
    {
        return type switch
        {
            PrototypeInteractionTarget.InteractionType.Sleep => "잠자기",
            PrototypeInteractionTarget.InteractionType.Phone => "휴대폰 확인하기",
            PrototypeInteractionTarget.InteractionType.Window => "창밖 보기",
            PrototypeInteractionTarget.InteractionType.Meal => "식사하기",
            PrototypeInteractionTarget.InteractionType.Medicine => "약 먹기",
            PrototypeInteractionTarget.InteractionType.Door => "현관문 보기",
            PrototypeInteractionTarget.InteractionType.Mirror => "거울 보기",
            PrototypeInteractionTarget.InteractionType.Shower => "세수하기",
            PrototypeInteractionTarget.InteractionType.Tidy => "하나 정리하기",
            _ => "상호작용"
        };
    }

    public static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
