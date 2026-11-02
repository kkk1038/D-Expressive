#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>Creates runtime-safe copies of the imported furniture without changing the source pack.</summary>
[InitializeOnLoad]
public static class DExpressiveFurnitureInstaller
{
    private const string Source = "Assets/CuteMagic_Free/Cute Low Poly Furniture Pack Free/Prefab/Room_01/";
    private const string Destination = "Assets/Resources/DExpressiveFurniture/";
    private static readonly string[] Prefabs =
    {
        "Bed", "Table", "chair_01", "window", "Wardrobe", "Lamp_01", "Lamp_02",
        "Wall art_01", "Wall art_02", "Plant pot_01", "Doormat", "Computer", "Book_01"
    };

    static DExpressiveFurnitureInstaller()
    {
        EditorApplication.delayCall += InstallCopies;
    }

    private static void InstallCopies()
    {
        if (!Directory.Exists(Source) || AssetDatabase.LoadAssetAtPath<GameObject>(Destination + "Bed.prefab") != null) return;
        Directory.CreateDirectory(Destination);
        foreach (string prefab in Prefabs)
        {
            AssetDatabase.CopyAsset(Source + prefab + ".prefab", Destination + prefab + ".prefab");
        }
        AssetDatabase.SaveAssets();
        Debug.Log("D-Expressive용 가구 복제본을 만들었습니다.");
    }
}
#endif
