using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A zero-asset prototype room for validating the day loop. It is generated only
/// in Play Mode, so it never replaces the final art scene.
/// </summary>
public class PrototypeRoomBootstrap : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;

    private readonly Color wallColor = new(0.35f, 0.38f, 0.46f);
    private readonly Color floorColor = new(0.18f, 0.16f, 0.15f);

    private void Awake()
    {
        if (!Application.isPlaying) return;

        if (dayManager == null)
        {
            dayManager = FindFirstObjectByType<DayManager>();
        }

        BuildRoom();
        BuildSleepButton();
        PositionCamera();
    }

    private void BuildRoom()
    {
        Transform room = new GameObject("PrototypeRoom (Runtime Only)").transform;

        CreateBlock(room, "Floor", new Vector3(0, -0.15f, 2.5f), new Vector3(10, 0.3f, 8), floorColor);
        CreateBlock(room, "Back Wall", new Vector3(0, 2.5f, 6.5f), new Vector3(10, 5, 0.25f), wallColor);
        CreateBlock(room, "Left Wall", new Vector3(-5, 2.5f, 2.5f), new Vector3(0.25f, 5, 8), wallColor);
        CreateBlock(room, "Right Wall", new Vector3(5, 2.5f, 2.5f), new Vector3(0.25f, 5, 8), wallColor);

        CreateBlock(room, "Bed Frame", new Vector3(-2.7f, 0.35f, 4.6f), new Vector3(3.1f, 0.45f, 1.9f), new Color(0.20f, 0.12f, 0.10f));
        GameObject mattress = CreateBlock(room, "Mattress - Click to Sleep", new Vector3(-2.7f, 0.68f, 4.6f), new Vector3(2.9f, 0.25f, 1.7f), new Color(0.80f, 0.78f, 0.68f));
        mattress.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Sleep);
        CreateBlock(room, "Pillow", new Vector3(-3.6f, 0.88f, 5.05f), new Vector3(0.8f, 0.18f, 0.55f), Color.white);
        CreateBlock(room, "Desk", new Vector3(2.5f, 1.05f, 5.5f), new Vector3(2.5f, 0.18f, 1.1f), new Color(0.24f, 0.13f, 0.08f));
        CreateBlock(room, "Desk Leg Left", new Vector3(1.55f, 0.5f, 5.5f), new Vector3(0.14f, 1, 0.14f), new Color(0.18f, 0.1f, 0.06f));
        CreateBlock(room, "Desk Leg Right", new Vector3(3.45f, 0.5f, 5.5f), new Vector3(0.14f, 1, 0.14f), new Color(0.18f, 0.1f, 0.06f));
        GameObject phone = CreateBlock(room, "Phone - Click to Read", new Vector3(2.5f, 1.2f, 5.5f), new Vector3(0.45f, 0.08f, 0.78f), new Color(0.04f, 0.06f, 0.08f));
        phone.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Phone);
        GameObject meal = CreateBlock(room, "Meal - Interact", new Vector3(1.75f, 1.22f, 5.5f), new Vector3(0.42f, 0.12f, 0.42f), new Color(0.85f, 0.42f, 0.15f));
        meal.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Meal);
        GameObject medicine = CreateBlock(room, "Medicine - Interact", new Vector3(3.25f, 1.26f, 5.5f), new Vector3(0.22f, 0.22f, 0.22f), new Color(0.94f, 0.94f, 0.96f));
        medicine.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Medicine);
        GameObject door = CreateBlock(room, "Door - Click to Interact", new Vector3(4.82f, 1.7f, 1.4f), new Vector3(0.08f, 3.3f, 1.55f), new Color(0.26f, 0.16f, 0.10f));
        door.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Door);
        GameObject window = CreateBlock(room, "Window - Click to Look Outside", new Vector3(0.5f, 2.9f, 6.32f), new Vector3(2.4f, 1.5f, 0.06f), new Color(0.20f, 0.55f, 0.85f));
        window.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Window);
        GameObject mirror = CreateBlock(room, "Mirror - Click to Look", new Vector3(-4.82f, 2.5f, 1.4f), new Vector3(0.06f, 1.6f, 1.1f), new Color(0.52f, 0.70f, 0.76f));
        mirror.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Mirror);
        GameObject shower = CreateBlock(room, "Sink - Click to Wash", new Vector3(-3.9f, 0.8f, 1.1f), new Vector3(0.9f, 1.2f, 0.6f), new Color(0.82f, 0.84f, 0.88f));
        shower.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Shower);
        GameObject tidy = CreateBlock(room, "Cup - Click to Tidy", new Vector3(1.15f, 1.25f, 5.5f), new Vector3(0.18f, 0.25f, 0.18f), new Color(0.75f, 0.75f, 0.70f));
        tidy.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Tidy);

        Light roomLight = new GameObject("Prototype Room Light").AddComponent<Light>();
        roomLight.type = LightType.Point;
        roomLight.color = new Color(1f, 0.78f, 0.56f);
        roomLight.intensity = 9f;
        roomLight.range = 12f;
        roomLight.transform.position = new Vector3(0, 4.2f, 3.2f);
    }

    private static GameObject CreateBlock(Transform parent, string blockName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = blockName;
        block.transform.SetParent(parent);
        block.transform.position = position;
        block.transform.localScale = scale;
        block.GetComponent<Renderer>().material.color = color;
        return block;
    }

    private void BuildSleepButton()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null || dayManager == null) return;

        GameObject buttonObject = new GameObject("Sleep Button", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.anchoredPosition = new Vector2(-32, 32);
        rect.sizeDelta = new Vector2(190, 58);
        buttonObject.GetComponent<Image>().color = new Color(0.16f, 0.26f, 0.40f, 0.95f);
        buttonObject.GetComponent<Button>().onClick.AddListener(dayManager.GoToSleep);

        GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        label.text = "잠자기  →  다음 날";
        label.alignment = TextAlignmentOptions.Center;
        label.fontSize = 20;
        label.color = Color.white;
    }

    private static void PositionCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) return;

        camera.transform.position = new Vector3(0, 3.5f, -5.8f);
        camera.transform.rotation = Quaternion.Euler(13f, 0f, 0f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.07f, 0.08f, 0.12f);
    }
}
