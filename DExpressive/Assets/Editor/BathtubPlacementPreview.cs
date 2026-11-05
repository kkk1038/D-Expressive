using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BathtubPlacementPreview
{
    static BathtubPlacementPreview() => EditorApplication.delayCall += ApplyToCurrentGame;

    [MenuItem("DExpressive/Refresh bathtub placement")]
    public static void ApplyToCurrentGame()
    {
        if (!EditorApplication.isPlaying) return;
        var bathtub = GameObject.Find("Bathroom Bathtub - Wash");
        if (!bathtub) return;
        AdditionalRoomPropsPalette.FitBathtub(bathtub.transform);
        Physics.SyncTransforms();
        var bounds = bathtub.GetComponent<Collider>().bounds;
        var obstacles = new[] { "Bathroom Toilet", "Bathroom Shower - Wash" }
            .Select(GameObject.Find).Where(o => o != null)
            .SelectMany(o => o.GetComponentsInChildren<Collider>())
            .Where(c => c.enabled && bounds.Intersects(c.bounds)).Select(c => c.name).ToArray();
        File.WriteAllText("Bathtub-Size-Check.txt", "World bounds: " + bounds + "\nAdjacent fixture overlaps: " + string.Join(", ", obstacles));
        Debug.Log("BATHTUB_SIZE_APPLIED: " + bounds);
    }
}
