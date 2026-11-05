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
    [SerializeField, Tooltip("First validated imported replacement: the bed only.")]
    private bool useImportedBed = true;
    [SerializeField, Tooltip("Validated imported replacements for the desk corner.")]
    private bool useImportedDeskSet = true;
    [SerializeField] private InteriorKitGamePalette interiorKitPalette;

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
        BuildDayHUD();
        BuildGameplaySystems();
    }

    private void BuildRoom()
    {
        Transform room = new GameObject("PrototypeRoom (Runtime Only)").transform;

        Color wood = new(0.18f, 0.11f, 0.08f);
        Color woodLight = new(0.34f, 0.22f, 0.15f);
        Color fabric = new(0.34f, 0.37f, 0.43f);
        Color paper = new(0.78f, 0.72f, 0.57f);
        Color curtain = new(0.22f, 0.27f, 0.37f);
        Color metal = new(0.14f, 0.16f, 0.19f);

        // A lived-in studio apartment: muted, compact and readable in first person.
        CreateBlock(room, "Floor", new Vector3(0, -0.15f, 2.5f), new Vector3(10, 0.3f, 8), new Color(0.20f, 0.14f, 0.10f));
        CreateBlock(room, "Ceiling", new Vector3(0, 5f, 2.5f), new Vector3(10, 0.15f, 8), new Color(0.13f, 0.14f, 0.18f));
        CreateBlock(room, "Back Wall", new Vector3(0, 2.5f, 6.5f), new Vector3(10, 5, 0.25f), wallColor);
        CreateBlock(room, "Left Wall", new Vector3(-5, 2.5f, 2.5f), new Vector3(0.25f, 5, 8), wallColor);
        CreateBlock(room, "Right Wall", new Vector3(5, 2.5f, 2.5f), new Vector3(0.25f, 5, 8), wallColor);
        BuildEntranceAndBathroom(room, wallColor, wood, metal);
        CreateBlock(room, "Rug", new Vector3(-0.8f, 0.02f, 3.15f), new Vector3(4.2f, 0.035f, 2.8f), new Color(0.16f, 0.20f, 0.27f));

        // Bed and the small traces of an interrupted daily routine.
        CreateBlock(room, "Bed Frame", new Vector3(-2.7f, 0.35f, 4.6f), new Vector3(3.1f, 0.45f, 1.9f), wood);
        CreateBlock(room, "Bed Headboard", new Vector3(-2.7f, 1.2f, 5.4f), new Vector3(3.1f, 1.2f, 0.16f), woodLight);
        GameObject mattress = CreateBlock(room, "Mattress - Click to Sleep", new Vector3(-2.7f, 0.68f, 4.6f), new Vector3(2.9f, 0.25f, 1.7f), new Color(0.64f, 0.62f, 0.57f));
        mattress.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Sleep);
        CreateBlock(room, "Pillow", new Vector3(-3.55f, 0.9f, 5.04f), new Vector3(0.9f, 0.18f, 0.58f), new Color(0.75f, 0.75f, 0.72f));
        CreateBlock(room, "Rumpled Blanket", new Vector3(-2.35f, 0.89f, 4.35f), new Vector3(2.1f, 0.16f, 1.2f), fabric);
        CreateBlock(room, "Laundry Basket", new Vector3(-4.15f, 0.38f, 2.15f), new Vector3(0.75f, 0.68f, 0.65f), new Color(0.52f, 0.49f, 0.43f));
        CreateBlock(room, "Folded Clothes", new Vector3(-4.15f, 0.78f, 2.15f), new Vector3(0.86f, 0.18f, 0.72f), new Color(0.18f, 0.20f, 0.27f));

        // Work desk, chair and a warm light contrasting the blue window light.
        CreateBlock(room, "Desk", new Vector3(2.5f, 1.05f, 5.5f), new Vector3(2.5f, 0.18f, 1.1f), woodLight);
        CreateBlock(room, "Desk Leg Left", new Vector3(1.55f, 0.5f, 5.5f), new Vector3(0.14f, 1, 0.14f), wood);
        CreateBlock(room, "Desk Leg Right", new Vector3(3.45f, 0.5f, 5.5f), new Vector3(0.14f, 1, 0.14f), wood);
        CreateBlock(room, "Desk Drawers", new Vector3(3.38f, 0.55f, 5.5f), new Vector3(0.42f, 0.85f, 0.92f), wood);
        CreateBlock(room, "Chair Seat", new Vector3(2.45f, 0.62f, 3.95f), new Vector3(1.05f, 0.18f, 1.0f), metal);
        CreateBlock(room, "Chair Back", new Vector3(2.45f, 1.25f, 4.36f), new Vector3(1.05f, 1.1f, 0.14f), new Color(0.20f, 0.22f, 0.27f));
        CreateBlock(room, "Chair Leg", new Vector3(2.45f, 0.3f, 3.95f), new Vector3(0.12f, 0.65f, 0.12f), metal);
        // Keep the phone beside the pillow: it is the first object the player
        // can naturally notice after waking, rather than another desk prop.
        GameObject phone = CreateBlock(room, "Phone - Click to Read", new Vector3(-2.88f, 1.28f, 4.16f), new Vector3(0.38f, 0.07f, 0.68f), new Color(0.025f, 0.035f, 0.055f));
        phone.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Phone);
        // A lit screen keeps the small prop readable against the dark bedding.
        CreateBlock(phone.transform, "Phone Screen", new Vector3(-2.88f, 1.325f, 4.16f), new Vector3(0.29f, 0.012f, 0.52f), new Color(0.30f, 0.70f, 0.92f));
        GameObject meal = CreateBlock(room, "Meal - Interact", new Vector3(1.75f, 1.22f, 5.5f), new Vector3(0.42f, 0.12f, 0.42f), new Color(0.85f, 0.42f, 0.15f));
        meal.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Meal);
        GameObject medicine = CreateBlock(room, "Medicine - Interact", new Vector3(3.25f, 1.26f, 5.5f), new Vector3(0.22f, 0.22f, 0.22f), new Color(0.94f, 0.94f, 0.96f));
        medicine.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Medicine);
        CreateBlock(room, "Desk Lamp Stand", new Vector3(3.85f, 1.48f, 5.45f), new Vector3(0.10f, 0.78f, 0.10f), metal);
        CreateBlock(room, "Desk Lamp Shade", new Vector3(3.85f, 1.88f, 5.45f), new Vector3(0.46f, 0.25f, 0.46f), new Color(0.10f, 0.10f, 0.12f));
        Light lamp = new GameObject("Desk Lamp Light").AddComponent<Light>();
        lamp.type = LightType.Point; lamp.color = new Color(1f, 0.66f, 0.35f); lamp.intensity = 1.3f; lamp.range = 3.2f;
        lamp.transform.position = new Vector3(3.5f, 1.85f, 4.95f);

        // Window, curtains and the entrance are deliberately prominent story anchors.
        GameObject window = CreateBlock(room, "Window - Click to Look Outside", new Vector3(0.5f, 2.9f, 6.32f), new Vector3(2.4f, 1.5f, 0.06f), new Color(0.20f, 0.55f, 0.85f));
        window.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Window);
        CreateBlock(room, "Curtain Left", new Vector3(-0.94f, 2.9f, 6.2f), new Vector3(0.50f, 1.9f, 0.10f), curtain);
        CreateBlock(room, "Curtain Right", new Vector3(1.94f, 2.9f, 6.2f), new Vector3(0.50f, 1.9f, 0.10f), curtain);
        CreateBlock(room, "Curtain Rail", new Vector3(0.5f, 4.1f, 6.17f), new Vector3(3.7f, 0.10f, 0.10f), metal);
        GameObject mirror = CreateBlock(room, "Mirror - Click to Look", new Vector3(-3.25f, 2.35f, 1.32f), new Vector3(1.25f, 1.25f, 0.06f), new Color(0.52f, 0.70f, 0.76f));
        mirror.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Mirror);
        GameObject shower = CreateBlock(room, "Sink - Click to Wash", new Vector3(-3.25f, 0.68f, 1.02f), new Vector3(1.1f, 1.05f, 0.55f), new Color(0.72f, 0.76f, 0.80f));
        shower.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Shower);
        GameObject tidy = CreateBlock(room, "Cup - Click to Tidy", new Vector3(1.15f, 1.25f, 5.5f), new Vector3(0.18f, 0.25f, 0.18f), new Color(0.75f, 0.75f, 0.70f));
        tidy.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Tidy);
        CreateBlock(room, "Wall Notes", new Vector3(-1.7f, 2.8f, 6.28f), new Vector3(0.75f, 0.85f, 0.04f), paper);
        CreateBlock(room, "Floor Clothes", new Vector3(-1.2f, 0.13f, 2.05f), new Vector3(0.75f, 0.12f, 0.55f), new Color(0.13f, 0.15f, 0.20f));

        Light roomLight = new GameObject("Prototype Room Light").AddComponent<Light>();
        roomLight.type = LightType.Point;
        roomLight.color = new Color(1f, 0.78f, 0.56f);
        roomLight.intensity = 9f;
        roomLight.range = 12f;
        roomLight.transform.position = new Vector3(0, 4.2f, 3.2f);

        // Replace one object at a time while validating imported asset scale.
        if (interiorKitPalette == null)
        {
            if (useImportedBed) BuildImportedBed(room);
            if (useImportedDeskSet) BuildImportedDeskSet(room);
        }

        BuildDExpressiveFurniture(room);
        if (interiorKitPalette != null) interiorKitPalette.Apply(room);
    }

    private static void BuildEntranceAndBathroom(Transform room, Color wall, Color wood, Color metal)
    {
        // Entrance behind the player: two wall sections frame a real front door.
        CreateBlock(room, "Entrance Wall Left", new Vector3(-2.45f, 2.5f, -1.45f), new Vector3(5.1f, 5f, 0.22f), wall);
        CreateBlock(room, "Entrance Wall Right", new Vector3(3.45f, 2.5f, -1.45f), new Vector3(3.1f, 5f, 0.22f), wall);
        Transform doorHinge = new GameObject("Entrance Door Hinge").transform;
        doorHinge.SetParent(room);
        doorHinge.position = new Vector3(0.18f, 0f, -1.32f);
        GameObject frontDoor = CreateBlock(doorHinge, "Door - Click to Interact", new Vector3(1.0f, 1.7f, -1.32f), new Vector3(1.65f, 3.3f, 0.14f), wood);
        frontDoor.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Door);
        CreateBlock(room, "Door Handle", new Vector3(1.58f, 1.65f, -1.18f), new Vector3(0.12f, 0.12f, 0.08f), metal);
        CreateBlock(room, "Shoe Cabinet", new Vector3(3.65f, 0.72f, -1.05f), new Vector3(1.65f, 1.38f, 0.45f), new Color(0.23f, 0.20f, 0.18f));
        CreateBlock(room, "Shoes", new Vector3(3.55f, 0.16f, -0.74f), new Vector3(0.65f, 0.16f, 0.32f), new Color(0.08f, 0.09f, 0.11f));
        CreateBlock(room, "Door Mat", new Vector3(1.0f, 0.02f, -0.85f), new Vector3(2.0f, 0.04f, 0.72f), new Color(0.12f, 0.14f, 0.17f));

        // Compact bathroom at the front-left, connected by an opening in its right partition.
        Color bathroomWall = new(0.42f, 0.45f, 0.49f);
        CreateBlock(room, "Bathroom Tile Floor", new Vector3(-3.3f, 0.025f, 0.1f), new Vector3(3.25f, 0.05f, 2.75f), new Color(0.38f, 0.43f, 0.47f));
        CreateBlock(room, "Bathroom Back Wall", new Vector3(-3.3f, 2.4f, 1.45f), new Vector3(3.3f, 4.8f, 0.16f), bathroomWall);
        CreateBlock(room, "Bathroom Divider Lower", new Vector3(-1.65f, 1.15f, -0.7f), new Vector3(0.16f, 2.3f, 1.15f), bathroomWall);
        CreateBlock(room, "Bathroom Divider Upper", new Vector3(-1.65f, 3.65f, 0.2f), new Vector3(0.16f, 2.7f, 2.5f), bathroomWall);
        CreateBlock(room, "Bathroom Door Frame", new Vector3(-1.57f, 1.2f, 0.48f), new Vector3(0.08f, 2.4f, 0.08f), wood);
        CreateBlock(room, "Bathroom Shelf", new Vector3(-4.35f, 1.5f, 1.15f), new Vector3(0.45f, 0.12f, 0.55f), wood);
    }

    private static void BuildImportedBed(Transform room)
    {
        // Reference layout: bed follows the left wall toward the window.
        ReplaceWithFurniture(room, "Bed", new Vector3(-3.78f, 0.12f, 4.72f), new Vector3(0f, 90f, 0f),
            new Color(0.42f, 0.45f, 0.56f), PrototypeInteractionTarget.InteractionType.Sleep,
            null, "Bed Frame", "Bed Headboard", "Mattress - Click to Sleep", "Pillow", "Rumpled Blanket");
    }

    private static void BuildImportedDeskSet(Transform room)
    {
        // The desk mirrors the bed: long edge along the right wall.
        ReplaceWithFurniture(room, "Table", new Vector3(3.92f, 0.06f, 4.68f), new Vector3(0f, 90f, 0f),
            new Color(0.31f, 0.22f, 0.17f), null, new Vector3(1.7f, 1f, 1f),
            "Desk", "Desk Leg Left", "Desk Leg Right", "Desk Drawers");
        ReplaceWithFurniture(room, "chair_01", new Vector3(2.95f, 0.06f, 4.15f), new Vector3(0f, 90f, 0f),
            new Color(0.20f, 0.23f, 0.29f), null, null, "Chair Seat", "Chair Back", "Chair Leg");
        ReplaceWithFurniture(room, "Lamp_01", new Vector3(3.65f, 1.15f, 5.58f), Vector3.zero,
            new Color(0.18f, 0.15f, 0.14f), null, null, "Desk Lamp Stand", "Desk Lamp Shade");
    }

    private void BuildDExpressiveFurniture(Transform room)
    {
        // The imported bed/desk set has been visually validated. The other
        // pack prefabs have inconsistent pivots, so use stable purpose-built
        // props here instead of letting them overlap walls or the camera.
        Color darkWood = new(0.20f, 0.16f, 0.14f);
        CreateBlock(room, "Wardrobe Body", new Vector3(4.36f, 1.35f, 2.25f), new Vector3(0.72f, 2.7f, 1.30f), darkWood);
        CreateBlock(room, "Wardrobe Door Left", new Vector3(3.98f, 1.37f, 1.93f), new Vector3(0.05f, 2.4f, 0.55f), new Color(0.28f, 0.23f, 0.21f));
        CreateBlock(room, "Wardrobe Door Right", new Vector3(3.98f, 1.37f, 2.57f), new Vector3(0.05f, 2.4f, 0.55f), new Color(0.28f, 0.23f, 0.21f));
        CreateBlock(room, "Wardrobe Handle", new Vector3(3.92f, 1.35f, 2.25f), new Vector3(0.06f, 0.10f, 0.10f), new Color(0.60f, 0.55f, 0.48f));

        CreateBlock(room, "Computer Monitor", new Vector3(3.20f, 1.65f, 5.20f), new Vector3(0.10f, 0.68f, 0.92f), new Color(0.04f, 0.06f, 0.09f));
        CreateBlock(room, "Computer Screen", new Vector3(3.13f, 1.65f, 5.20f), new Vector3(0.02f, 0.52f, 0.70f), new Color(0.12f, 0.20f, 0.31f));
        CreateBlock(room, "Keyboard", new Vector3(3.32f, 1.25f, 4.70f), new Vector3(0.52f, 0.06f, 0.26f), new Color(0.10f, 0.12f, 0.15f));
        CreateBlock(room, "Plant Pot", new Vector3(3.42f, 1.30f, 5.75f), new Vector3(0.22f, 0.35f, 0.22f), new Color(0.25f, 0.34f, 0.25f));
    }

    private static void ReplaceWithFurniture(Transform parent, string resourceName, Vector3 position, Vector3 rotation,
        Color tint, PrototypeInteractionTarget.InteractionType? interaction = null, Vector3? scale = null, params string[] replacedNames)
    {
        GameObject prefab = Resources.Load<GameObject>("DExpressiveFurniture/" + resourceName);
        if (prefab == null)
        {
            Debug.LogError("가구 프리팹을 불러오지 못했습니다: " + resourceName);
            return;
        }

        foreach (string replacedName in replacedNames)
        {
            GameObject placeholder = GameObject.Find(replacedName);
            if (placeholder != null) placeholder.SetActive(false);
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.Euler(rotation), parent);
        instance.name = "D-Expressive " + resourceName;
        if (scale.HasValue) instance.transform.localScale = scale.Value;
        ApplyMoodMaterial(instance, tint);
        if (interaction.HasValue)
        {
            // Unity components can behave like a "fake null" after a prefab
            // import. Use Unity's null check rather than the C# ?? operator.
            BoxCollider collider = instance.GetComponent<BoxCollider>();
            if (collider == null) collider = instance.AddComponent<BoxCollider>();
            Bounds bounds = CalculateBounds(instance);
            collider.center = instance.transform.InverseTransformPoint(bounds.center);
            collider.size = instance.transform.InverseTransformVector(bounds.size);
            instance.AddComponent<PrototypeInteractionTarget>().SetType(interaction.Value);
        }
    }

    private static void ApplyMoodMaterial(GameObject instance, Color tint)
    {
        Shader roomShader = Shader.Find("Universal Render Pipeline/Lit");
        if (roomShader == null) roomShader = Shader.Find("Standard");

        foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>())
        {
            Material material = renderer.material;
            // The imported sample uses its own shader. In this URP project it
            // renders pink, so preserve the base texture and use URP Lit.
            Texture baseTexture = material.mainTexture;
            Color baseColor = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor") : material.color;
            if (roomShader != null) material.shader = roomShader;
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", baseTexture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.Lerp(baseColor, tint, 0.58f));
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.Lerp(baseColor, tint, 0.58f));
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.22f);
        }
    }

    private static Bounds CalculateBounds(GameObject instance)
    {
        Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
        Bounds bounds = new Bounds(instance.transform.position, Vector3.one);
        if (renderers.Length == 0) return bounds;
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static GameObject CreateBlock(Transform parent, string blockName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = blockName;
        block.transform.SetParent(parent);
        block.transform.position = position;
        block.transform.localScale = scale;
        block.GetComponent<Renderer>().sharedMaterial = RuntimeRoomMaterial.Create(color);
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

    private void BuildDayHUD()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null || dayManager == null) return;

        GameObject hudObject = new GameObject("Day HUD", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(DayHUD));
        hudObject.transform.SetParent(canvas.transform, false);
        RectTransform rect = hudObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(28f, -76f);
        rect.sizeDelta = new Vector2(280f, 48f);

        TextMeshProUGUI label = hudObject.GetComponent<TextMeshProUGUI>();
        label.alignment = TextAlignmentOptions.Left;
        label.fontSize = 30;
        label.fontStyle = FontStyles.Bold;
        label.color = new Color(0.72f, 0.86f, 1f, 1f);

        hudObject.GetComponent<DayHUD>().Initialize(dayManager, label);
    }

    private void BuildGameplaySystems()
    {
        GameObject systems = new GameObject("Prototype Gameplay Systems");
        systems.AddComponent<RoomActionVisuals>();
        systems.AddComponent<DayPresentationController>();
        systems.AddComponent<SleepTransitionController>();
        systems.AddComponent<WakeUpTransitionController>();
        systems.AddComponent<DoorEndingController>();
        systems.AddComponent<DailyRoomEvolution>();
        PrototypeGameUI gameUI = systems.AddComponent<PrototypeGameUI>();
        gameUI.Initialize(dayManager);
    }

}
