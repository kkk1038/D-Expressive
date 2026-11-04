using System;
using System.Linq;
using UnityEngine;

/// <summary>Replaces an explicit list of room parts, keeping all unmatched story props intact.</summary>
[CreateAssetMenu(menuName = "DExpressive/Interior Kit Game Palette")]
public class InteriorKitGamePalette : ScriptableObject
{
    public GameObject wall, floor, ceiling, bed, desk, chair, rug, window, door;
    public AdditionalRoomPropsPalette additionalProps;

    public void Apply(Transform room)
    {
        // Validate the entire palette before hiding anything, so a missing asset keeps the prototype usable.
        if (new[] { wall, floor, ceiling, bed, desk, chair, rug, window, door }.Any(p => p == null))
        {
            Debug.LogError("Interior Kit palette is incomplete; prototype geometry retained.");
            return;
        }
        var architecture = new GameObject("Interior Kit Architecture").transform;
        architecture.SetParent(room, false);
        var furniture = new GameObject("Interior Kit Furniture").transform;
        furniture.SetParent(room, false);
        for (int x = -4; x <= 4; x += 2)
        for (float z = -0.5f; z <= 5.5f; z += 2)
        {
            Spawn(floor, architecture, "Floor Tile", new Vector3(x, -0.3f, z), 0, new Vector3(1, 6, 1));
            Spawn(ceiling, architecture, "Ceiling Tile", new Vector3(x, 4.925f, z), 0, new Vector3(1, 3, 1));
        }
        Hide(room, "Floor", "Ceiling");
        ReplaceWall(room, architecture, "Left Wall", 90);
        ReplaceWall(room, architecture, "Right Wall", 90);
        ReplaceWall(room, architecture, "Entrance Wall Left", 0);
        ReplaceWall(room, architecture, "Entrance Wall Right", 0);
        ReplaceWall(room, architecture, "Bathroom Back Wall", 0);
        ReplaceWall(room, architecture, "Bathroom Divider Lower", 90);
        ReplaceWall(room, architecture, "Bathroom Divider Upper", 90);
        // Match the original window's exact opening, rather than moving curtains, notes or story anchors.
        WallSpan(architecture, "Back Wall Left", -5, -0.7f, 0, 5, 6.5f);
        WallSpan(architecture, "Back Wall Right", 1.7f, 5, 0, 5, 6.5f);
        WallSpan(architecture, "Back Wall Below Window", -0.7f, 1.7f, 0, 2.15f, 6.5f);
        WallSpan(architecture, "Back Wall Above Window", -0.7f, 1.7f, 3.65f, 5, 6.5f);
        WallSpan(architecture, "Entrance Lintel", 0.1f, 1.9f, 3.4f, 5, -1.45f);
        Hide(room, "Back Wall");
        var newWindow = Spawn(window, architecture, "Window - Click to Look Outside", new Vector3(0.5f, 2.15f, 6.32f), 0, new Vector3(2, 1.5f / 1.4f, 1));
        newWindow.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Window);
        HideOriginal(room, "Window - Click to Look Outside", newWindow);
        // Keep desktop story objects at their original coordinates. The replacement desk is sized to support them.
        Spawn(desk, furniture, "Interior Kit Desk", new Vector3(2.5f, 0, 5.35f), 180, new Vector3(2.3f, 1.52f, 2.5f));
        Hide(room, "Desk", "Desk Leg Left", "Desk Leg Right", "Desk Drawers");
        Spawn(chair, furniture, "Interior Kit Office Chair", new Vector3(2.65f, 0, 4.05f), 0, Vector3.one * 1.25f);
        Hide(room, "Chair Seat", "Chair Back", "Chair Leg");
        var newBed = Spawn(bed, furniture, "Interior Kit Bed - Sleep", new Vector3(-2.8f, 0, 4.7f), 180, new Vector3(1.6f, 1.1f, 1.1f));
        newBed.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Sleep);
        Hide(room, "Bed Frame", "Bed Headboard", "Mattress - Click to Sleep", "Pillow", "Rumpled Blanket");
        Spawn(rug, furniture, "Interior Kit Rug", new Vector3(-0.8f, 0.005f, 3.15f), 0, new Vector3(4.2f / 3.05f, 1, 1.4f));
        Hide(room, "Rug");
        ReplaceDoor(room, architecture);
        BuildBathroomDoor(room, architecture);
        AlignComputer(room);
        if(additionalProps!=null) additionalProps.Apply(room);
        Debug.Log("INTERIOR_GAME_RETROFIT_OK: explicit replacements applied; unmatched props preserved.");
    }

    private void BuildBathroomDoor(Transform room, Transform architecture)
    {
        var oldFrame = Find(room, "Bathroom Door Frame");
        oldFrame.gameObject.SetActive(false);
        var assembly = Spawn(door, architecture, "Bathroom Door", new Vector3(-1.65f, 0.05f, 0.43f), 90, Vector3.one);
        var hinge = new GameObject("Bathroom Door Hinge").transform;
        hinge.SetParent(assembly.transform, false);
        hinge.localPosition = new Vector3(-0.45f, 0, 0);
        foreach (var mf in assembly.GetComponentsInChildren<MeshFilter>().ToArray())
        {
            if (!mf.name.StartsWith("Door Leaf", StringComparison.Ordinal)) continue;
            foreach (var collider in mf.GetComponents<Collider>()) { collider.enabled = false; Destroy(collider); }
            mf.transform.SetParent(hinge, true);
        }
        var leaf = hinge.gameObject.AddComponent<BoxCollider>();
        leaf.center = new Vector3(0.45f, 1.05f, 0);
        leaf.size = new Vector3(0.9f, 2.1f, 0.055f);
        assembly.AddComponent<BathroomDoorController>().Initialize(hinge, leaf);
        assembly.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Door);
        Spawn(wall, architecture, "Bathroom Door Fixed Side", new Vector3(-1.65f, 0, 1.2f), 90, new Vector3(0.25f, 2.3f / 3f, 0.16f / 0.18f));
        Spawn(wall, architecture, "Bathroom Door Lintel", new Vector3(-1.65f, 2.22f, 0.43f), 90, new Vector3(0.52f, 0.08f / 3f, 0.16f / 0.18f));
    }

    private static void AlignComputer(Transform room)
    {
        Transform monitor = Find(room, "Computer Monitor"), screen = Find(room, "Computer Screen");
        var computer = new GameObject("Computer Setup").transform;
        computer.SetParent(room, false); computer.position = monitor.position;
        monitor.SetParent(computer, true); screen.SetParent(computer, true);
        computer.rotation = Quaternion.Euler(0, -90, 0);
        computer.position = new Vector3(2.65f, 1.48f, 5.85f);
        Find(room, "Keyboard").position = new Vector3(2.65f, 1.18f, 5.1f);
    }

    private void ReplaceDoor(Transform room, Transform architecture)
    {
        Transform hinge = Find(room, "Entrance Door Hinge");
        var replacement = Spawn(door, architecture, "Interior Kit Entrance Frame", new Vector3(1, 0, -1.32f), 0, new Vector3(1.65f / 0.9f, 3.3f / 2.1f, 1));
        foreach (var mf in replacement.GetComponentsInChildren<MeshFilter>().ToArray())
            if (mf.name.StartsWith("Door Leaf", StringComparison.Ordinal)) mf.transform.SetParent(hinge, true);
        // Existing DoorEndingController rotates this exact hinge on the original Day 9/10 choices.
        hinge.gameObject.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Door);
        replacement.AddComponent<PrototypeInteractionTarget>().SetType(PrototypeInteractionTarget.InteractionType.Door);
        Hide(room, "Door - Click to Interact", "Door Handle");
    }

    private void ReplaceWall(Transform room, Transform parent, string name, float yaw)
    {
        var old = Find(room, name);
        var bounds = old.GetComponent<Renderer>().bounds;
        float width = yaw == 0 ? bounds.size.x : bounds.size.z;
        float depth = yaw == 0 ? bounds.size.z : bounds.size.x;
        Spawn(wall, parent, "Interior Kit " + name, new Vector3(bounds.center.x, bounds.min.y, bounds.center.z), yaw,
            new Vector3(width / 2, bounds.size.y / 3, depth / 0.18f));
        old.gameObject.SetActive(false);
    }

    private void WallSpan(Transform parent, string name, float minX, float maxX, float minY, float maxY, float z)
    {
        int count = Mathf.CeilToInt((maxX - minX) / 2);
        float width = (maxX - minX) / count;
        for (int i = 0; i < count; i++)
            Spawn(wall, parent, name + " " + i, new Vector3(minX + width * (i + 0.5f), minY, z), 0,
                new Vector3(width / 2, (maxY - minY) / 3, 0.25f / 0.18f));
    }

    private static GameObject Spawn(GameObject prefab, Transform parent, string name, Vector3 position, float yaw, Vector3 scale)
    {
        var instance = Instantiate(prefab, position, Quaternion.Euler(0, yaw, 0), parent);
        instance.name = name; instance.transform.localScale = scale;
        return instance;
    }

    private static Transform Find(Transform root, string name) => root.GetComponentsInChildren<Transform>(true).First(t => t.name == name);
    private static void Hide(Transform root, params string[] names)
    {
        foreach (string name in names) Find(root, name).gameObject.SetActive(false);
    }
    private static void HideOriginal(Transform root, string name, GameObject replacement)
    {
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name && t.gameObject != replacement) t.gameObject.SetActive(false);
    }
}
