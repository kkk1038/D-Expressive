using System.Collections;
using UnityEngine;

/// <summary>Gives each new day a short first-person "waking up" camera motion.</summary>
public class WakeUpTransitionController : MonoBehaviour
{
    private DayManager dayManager;
    private Coroutine wakeUpRoutine;

    private void Start()
    {
        dayManager = FindFirstObjectByType<DayManager>();
        if (dayManager != null) dayManager.OnDayStarted += PlayWakeUp;
    }

    private void OnDestroy()
    {
        if (dayManager != null) dayManager.OnDayStarted -= PlayWakeUp;
    }

    private void PlayWakeUp(int day)
    {
        if (wakeUpRoutine != null) StopCoroutine(wakeUpRoutine);
        wakeUpRoutine = StartCoroutine(WakeUp());
    }

    private IEnumerator WakeUp()
    {
        // The first frame may occur before the runtime player has completed
        // setup, especially on a fresh scene load.
        yield return null;

        Camera camera = Camera.main;
        if (camera == null || camera.transform.parent == null) yield break;

        FirstPersonPlayerController player = camera.GetComponentInParent<FirstPersonPlayerController>();
        if (player != null)
        {
            player.transform.position = FirstPersonPlayerBootstrap.WakeUpPosition;
            player.transform.rotation = Quaternion.Euler(0f, FirstPersonPlayerBootstrap.WakeUpYaw, 0f);
            player.ResetViewPitch();
        }

        Transform view = camera.transform;
        Vector3 uprightPosition = new Vector3(0f, 1.6f, 0f);
        Quaternion uprightRotation = Quaternion.identity;
        view.localPosition = new Vector3(0f, 0.78f, -0.28f);
        view.localRotation = Quaternion.Euler(21f, 0f, 0f);

        const float duration = 0.9f;
        Vector3 fromPosition = view.localPosition;
        Quaternion fromRotation = view.localRotation;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
        {
            float eased = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            view.localPosition = Vector3.Lerp(fromPosition, uprightPosition, eased);
            view.localRotation = Quaternion.Slerp(fromRotation, uprightRotation, eased);
            yield return null;
        }

        view.localPosition = uprightPosition;
        view.localRotation = uprightRotation;
        wakeUpRoutine = null;
    }
}
