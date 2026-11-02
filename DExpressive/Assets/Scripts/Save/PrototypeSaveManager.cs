using System.IO;
using UnityEngine;

/// <summary>Single-slot local save for the playable prototype.</summary>
public static class PrototypeSaveManager
{
    [System.Serializable]
    private class SaveFile
    {
        public int version = 2;
        public GameStateData state;
        public string checkpoints;
    }
    private static string SavePath => Path.Combine(Application.persistentDataPath, "dexpressive_save.json");
    public static bool HasSave => File.Exists(SavePath);

    public static void Save()
    {
        if (GameStateManager.Instance == null) return;
        CheckpointManager checkpointManager = Object.FindFirstObjectByType<CheckpointManager>();
        SaveFile save = new()
        {
            state = GameStateManager.Instance.CurrentState,
            checkpoints = checkpointManager == null ? string.Empty : checkpointManager.ExportCheckpoints()
        };
        File.WriteAllText(SavePath, JsonUtility.ToJson(save, true));
    }

    public static bool TryLoad()
    {
        if (!HasSave || GameStateManager.Instance == null) return false;

        string json = File.ReadAllText(SavePath);
        SaveFile save = JsonUtility.FromJson<SaveFile>(json);
        GameStateData data = save == null ? null : save.state;
        // Compatible with saves made before checkpoint history was added.
        if (data == null) data = JsonUtility.FromJson<GameStateData>(json);
        if (data == null) return false;
        GameStateManager.Instance.SetState(data);
        CheckpointManager checkpointManager = Object.FindFirstObjectByType<CheckpointManager>();
        if (checkpointManager != null) checkpointManager.ImportCheckpoints(save == null ? string.Empty : save.checkpoints);
        return true;
    }
}
