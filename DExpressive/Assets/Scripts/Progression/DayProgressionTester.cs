using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>Optional manual smoke test. Keep disabled in release scenes.</summary>
public class DayProgressionTester : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
        {
            dayManager.GoToSleep();
        }
    }
}
