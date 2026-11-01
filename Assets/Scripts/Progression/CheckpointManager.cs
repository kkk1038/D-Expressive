
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private GameStateData checkpoint;

    public void SaveCheckpoint()
    {
        GameStateData current =
            GameStateManager.Instance.CurrentState;

        string json = JsonUtility.ToJson(current);
        checkpoint = JsonUtility.FromJson<GameStateData>(json);

        Debug.Log("체크포인트 저장 완료!");
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
}
