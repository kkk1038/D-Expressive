using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class InteriorKitGameSetup
{
    [MenuItem("DExpressive/Interior Kit/Apply assets to existing SampleScene")]
    public static void Apply()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        const string path = "Assets/Settings/InteriorKitGamePalette.asset";
        var palette = AssetDatabase.LoadAssetAtPath<InteriorKitGamePalette>(path);
        if (!palette) { palette = ScriptableObject.CreateInstance<InteriorKitGamePalette>(); AssetDatabase.CreateAsset(palette, path); }
        palette.wall = Prefab("wall_2x3"); palette.floor = Prefab("floor_2x2"); palette.ceiling = Prefab("ceiling_2x2");
        palette.bed = Prefab("bed_double"); palette.desk = Prefab("office_desk"); palette.chair = Prefab("office_chair");
        palette.rug = Prefab("rug_2x3"); palette.window = Prefab("window_1200x1400"); palette.door = Prefab("door_interior");
        EditorUtility.SetDirty(palette);
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        var bootstrap = Object.FindFirstObjectByType<PrototypeRoomBootstrap>();
        if (!bootstrap) throw new System.Exception("Existing room bootstrap not found in SampleScene");
        var obj = new SerializedObject(bootstrap);
        obj.FindProperty("interiorKitPalette").objectReferenceValue = palette;
        obj.ApplyModifiedPropertiesWithoutUndo();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("INTERIOR_GAME_SCENE_CONFIGURED");
    }
    private static GameObject Prefab(string name)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(InteriorKitSetup.Kit + "/Prefabs/" + name + ".prefab");
        if (!prefab) throw new System.Exception("Missing prepared prefab: " + name);
        return prefab;
    }
}
