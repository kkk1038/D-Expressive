using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public static class InteriorKitSetup
{
    public const string Kit = "Assets/ThirdParty/InteriorKitV1";
    private static readonly System.Collections.Generic.Dictionary<string, Material> Materials = new();

    [MenuItem("DExpressive/Interior Kit/Create or rebuild test scene")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Kit + "/Materials");
        Directory.CreateDirectory(Kit + "/Packed");
        Directory.CreateDirectory(Kit + "/Prefabs");
        AssetDatabase.Refresh();
        PrepareMaterials();
        PreparePrefabs();
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.48f, 0.51f, 0.56f);
        RenderSettings.fog = false;
        var root = new GameObject("InteriorKitTest - 10m x 8m x 3m").transform;
        var shell = new GameObject("Room Shell").transform; shell.SetParent(root);
        var furniture = new GameObject("Furniture").transform; furniture.SetParent(root);
        for (int x = -4; x <= 4; x += 2)
        for (int z = -3; z <= 3; z += 2)
        {
            Place("floor_2x2", new Vector3(x, 0, z), 0, shell);
            Place("ceiling_2x2", new Vector3(x, 3, z), 0, shell);
        }
        for (int x = -4; x <= 4; x += 2)
        {
            Place(x == 0 ? "wall_door_2x3" : "wall_2x3", new Vector3(x, 0, -4), 0, shell);
            Place(x == 0 || x == 2 ? "wall_window_2x3" : "wall_2x3", new Vector3(x, 0, 4), 180, shell);
            if (x == 0 || x == 2) Place("window_1200x1400", new Vector3(x, 0.9f, 4), 180, shell);
        }
        for (int z = -3; z <= 3; z += 2)
        {
            Place("wall_2x3", new Vector3(-5, 0, z), 90, shell);
            Place("wall_2x3", new Vector3(5, 0, z), -90, shell);
        }
        // A small landing allows the player to pass through and inspect the open doorway.
        Place("floor_2x2", new Vector3(0, 0, -5), 0, shell);
        Place("ceiling_2x2", new Vector3(0, 3, -5), 0, shell);
        Place("wall_2x3", new Vector3(-1, 0, -5), 90, shell);
        Place("wall_2x3", new Vector3(1, 0, -5), -90, shell);
        Place("wall_2x3", new Vector3(0, 0, -6), 0, shell);
        CreateDoor(shell);
        Place("bed_double", new Vector3(-3.5f, 0.05f, 2.4f), 180, furniture);
        Place("office_desk", new Vector3(2, 0.04f, 3.35f), 180, furniture);
        Place("office_chair", new Vector3(2, 0.04f, 2.2f), 0, furniture);
        Place("bookshelf", new Vector3(4.65f, 0.04f, 2.5f), -90, furniture);
        Place("fridge", new Vector3(4.5f, 0.04f, -2.9f), -90, furniture);
        Place("kitchen_counter_sink", new Vector3(4.5f, 0.04f, -0.8f), -90, furniture);
        Place("sofa_3seat", new Vector3(-4.25f, 0.04f, -1.3f), 90, furniture);
        Place("coffee_table", new Vector3(-2.65f, 0.04f, -1.3f), 90, furniture);
        Place("rug_2x3", new Vector3(-2.7f, 0.045f, -1.3f), 90, furniture);
        Place("floor_lamp", new Vector3(-4.3f, 0.04f, 0.1f), 0, furniture);
        Place("dining_table", new Vector3(2.1f, 0.04f, -1.6f), 0, furniture);
        Place("dining_chair", new Vector3(2.1f, 0.04f, -2.6f), 0, furniture);
        var player = new GameObject("Interior Test Player");
        player.transform.position = new Vector3(0, 0.1f, -2.5f);
        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0, 0.9f, 0);
        cc.stepOffset = 0.25f;
        var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
        camera.tag = "MainCamera";
        camera.transform.SetParent(player.transform, false);
        camera.transform.localPosition = new Vector3(0, 1.6f, 0);
        camera.nearClipPlane = 0.03f; camera.farClipPlane = 60; camera.fieldOfView = 75;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.18f, 0.22f, 0.28f);
        var controller = player.AddComponent<FirstPersonPlayerController>();
        var serialized = new SerializedObject(controller);
        serialized.FindProperty("playerCamera").objectReferenceValue = camera;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        player.AddComponent<InteriorKitTestControls>();
        PointLight("Warm Room Light", new Vector3(-2, 2.65f, 0), new Color(1, 0.83f, 0.64f), 5, 9);
        PointLight("Cool Window Fill", new Vector3(1, 2.4f, 2.8f), new Color(0.7f, 0.83f, 1), 4, 8);
        PointLight("Kitchen Light", new Vector3(3.6f, 2.6f, -2), new Color(1, 0.94f, 0.83f), 3, 6);
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/InteriorKitTest.unity");
        AssetDatabase.SaveAssets();
        ValidateScene();
        Debug.Log("INTERIOR_BUILD_OK");
    }

    private static void PrepareMaterials()
    {
        Materials.Clear();
        foreach (string file in Directory.GetFiles(Kit + "/textures", "*.jpg"))
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(file);
            imp.textureType = file.EndsWith("_nor.jpg") ? TextureImporterType.NormalMap : TextureImporterType.Default;
            imp.sRGBTexture = file.EndsWith("_col.jpg");
            imp.isReadable = file.EndsWith("_orm.jpg");
            imp.SaveAndReimport();
        }
        string[] keys = { "books", "bulb", "ceiling", "fabric_cream", "floor_wood", "glass", "leather", "marble", "oak", "paint_white", "rug", "steel", "velvet", "wall_plaster", "walnut", "window_view" };
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (!shader) throw new System.Exception("URP Lit shader is missing");
        foreach (string key in keys)
        {
            string path = Kit + "/Materials/" + key + ".mat";
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { m = new Material(shader); AssetDatabase.CreateAsset(m, path); }
            m.shader = shader;
            m.SetColor("_BaseColor", Color.white);
            m.SetTexture("_BaseMap", Texture(key + "_col.jpg"));
            var normal = Texture(key + "_nor.jpg");
            if (!normal) normal = Texture("flat_nor.jpg");
            if (normal) { m.SetTexture("_BumpMap", normal); m.EnableKeyword("_NORMALMAP"); }
            var orm = Texture(key + "_orm.jpg");
            if (orm)
            {
                var pixels = orm.GetPixels32();
                for (int i = 0; i < pixels.Length; i++)
                {
                    var p = pixels[i]; pixels[i] = new Color32(p.b, p.r, 0, (byte)(255 - p.g));
                }
                var packed = new Texture2D(orm.width, orm.height, TextureFormat.RGBA32, false, true);
                packed.SetPixels32(pixels); packed.Apply();
                string packedPath = Kit + "/Packed/" + key + "_MAOS.png";
                File.WriteAllBytes(packedPath, packed.EncodeToPNG()); Object.DestroyImmediate(packed);
                AssetDatabase.ImportAsset(packedPath);
                var imp = (TextureImporter)AssetImporter.GetAtPath(packedPath);
                imp.sRGBTexture = false; imp.alphaSource = TextureImporterAlphaSource.FromInput; imp.SaveAndReimport();
                var map = AssetDatabase.LoadAssetAtPath<Texture2D>(packedPath);
                m.SetTexture("_MetallicGlossMap", map); m.EnableKeyword("_METALLICSPECGLOSSMAP");
                m.SetTexture("_OcclusionMap", map); m.EnableKeyword("_OCCLUSIONMAP");
                m.SetFloat("_Metallic", 1); m.SetFloat("_Smoothness", 1);
            }
            m.SetFloat("_Cull", 0); // Interior kit surfaces are visible from inside and outside.
            if (key == "bulb")
            {
                m.SetColor("_BaseColor", new Color(1, 0.92f, 0.72f));
                m.SetColor("_EmissionColor", new Color(1.5f, 1.15f, 0.65f)); m.EnableKeyword("_EMISSION");
            }
            if (key == "glass")
            {
                m.SetColor("_BaseColor", new Color(0.68f, 0.84f, 0.93f, 0.18f));
                m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0);
                m.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                m.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                m.SetFloat("_ZWrite", 0); m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                m.SetOverrideTag("RenderType", "Transparent"); m.renderQueue = 3000;
            }
            EditorUtility.SetDirty(m); Materials[key] = m;
        }
    }

    private static Texture2D Texture(string name) => AssetDatabase.LoadAssetAtPath<Texture2D>(Kit + "/textures/" + name);

    private static Material ResolveMaterial(string name)
    {
        string normalized = name.ToLowerInvariant().Replace(" (instance)", "").Replace("mat_", "");
        normalized = normalized switch { "cream" => "fabric_cream", "paint" => "paint_white", "floor" => "floor_wood", "plaster" => "wall_plaster", _ => normalized };
        foreach (var pair in Materials.OrderByDescending(p => p.Key.Length))
            if (normalized.Contains(pair.Key)) return pair.Value;
        throw new System.Exception("Unknown Interior Kit material: " + name);
    }

    private static void PreparePrefabs()
    {
        foreach (string path in Directory.GetFiles(Kit + "/fbx", "*.fbx"))
        {
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.importAnimation = false; importer.addCollider = false; importer.SaveAndReimport();
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            instance.name = Path.GetFileNameWithoutExtension(path);
            foreach (var r in instance.GetComponentsInChildren<Renderer>(true))
            {
                if (IsCollisionHelper(r.transform)) { r.enabled = false; continue; }
                r.sharedMaterials = r.sharedMaterials.Select(m => ResolveMaterial(m.name)).ToArray();
            }
            if (instance.name == "door_interior") SplitDoor(instance);
            // Use the visible static triangle geometry so openings are not replaced by solid AABBs.
            foreach (var mf in instance.GetComponentsInChildren<MeshFilter>(true))
            {
                if (IsCollisionHelper(mf.transform) || instance.name == "rug_2x3") continue;
                var mc = mf.gameObject.AddComponent<MeshCollider>(); mc.sharedMesh = mf.sharedMesh;
            }
            PrefabUtility.SaveAsPrefabAsset(instance, Kit + "/Prefabs/" + instance.name + ".prefab");
            Object.DestroyImmediate(instance);
        }
    }

    private static bool IsCollisionHelper(Transform t)
    {
        for (; t; t = t.parent) if (t.name.Contains("colonly")) return true;
        return false;
    }

    private static void SplitDoor(GameObject door)
    {
        Directory.CreateDirectory(Kit + "/Meshes");
        foreach (var mf in door.GetComponentsInChildren<MeshFilter>().ToArray())
        {
            if (IsCollisionHelper(mf.transform)) continue;
            var source = mf.sharedMesh;
            var materials = mf.GetComponent<Renderer>().sharedMaterials;
            for (int s = 0; s < source.subMeshCount; s++)
            {
                var indices = source.GetTriangles(s);
                var unique = indices.Distinct().ToArray();
                var remap = unique.Select((v, i) => (v, i)).ToDictionary(p => p.v, p => p.i);
                var mesh = new Mesh { name = "Door_" + materials[s].name };
                mesh.vertices = unique.Select(i => source.vertices[i]).ToArray();
                mesh.normals = unique.Select(i => source.normals[i]).ToArray();
                mesh.uv = unique.Select(i => source.uv[i]).ToArray();
                mesh.triangles = indices.Select(i => remap[i]).ToArray(); mesh.RecalculateBounds(); mesh.RecalculateTangents();
                string path = Kit + "/Meshes/" + mesh.name + ".asset";
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (existing) { EditorUtility.CopySerialized(mesh, existing); Object.DestroyImmediate(mesh); mesh = existing; }
                else AssetDatabase.CreateAsset(mesh, path);
                var piece = new GameObject(materials[s].name == "paint_white" ? "Door Fixed Frame" : "Door Leaf " + materials[s].name);
                piece.transform.SetParent(mf.transform.parent, false);
                piece.transform.localPosition = mf.transform.localPosition;
                piece.transform.localRotation = mf.transform.localRotation;
                piece.transform.localScale = mf.transform.localScale;
                piece.AddComponent<MeshFilter>().sharedMesh = mesh;
                piece.AddComponent<MeshRenderer>().sharedMaterial = materials[s];
            }
            Object.DestroyImmediate(mf.gameObject);
        }
    }

    private static GameObject Place(string name, Vector3 position, float yaw, Transform parent)
    {
        var obj = (GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Kit + "/Prefabs/" + name + ".prefab"));
        obj.transform.SetParent(parent); obj.transform.position = position; obj.transform.rotation = Quaternion.Euler(0, yaw, 0);
        return obj;
    }

    private static void CreateDoor(Transform parent)
    {
        var door = Place("door_interior", new Vector3(0, 0, -4), 0, parent);
        PrefabUtility.UnpackPrefabInstance(door, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
        var control = door.AddComponent<InteriorKitTestDoor>();
        // Keep the frame fixed; group only the leaf/handle render meshes around a hinge.
        var pivot = new GameObject("Door Hinge").transform;
        pivot.SetParent(door.transform, false); pivot.localPosition = new Vector3(-0.45f, 0, 0);
        foreach (var mf in door.GetComponentsInChildren<MeshFilter>().ToArray())
        {
            if (IsCollisionHelper(mf.transform)) continue;
            if (!mf.name.StartsWith("Door Leaf")) continue;
            mf.transform.SetParent(pivot, true);
        }
        control.leaf = pivot;
    }

    private static void PointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        var light = new GameObject(name).AddComponent<Light>();
        light.type = LightType.Point; light.transform.position = position;
        light.color = color; light.intensity = intensity; light.range = range;
        light.shadows = name == "Warm Room Light" ? LightShadows.Soft : LightShadows.None;
    }

    public static void ValidateScene()
    {
        Physics.SyncTransforms();
        if (Object.FindFirstObjectByType<PrototypeRoomBootstrap>() || Object.FindFirstObjectByType<DayManager>())
            throw new System.Exception("Story/bootstrap component leaked into test scene");
        foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (IsCollisionHelper(r.transform) && r.enabled) throw new System.Exception("Visible collision helper: " + r.name);
            if (!r.enabled) continue;
            foreach (var m in r.sharedMaterials)
                if (!m || m.shader.name != "Universal Render Pipeline/Lit") throw new System.Exception("Invalid URP material on " + r.name);
        }
        if (!Physics.Raycast(new Vector3(0, 1.6f, -2.5f), Vector3.down, 2)) throw new System.Exception("Spawn has no floor");
        var door = Object.FindFirstObjectByType<InteriorKitTestDoor>();
        door.Toggle();
        if (Physics.Raycast(new Vector3(0, 1, -3.5f), Vector3.back, out var doorHit, 1))
            throw new System.Exception($"Open doorway is blocked by {doorHit.collider.name} at {doorHit.point}, door scale={door.transform.lossyScale}, leaf={door.leaf.position}, leaf scale={door.leaf.lossyScale}");
        door.Toggle();
        if (!Physics.Raycast(new Vector3(-4, 1.5f, 0), Vector3.left, 2)) throw new System.Exception("Wall collider missing");
        File.WriteAllText("InteriorKitValidation.txt", "PASS: URP renderer materials, hidden colonly helpers, spawn floor, wall collision, open doorway, isolated story/save systems.\n");
    }
    public static void InspectModels()
    {
        var report = new System.Text.StringBuilder();
        foreach (string path in Directory.GetFiles(Kit + "/fbx", "*.fbx"))
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance = Object.Instantiate(model);
            report.AppendLine(path);
            foreach (var r in instance.GetComponentsInChildren<Renderer>())
                report.AppendLine($"  {r.name} bounds={r.bounds} mats={string.Join(",", r.sharedMaterials.Select(m => m ? m.name : "NULL"))}");
            Object.DestroyImmediate(instance);
        }
        File.WriteAllText("InteriorKitModelReport.txt", report.ToString());
        Debug.Log("INTERIOR_INSPECT_OK");
    }
}
