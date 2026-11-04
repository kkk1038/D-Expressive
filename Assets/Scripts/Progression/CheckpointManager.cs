
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [System.Serializable]
    private class CheckpointCollection
    {
        public GameStateData latest;
        public System.Collections.Generic.List<GameStateData> byDay = new();
    }

    private GameStateData checkpoint;
    private readonly Dictionary<int, GameStateData> checkpointsByDay = new();

    public void SaveCheckpoint()
    {
        GameStateData current =
            GameStateManager.Instance.CurrentState;

        string json = JsonUtility.ToJson(current);
        checkpoint = JsonUtility.FromJson<GameStateData>(json);
        checkpointsByDay[current.day] = checkpoint;

        Debug.Log(current.day + "일 차 체크포인트 저장 완료!");
    }

    public GameStateData LoadCheckpoint()
    {
        if (checkpoint == null)
        {
            Debug.LogWarning("저장된 체크포인트가 없습니다.");
            return null;
        }

        string json = JsonUtility.ToJson(checkpoint);

        return JsonUtility.FromJson<GameStateData>(json);
    }

    /// <summary>
    /// Restores the requested day. If a checkpoint for that exact day was not
    /// saved yet, use the newest earlier day instead of jumping forward.
    /// </summary>
    public GameStateData LoadCheckpointForDay(int requestedDay)
    {
        int selectedDay = -1;
        foreach (int savedDay in checkpointsByDay.Keys)
        {
            if (savedDay <= requestedDay && savedDay > selectedDay)
            {
                selectedDay = savedDay;
            }
        }

        if (selectedDay < 0)
        {
            return LoadCheckpoint();
        }

        string json = JsonUtility.ToJson(checkpointsByDay[selectedDay]);
        return JsonUtility.FromJson<GameStateData>(json);
    }

    public string ExportCheckpoints()
    {
        CheckpointCollection collection = new() { latest = checkpoint };
        foreach (GameStateData saved in checkpointsByDay.Values)
        {
            string json = JsonUtility.ToJson(saved);
            collection.byDay.Add(JsonUtility.FromJson<GameStateData>(json));
        }
        return JsonUtility.ToJson(collection);
    }

    public void ImportCheckpoints(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        CheckpointCollection collection = JsonUtility.FromJson<CheckpointCollection>(json);
        if (collection == null) return;
        checkpoint = collection.latest;
        checkpointsByDay.Clear();
        if (collection.byDay == null) return;
        foreach (GameStateData saved in collection.byDay)
        {
            if (saved == null) continue;
            checkpointsByDay[saved.day] = saved;
        }
    }
}
