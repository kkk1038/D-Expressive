using System.IO;
using System.Linq;
using System.Text;
using System;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;
using UnityEditor;
using UnityEngine;

public static class AdditionalPropsSetup
{
    public const string Root = "Assets/ThirdParty/AdditionalProps";
    [Serializable] public class Manifest { public string name; public MaterialInfo[] materials; public Part[] parts; }
    [Serializable] public class MaterialInfo { public string name, baseMap, normalMap, packedMap, alphaMode; public float[] color, emission; public float metallic, roughness, cutoff; }
    [Serializable] public class Part { public string name, group, file; public int material, triangles, sourceTriangles; }

    public static void BuildImports()
    {
        Directory.CreateDirectory(Root + "/Materials"); Directory.CreateDirectory(Root + "/Meshes"); Directory.CreateDirectory(Root + "/ImportedPrefabs");
        AssetDatabase.Refresh();
        var report = new StringBuilder();
        foreach (string path in Directory.GetFiles(Root + "/PreparedInput", "manifest.json", SearchOption.AllDirectories))
        {
            string directory = Path.GetDirectoryName(path).Replace('\\','/');
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(path));
            var materials = manifest.materials.Select((m,i) => BuildMaterial(directory, manifest.name + "_" + i, m)).ToArray();
            var root = new GameObject(manifest.name);
            var groups = new System.Collections.Generic.Dictionary<string,Transform> { [""] = root.transform };
            foreach (var part in manifest.parts)
            {
                string groupPath = ""; Transform parent = root.transform;
                foreach (string segment in part.group.Split('/'))
                {
                    groupPath += "/" + segment;
                    if (!groups.TryGetValue(groupPath, out var next)) { next = new GameObject(segment).transform; next.SetParent(parent,false); groups[groupPath] = next; }
                    parent = next;
                }
                string meshPath = Root + "/Meshes/" + manifest.name + "_" + Path.GetFileNameWithoutExtension(part.file) + ".asset";
                Mesh mesh;
                using (var reader = new BinaryReader(File.OpenRead(directory + "/" + part.file)))
                {
                    int vertexCount = reader.ReadInt32(), indexCount = reader.ReadInt32();
                    var vertices = new Vector3[vertexCount]; var normals = new Vector3[vertexCount]; var uv = new Vector2[vertexCount]; var indices = new int[indexCount];
                    for(int i=0;i<vertexCount;i++) vertices[i] = new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                    for(int i=0;i<vertexCount;i++) normals[i] = new Vector3(reader.ReadSingle(),reader.ReadSingle(),reader.ReadSingle());
                    for(int i=0;i<vertexCount;i++) uv[i] = new Vector2(reader.ReadSingle(),reader.ReadSingle());
                    for(int i=0;i<indexCount;i++) indices[i] = reader.ReadInt32();
                    mesh = new Mesh { name = manifest.name + " " + part.name, indexFormat = vertexCount > 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16 };
                    mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateBounds();
                    if(normals.All(n=>n.sqrMagnitude<0.01f)) mesh.RecalculateNormals();
                    mesh.RecalculateTangents();
                }
                var existing = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if(existing) { EditorUtility.CopySerialized(mesh,existing); UnityEngine.Object.DestroyImmediate(mesh);mesh=existing; }
                else AssetDatabase.CreateAsset(mesh,meshPath);
                var child = new GameObject("Geometry " + part.material);child.transform.SetParent(parent,false);
                child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=materials[part.material];
                report.AppendLine(manifest.name+" / "+part.group+" triangles="+part.triangles+" bounds="+child.GetComponent<Renderer>().bounds);
            }
            PrefabUtility.SaveAsPrefabAsset(root,Root+"/ImportedPrefabs/"+manifest.name+".prefab");
            UnityEngine.Object.DestroyImmediate(root);
        }
        BuildFBX(report);
        File.WriteAllText("AdditionalProps-import-report.txt",report.ToString());
        AssetDatabase.SaveAssets();Debug.Log("ADDITIONAL_PROPS_IMPORTED");
    }

    private static void SaveMesh(Mesh mesh,string path)
    {
        var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(existing) { EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh); }
        else AssetDatabase.CreateAsset(mesh,path);
    }

    private static void BuildFBX(StringBuilder report)
    {
        string directory=Root+"/PreparedFBX";
        var mats=new Material[3];
        mats[0]=BuildMaterial(directory,"Bathroom_Default",new MaterialInfo { color=new[]{0.9f,0.9f,0.9f,1},roughness=0.5f });
        for(int i=1;i<=2;i++) mats[i]=BuildMaterial(directory,"Bathroom_Atlas_"+i,new MaterialInfo {color=new[]{1f,1f,1f,1f},metallic=1,roughness=0,baseMap="bath"+i+"_base.png",normalMap="bath"+i+"_normal.png",packedMap="bath"+i+"_packed.png",alphaMode=i==2?"MASK":"OPAQUE",cutoff=0.3f});
        string path=Directory.GetFiles(Root+"/SourceFBX/BathroomProps","*.fbx",SearchOption.AllDirectories).Single();
        var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.isReadable=true;importer.importAnimation=false;importer.SaveAndReimport();
        var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        var output=new GameObject("BathroomProps_Components");int piece=0;
        foreach(var mf in model.GetComponentsInChildren<MeshFilter>())
        {
            Mesh source=mf.sharedMesh;var vertices=source.vertices.Select(p=>mf.transform.TransformPoint(p)).ToArray();var normals=source.normals.Select(n=>mf.transform.localToWorldMatrix.inverse.transpose.MultiplyVector(n).normalized).ToArray();var uv=source.uv;
            int[] parents=Enumerable.Range(0,vertices.Length).ToArray();
            int Find(int x) { while(parents[x]!=x) { parents[x]=parents[parents[x]];x=parents[x]; }return x; }
            void Join(int a,int b) { parents[Find(a)]=Find(b); }
            var welded=new System.Collections.Generic.Dictionary<Vector3Int,int>();
            for(int i=0;i<vertices.Length;i++)
            {
                var p=vertices[i];var key=new Vector3Int(Mathf.RoundToInt(p.x*100000),Mathf.RoundToInt(p.y*100000),Mathf.RoundToInt(p.z*100000));
                if(welded.TryGetValue(key,out int prior)) Join(i,prior);else welded[key]=i;
            }
            for(int s=0;s<source.subMeshCount;s++) { var tr=source.GetTriangles(s);for(int i=0;i<tr.Length;i+=3) { Join(tr[i],tr[i+1]);Join(tr[i],tr[i+2]); } }
            var components=new System.Collections.Generic.Dictionary<int,System.Collections.Generic.List<(int material,int a,int b,int c)>>();
            for(int s=0;s<source.subMeshCount;s++)
            {
                var tr=source.GetTriangles(s);
                for(int i=0;i<tr.Length;i+=3) { int key=Find(tr[i]);if(!components.ContainsKey(key))components[key]=new();components[key].Add((s,tr[i],tr[i+1],tr[i+2])); }
            }
            foreach(var component in components.Values.OrderByDescending(c=>c.Count))
            {
                int[] ids=component.SelectMany(t=>new[]{t.a,t.b,t.c}).Distinct().ToArray();var remap=ids.Select((v,i)=>(v,i)).ToDictionary(p=>p.v,p=>p.i);
                var mesh=new Mesh {name="Bathroom Component "+piece};mesh.vertices=ids.Select(i=>vertices[i]).ToArray();mesh.normals=ids.Select(i=>normals[i]).ToArray();mesh.uv=ids.Select(i=>uv[i]).ToArray();mesh.subMeshCount=source.subMeshCount;
                for(int s=0;s<source.subMeshCount;s++)mesh.SetTriangles(component.Where(t=>t.material==s).SelectMany(t=>new[]{remap[t.a],remap[t.b],remap[t.c]}).ToArray(),s);
                mesh.RecalculateBounds();mesh.RecalculateTangents();string meshPath=Root+"/Meshes/BathroomComponent_"+piece+".asset";SaveMesh(mesh,meshPath);
                var obj=new GameObject("Part_"+piece);obj.transform.SetParent(output.transform,false);obj.AddComponent<MeshFilter>().sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                obj.AddComponent<MeshRenderer>().sharedMaterials=mf.GetComponent<Renderer>().sharedMaterials.Select(m=>m.name.Contains("1")?mats[1]:m.name.Contains("2")?mats[2]:mats[0]).ToArray();
                report.AppendLine(obj.name+" bounds="+obj.GetComponent<Renderer>().bounds+" tris="+component.Count);piece++;
            }
        }
        PrefabUtility.SaveAsPrefabAsset(output,Root+"/ImportedPrefabs/BathroomProps.prefab");Object.DestroyImmediate(output);Object.DestroyImmediate(model);
        path=Directory.GetFiles(Root+"/SourceFBX/Refrigerator","*.fbx",SearchOption.AllDirectories).Single();
        var fridge=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));fridge.name="Refrigerator";
        var fridgeMat=BuildMaterial(directory,"Refrigerator",new MaterialInfo {color=new[]{1f,1f,1f,1f},metallic=0.1f,roughness=0.4f,baseMap="fridge_base.png",normalMap="fridge_normal.png"});
        fridgeMat.SetTexture("_OcclusionMap",PrepareTexture(directory+"/fridge_ao.png",false,false));fridgeMat.EnableKeyword("_OCCLUSIONMAP");EditorUtility.SetDirty(fridgeMat);
        foreach(var r in fridge.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(_=>fridgeMat).ToArray();
        PrefabUtility.SaveAsPrefabAsset(fridge,Root+"/ImportedPrefabs/Refrigerator.prefab");Object.DestroyImmediate(fridge);
    }

    private static Material BuildMaterial(string directory,string name,MaterialInfo info)
    {
        string path=Root+"/Materials/"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(!material) { material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path); }
        material.shader=Shader.Find("Universal Render Pipeline/Lit");
        material.SetColor("_BaseColor",new Color(info.color[0],info.color[1],info.color[2],info.color[3]));
        material.SetFloat("_Metallic",info.metallic);material.SetFloat("_Smoothness",1-info.roughness);material.SetFloat("_Cull",0);
        if(!string.IsNullOrEmpty(info.baseMap)) material.SetTexture("_BaseMap",PrepareTexture(directory+"/"+info.baseMap,true,false));
        if(!string.IsNullOrEmpty(info.normalMap)) { material.SetTexture("_BumpMap",PrepareTexture(directory+"/"+info.normalMap,false,true));material.EnableKeyword("_NORMALMAP"); }
        if(!string.IsNullOrEmpty(info.packedMap))
        {
            var map=PrepareTexture(directory+"/"+info.packedMap,false,false);material.SetTexture("_MetallicGlossMap",map);material.SetTexture("_OcclusionMap",map);
            material.SetFloat("_Smoothness",1);material.EnableKeyword("_METALLICSPECGLOSSMAP");material.EnableKeyword("_OCCLUSIONMAP");
        }
        if(info.emission!=null && info.emission.Length>=3 && info.emission.Any(x=>x>0)) { material.SetColor("_EmissionColor",new Color(info.emission[0],info.emission[1],info.emission[2]));material.EnableKeyword("_EMISSION"); }
        if(info.alphaMode=="MASK") { material.SetFloat("_AlphaClip",1);material.SetFloat("_Cutoff",info.cutoff);material.EnableKeyword("_ALPHATEST_ON");material.renderQueue=2450; }
        if(info.alphaMode=="BLEND")
        {
            material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.SetOverrideTag("RenderType","Transparent");material.renderQueue=3000;
        }
        EditorUtility.SetDirty(material);return material;
    }

    private static Texture2D PrepareTexture(string path,bool srgb,bool normal)
    {
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        var type=normal?TextureImporterType.NormalMap:TextureImporterType.Default;
        if(importer.textureType!=type || importer.sRGBTexture!=srgb || importer.maxTextureSize>2048)
        { importer.textureType=type;importer.sRGBTexture=srgb;importer.maxTextureSize=2048;importer.SaveAndReimport(); }
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    public static void PreviewBathroomProps()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/ImportedPrefabs/BathroomProps.prefab"));
        var renderers=model.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=Color.gray;
        var light=new GameObject("Light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=2;light.transform.rotation=Quaternion.Euler(50,-35,0);
        var camera=new GameObject("Camera").AddComponent<Camera>();camera.transform.position=b.center+new Vector3(3.6f,2.5f,-4.2f);camera.transform.LookAt(b.center);camera.backgroundColor=new Color(0.18f,0.2f,0.25f);camera.clearFlags=CameraClearFlags.SolidColor;camera.fieldOfView=42;
        Capture(camera,"BathroomProps-review.png");
        camera.transform.position=b.center+new Vector3(-3.6f,2.5f,4.2f);camera.transform.LookAt(b.center);Capture(camera,"BathroomProps-review-back.png");
    }
    public static void PreviewFridge()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/GamePrefabs/Fridge.prefab"));
        RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=Color.gray;
        var light=new GameObject("Light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=2;light.transform.rotation=Quaternion.Euler(50,-35,0);
        var camera=new GameObject("Camera").AddComponent<Camera>();camera.backgroundColor=new Color(0.18f,0.2f,0.25f);camera.clearFlags=CameraClearFlags.SolidColor;camera.fieldOfView=40;
        var b=model.GetComponent<Collider>().bounds;
        camera.transform.position=b.center+new Vector3(1.2f,0.5f,-3);camera.transform.LookAt(b.center);Capture(camera,"Fridge-front.png");
        camera.transform.position=b.center+new Vector3(-1.2f,0.5f,3);camera.transform.LookAt(b.center);Capture(camera,"Fridge-back.png");
    }
    public static void Capture(Camera camera,string path)
    {
        var rt=new RenderTexture(1600,1200,24,RenderTextureFormat.ARGB32);rt.Create();
        for(int i=0;i<3;i++) RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest {destination=rt});
        var old=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(1600,1200,TextureFormat.RGB24,false);
        texture.ReadPixels(new Rect(0,0,1600,1200),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
        RenderTexture.active=old;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);
    }

    [MenuItem("DExpressive/Additional Props/Build and apply supplied models")]
    public static void ApplyToGame()
    {
        if(!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        BuildImports();
        Directory.CreateDirectory(Root+"/GamePrefabs");AssetDatabase.Refresh();
        var palette=AssetDatabase.LoadAssetAtPath<AdditionalRoomPropsPalette>("Assets/Settings/AdditionalRoomPropsPalette.asset");
        if(!palette){palette=ScriptableObject.CreateInstance<AdditionalRoomPropsPalette>();AssetDatabase.CreateAsset(palette,"Assets/Settings/AdditionalRoomPropsPalette.asset");}
        palette.phone=Variant("samsung-phone","Phone",_=>true,new Vector3(0.09f,0.012f,0.18f));
        palette.meal=Variant("free-japanese-bento-box-3d-food-model","Bento",_=>true,new Vector3(0.42f,0.12f,0.32f));
        palette.shoe=Variant("nike-tc-7900-sail","Shoe",_=>true,new Vector3(0.14f,0.14f,0.32f));
        palette.fridge=Variant("Refrigerator","Fridge",_=>true,new Vector3(1,1.85f,0.7f));
        palette.vanity=Variant("bathroom-asset-part-1","Vanity",r=>HasAncestor(r.transform,"Badrumsskåp"),new Vector3(1.2f,0.78f,0.5f));
        palette.mirror=Variant("bathroom-asset-part-1","Mirror",r=>HasAncestor(r.transform,"Spegel"),new Vector3(1.25f,1.25f,0.06f));
        palette.soap=Variant("bathroom-asset-part-1","HandSoap",r=>HasAncestor(r.transform,"Tvål_Aesop_svart.001"),new Vector3(0.09f,0.2f,0.09f));
        palette.bathMat=Variant("bathroom-asset-part-1","BathMat",r=>HasAncestor(r.transform,"Matta"),new Vector3(1.1f,0.006f,0.6f),false,true);
        palette.toilet=Variant("bathroom-asset-part-2","Toilet",r=>HasAncestor(r.transform,"Toalett_svart"),new Vector3(0.38f,0.85f,0.63f));
        palette.paper=Variant("bathroom-asset-part-2","PaperHolder",r=>HasAncestor(r.transform,"Plane.003") || HasAncestor(r.transform,"Cylinder.002"),new Vector3(0.22f,0.2f,0.18f));
        palette.brush=Variant("bathroom-asset-part-2","ToiletBrush",r=>HasAncestor(r.transform,"toalett_borste"),new Vector3(0.14f,0.52f,0.14f));
        palette.shower=Variant("bathroom-asset-part-3","Shower",r=>!HasAncestor(r.transform,"handduk_2.003") && !HasAncestor(r.transform,"handduk_2.001"),new Vector3(0.9f,2.1f,0.85f),true,true);
        palette.tub=Variant("bathroom-asset-part-4","Bathtub",r=>HasAncestor(r.transform,"badkar"),new Vector3(0.75f,0.65f,1.65f),false,true);
        palette.towelRail=Variant("bathroom-asset-part-4","TowelRail",r=>HasAncestor(r.transform,"handukstork"),new Vector3(0.2f,0.9f,0.53f));
        palette.shelf=Variant("bathroom-asset-part-4","Shelf",r=>HasAncestor(r.transform,"hylla"),new Vector3(0.45f,0.12f,0.55f),false,true);
        palette.washingMachine=Variant("BathroomProps","WashingMachine",r=>r.bounds.center.x>0.1f && r.bounds.center.z< -0.3f && r.bounds.max.y<1.02f && r.bounds.size.x<1.05f,new Vector3(0.8f,0.92f,0.7f));
        palette.trashBin=Variant("trashbin","TrashBin",r=>HasAncestor(r.transform,"Waste Basket.001_0"),new Vector3(0.38f,0.55f,0.38f));
        EditorUtility.SetDirty(palette);
        var main=AssetDatabase.LoadAssetAtPath<InteriorKitGamePalette>("Assets/Settings/InteriorKitGamePalette.asset");main.additionalProps=palette;EditorUtility.SetDirty(main);
        AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Debug.Log("ADDITIONAL_PROPS_GAME_READY");
    }
    public static void RefreshTrashBin()
    {
        var palette=AssetDatabase.LoadAssetAtPath<AdditionalRoomPropsPalette>("Assets/Settings/AdditionalRoomPropsPalette.asset");
        palette.trashBin=Variant("trashbin","TrashBin",r=>HasAncestor(r.transform,"Waste Basket.001_0"),new Vector3(0.38f,0.55f,0.38f));
        EditorUtility.SetDirty(palette);AssetDatabase.SaveAssets();
    }
    private static bool HasAncestor(Transform t,string name)
    {
        for(;t;t=t.parent)if(t.name==name)return true;return false;
    }
    private static GameObject Variant(string source,string name,Func<Renderer,bool> include,Vector3 limits,bool precise=false,bool stretch=false)
    {
        var root=new GameObject(name);
        var visual=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/ImportedPrefabs/"+source+".prefab"));visual.transform.SetParent(root.transform,false);
        foreach(var r in visual.GetComponentsInChildren<Renderer>().ToArray())if(!include(r))Object.DestroyImmediate(r.gameObject);
        var renderers=visual.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new Exception("No geometry selected for "+name);
        var bounds=renderers[0].bounds;foreach(var r in renderers)bounds.Encapsulate(r.bounds);
        var size=bounds.size;
        var scale=new Vector3(limits.x/Mathf.Max(size.x,0.0001f),limits.y/Mathf.Max(size.y,0.0001f),limits.z/Mathf.Max(size.z,0.0001f));
        if(!stretch)scale=Vector3.one*Mathf.Min(scale.x,scale.y,scale.z);
        visual.transform.localScale=scale;
        visual.transform.localPosition=Vector3.Scale(new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z),scale);
        if(name!="BathMat")
        {
            if(precise && name=="Shower")
            {
                foreach(var mf in visual.GetComponentsInChildren<MeshFilter>())
                    if(HasAncestor(mf.transform,"Dusch_vägg") || HasAncestor(mf.transform,"Dusch_vägg_2"))mf.gameObject.AddComponent<MeshCollider>().sharedMesh=mf.sharedMesh;
            }
            else
            {
                var box=root.AddComponent<BoxCollider>();box.center=new Vector3(0,size.y*scale.y/2,0);box.size=Vector3.Scale(size,scale);
            }
        }
        string path=Root+"/GamePrefabs/"+name+".prefab";
        PrefabUtility.SaveAsPrefabAsset(root,path);Object.DestroyImmediate(root);return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }
    public static void Inspect()
    {
        var report = new StringBuilder();
        foreach (var path in Directory.GetFiles(Root + "/SourceFBX", "*.fbx", SearchOption.AllDirectories))
        {
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
            report.AppendLine(path);
            foreach (var r in instance.GetComponentsInChildren<Renderer>())
            {
                report.AppendLine(r.name + " bounds=" + r.bounds + " materials=" + string.Join(",", r.sharedMaterials.Select(m => m.name)));
                foreach (var m in r.sharedMaterials)
                    report.AppendLine("  " + m.name + " color=" + m.color + " texture=" + (m.mainTexture ? m.mainTexture.name : "NONE"));
            }
            Object.DestroyImmediate(instance);
        }
        File.WriteAllText("AdditionalProps-FBX-inspection.txt", report.ToString());
    }
}
