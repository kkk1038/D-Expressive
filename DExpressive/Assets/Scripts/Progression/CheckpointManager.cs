
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
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
}
