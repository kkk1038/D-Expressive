using UnityEngine;

/// <summary>Explicit URP material reference for room props created at runtime.</summary>
public static class RuntimeRoomMaterial
{
    private static Material template;

    public static Material Create(Color color)
    {
        if (template == null) template = Resources.Load<Material>("Materials/RuntimeRoom");
        if (template == null) throw new System.InvalidOperationException("Missing Resources/Materials/RuntimeRoom material.");
        var material = new Material(template) { name = "Runtime Room Prop" };
        material.color = color;
        return material;
    }
}
