using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Only attached in InteriorKitTest; never changes story state or saves.</summary>
public class InteriorKitTestControls : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            FirstPersonPlayerController.UnlockCursor();
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            FirstPersonPlayerController.LockCursor();
        if (Keyboard.current == null || !Keyboard.current.fKey.wasPressedThisFrame) return;
        var camera = Camera.main;
        if (camera && Physics.Raycast(camera.transform.position, camera.transform.forward, out var hit, 2.4f))
            hit.collider.GetComponentInParent<InteriorKitTestDoor>()?.Toggle();
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(15, 15, 570, 58),
            "INTERIOR KIT TEST | WASD: Move | Click: Look | Esc: Cursor\nF: Open / close entrance | Isolated scene: no story or save writes");
        GUI.Label(new Rect(Screen.width / 2f - 5, Screen.height / 2f - 10, 20, 20), "+");
    }

    private void OnDisable() => FirstPersonPlayerController.UnlockCursor();
}
