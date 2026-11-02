using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Creates a runtime-only first-person capsule in the prototype room.</summary>
public class FirstPersonPlayerBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (!Application.isPlaying) return;

        GameObject player = new GameObject("First Person Player (Runtime Only)");
        player.transform.position = new Vector3(0f, 0.05f, -0.8f);
        CharacterController controller = player.AddComponent<CharacterController>();
        controller.height = 1.8f;
        controller.radius = 0.32f;
        controller.center = new Vector3(0f, 0.9f, 0f);

        Camera camera = Camera.main;
        if (camera == null) return;
        camera.transform.SetParent(player.transform);
        camera.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        camera.transform.localRotation = Quaternion.identity;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.08f, 0.12f);

        FirstPersonPlayerController playerController = player.AddComponent<FirstPersonPlayerController>();
        SetPrivateField(playerController, "playerCamera", camera);
        SetPrivateField(playerController, "interactionHint", CreateHint());
        CreateCrosshair();
    }

    private static TMP_Text CreateHint()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return null;

        GameObject hintObject = new GameObject("First Person Hint", typeof(RectTransform), typeof(TextMeshProUGUI));
        hintObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = hintObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 28f);
        rect.sizeDelta = new Vector2(600f, 38f);
        TextMeshProUGUI hint = hintObject.GetComponent<TextMeshProUGUI>();
        hint.alignment = TextAlignmentOptions.Center;
        hint.fontSize = 20;
        hint.color = Color.white;
        return hint;
    }

    private static void CreateCrosshair()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject crosshair = new GameObject("Interaction Crosshair", typeof(RectTransform), typeof(Image));
        crosshair.transform.SetParent(canvas.transform, false);
        RectTransform rect = crosshair.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(7f, 7f);
        Image image = crosshair.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.72f);
        image.raycastTarget = false;
    }

    // Keeps setup in one file without exposing prototype-only configuration in the Inspector.
    private static void SetPrivateField<T>(FirstPersonPlayerController target, string fieldName, T value)
    {
        typeof(FirstPersonPlayerController).GetField(fieldName,
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.SetValue(target, value);
    }
}
