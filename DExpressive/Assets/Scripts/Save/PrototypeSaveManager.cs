using System.IO;
using UnityEngine;

/// <summary>Single-slot local save for the playable prototype.</summary>
public static class PrototypeSaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "dexpressive_save.json");
    public static bool HasSave => File.Exists(SavePath);

    public static void Save()
    {
        if (GameStateManager.Instance == null) return;
        File.WriteAllText(SavePath, JsonUtility.ToJson(GameStateManager.Instance.CurrentState, true));
    }

    public static bool TryLoad()
    {
        if (!HasSave || GameStateManager.Instance == null) return false;

        GameStateData data = JsonUtility.FromJson<GameStateData>(File.ReadAllText(SavePath));
        if (data == null) return false;
        GameStateManager.Instance.SetState(data);
        return true;
    }
}
