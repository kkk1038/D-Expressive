using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

public static class DeskPropsSetup
{
    private const string Root = "Assets/ThirdParty/AdditionalProps";
    private const string MedicineSource = "Assets/ThirdParty/DeskMedicine/MedicineBag.skp";

    [MenuItem("DExpressive/Apply desk papers and medicine bag")]
    public static void Apply()
    {
        Directory.CreateDirectory(Root + "/Meshes");
        Directory.CreateDirectory(Root + "/Materials");
        Directory.CreateDirectory(Root + "/GamePrefabs");
        AssetDatabase.Refresh();
        var palette = AssetDatabase.LoadAssetAtPath<AdditionalRoomPropsPalette>("Assets/Settings/AdditionalRoomPropsPalette.asset");
        palette.deskPapers = BuildPapers();
        palette.medicineBag = BuildMedicine();
        EditorUtility.SetDirty(palette);
        AssetDatabase.SaveAssets();
        Debug.Log("DESK_PROPS_APPLIED");
    }

    public static void BuildWindows()
    {
        Environment.SetEnvironmentVariable("DEXPRESSIVE_WINDOWS_OUTPUT", "Builds/DeskPropsWindows/DExpressive.exe");
        ReleaseBuild.Windows();
    }

    private static GameObject BuildPapers()
    {
        string directory = Root + "/PreparedInput/papers";
        var manifest = JsonUtility.FromJson<AdditionalPropsSetup.Manifest>(File.ReadAllText(directory + "/manifest.json"));
        var part = manifest.parts.Single(p => p.name == "SM_PapersCluster");
        var info = manifest.materials[part.material];
        var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Desk Papers URP", doubleSidedGI = true };
        material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(directory + "/" + info.baseMap));
        if (!string.IsNullOrEmpty(info.normalMap))
        {
            string path = directory + "/" + info.normalMap;
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.NormalMap; importer.SaveAndReimport();
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(path));
            material.EnableKeyword("_NORMALMAP");
        }
        material.SetFloat("_Metallic", 0); material.SetFloat("_Smoothness", 0.075f);
        material.SetFloat("_Cull", 0); material.SetFloat("_AlphaClip", 1); material.SetFloat("_Cutoff", 0.5f);
        material.EnableKeyword("_ALPHATEST_ON"); material.renderQueue = (int)RenderQueue.AlphaTest;
        material = Save(material, Root + "/Materials/DeskPapers.mat");
        Mesh mesh;
        using (var reader = new BinaryReader(File.OpenRead(directory + "/" + part.file)))
        {
            int vertices = reader.ReadInt32(), indices = reader.ReadInt32();
            var v = new Vector3[vertices]; var n = new Vector3[vertices]; var uv = new Vector2[vertices]; var tr = new int[indices];
            for (int i = 0; i < vertices; i++) v[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            for (int i = 0; i < vertices; i++) n[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            for (int i = 0; i < vertices; i++) uv[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            for (int i = 0; i < indices; i++) tr[i] = reader.ReadInt32();
            mesh = new Mesh { name = "Desk Papers Cluster", vertices = v, normals = n, uv = uv, triangles = tr };
            mesh.RecalculateBounds(); mesh.RecalculateTangents();
        }
        mesh = Save(mesh, Root + "/Meshes/DeskPapers.asset");
        var root = new GameObject("Desk Papers"); var visual = new GameObject("Visual"); visual.transform.SetParent(root.transform, false);
        visual.AddComponent<MeshFilter>().sharedMesh = mesh; visual.AddComponent<MeshRenderer>().sharedMaterial = material;
        Normalise(root, visual, 0.48f, 0.48f);
        var result = PrefabUtility.SaveAsPrefabAsset(root, Root + "/GamePrefabs/DeskPapers.prefab"); Object.DestroyImmediate(root);
        return result;
    }

    private static GameObject BuildMedicine()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(MedicineSource);
        if (!model) throw new Exception("Medicine bag model missing");
        var root = new GameObject("Medicine Bag"); var visual = Object.Instantiate(model, root.transform); visual.name = "Visual";
        foreach (var camera in visual.GetComponentsInChildren<Camera>()) Object.DestroyImmediate(camera.gameObject);
        var materials = new System.Collections.Generic.Dictionary<Material, Material>(); int index = 0;
        foreach (var mf in visual.GetComponentsInChildren<MeshFilter>())
        {
            var renderer = mf.GetComponent<Renderer>();
            // Coplanar front/back SketchUp faces should not cast shadows onto their own printed label.
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            mf.sharedMesh = Save(Object.Instantiate(mf.sharedMesh), Root + "/Meshes/MedicineBag_" + index++ + ".asset");
            renderer.sharedMaterials = renderer.sharedMaterials.Select(source =>
            {
                if (materials.TryGetValue(source, out var prepared)) return prepared;
                Texture texture = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.mainTexture;
                if (!texture && source.HasProperty("_MainTex")) texture = source.GetTexture("_MainTex");
                var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = "Medicine " + source.name, doubleSidedGI = true };
                material.color = source.color;
                material.SetTexture("_BaseMap", texture); material.SetFloat("_Cull", 0); material.SetFloat("_Smoothness", 0.1f);
                material.SetFloat("_ReceiveShadows", 0); material.EnableKeyword("_RECEIVE_SHADOWS_OFF");
                if (source.color.a < 0.99f)
                {
                    material.SetFloat("_Surface", 1); material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha); material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0); material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                    material.renderQueue = (int)RenderQueue.Transparent; material.SetShaderPassEnabled("ShadowCaster", false);
                }
                prepared = Save(material, Root + "/Materials/MedicineBag_" + materials.Count + ".mat"); materials[source] = prepared;
                return prepared;
            }).ToArray();
        }
        if (!materials.Values.Any(m => m.GetTexture("_BaseMap") != null)) throw new Exception("Medicine printing textures were not restored");
        Normalise(root, visual, 0.26f, 0.32f);
        var result = PrefabUtility.SaveAsPrefabAsset(root, Root + "/GamePrefabs/MedicineBag.prefab"); Object.DestroyImmediate(root);
        return result;
    }

    private static void Normalise(GameObject root, GameObject visual, float width, float depth)
    {
        var renderers = visual.GetComponentsInChildren<Renderer>(); if (renderers.Length == 0) throw new Exception("Missing mesh");
        var bounds = renderers[0].bounds; foreach (var r in renderers) bounds.Encapsulate(r.bounds);
        float scale = Mathf.Min(width / Mathf.Max(0.001f, bounds.size.x), depth / Mathf.Max(0.001f, bounds.size.z));
        visual.transform.localScale *= scale;
        visual.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z) * scale;
        var box = root.AddComponent<BoxCollider>();
        box.center = new Vector3(0, Mathf.Max(0.0175f, bounds.size.y * scale / 2), 0);
        box.size = new Vector3(bounds.size.x * scale, Mathf.Max(0.035f, bounds.size.y * scale), bounds.size.z * scale);
    }

    private static T Save<T>(T asset, string path) where T : Object
    {
        var old = AssetDatabase.LoadAssetAtPath<T>(path);
        if (old) { EditorUtility.CopySerialized(asset, old); Object.DestroyImmediate(asset); return old; }
        AssetDatabase.CreateAsset(asset, path); return asset;
    }

    public static void Inspect()
    {
        AssetDatabase.Refresh();
        var importer = (ModelImporter)AssetImporter.GetAtPath(MedicineSource);
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
        importer.isReadable = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.SaveAndReimport();
        Directory.CreateDirectory("Assets/ThirdParty/DeskMedicine/Textures");
        importer.ExtractTextures("Assets/ThirdParty/DeskMedicine/Textures");
        AssetDatabase.Refresh();
        importer.SaveAndReimport();
        var report = new StringBuilder();
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(MedicineSource);
        if (!model) throw new Exception("Unity SketchUp importer did not produce a model");
        var instance = Object.Instantiate(model);
        foreach (var r in instance.GetComponentsInChildren<Renderer>())
        {
            report.AppendLine(r.name + " bounds=" + r.bounds);
            foreach (var m in r.sharedMaterials)
            {
                report.AppendLine("  Material=" + m.name + " shader=" + m.shader.name + " color=" + m.color);
                foreach (var property in m.GetTexturePropertyNames())
                {
                    var texture = m.GetTexture(property);
                    if (texture) report.AppendLine("    " + property + "=" + texture.name + " " + texture.width + "x" + texture.height + " " + AssetDatabase.GetAssetPath(texture));
                }
            }
        }
        File.WriteAllText("DeskMedicineImport.txt", report.ToString());
        Object.DestroyImmediate(instance);
        Debug.Log("DESK_MEDICINE_IMPORT_OK");
    }
}
