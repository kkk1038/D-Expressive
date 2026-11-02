
using UnityEngine;

public class RollbackManager : MonoBehaviour
{
    public event System.Action OnRollbackCompleted;
    [SerializeField]
    private MentalStateManager mentalStateManager;

    [SerializeField]
    private CheckpointManager checkpointManager;

    private void OnEnable()
    {
        if (mentalStateManager != null)
        {
            mentalStateManager.OnMentalStateDepleted
                += Rollback;
        }
    }

    private void OnDisable()
    {
        if (mentalStateManager != null)
        {
            mentalStateManager.OnMentalStateDepleted
                -= Rollback;
        }
    }

    public void Rollback()
    {
        int currentDay = GameStateManager.Instance.CurrentState.day;
        // Reaching zero is a full setback: return to the beginning of the
        // previous day. Day 1 has no earlier day, so it returns to Day 1.
        int rollbackDay = Mathf.Max(1, currentDay - 1);
        GameStateData checkpoint =
            checkpointManager.LoadCheckpointForDay(rollbackDay);

        if (checkpoint == null) return;

        GameStateManager.Instance.SetState(checkpoint);

        Debug.Log("완전 회귀 실행! " + rollbackDay + "일 차로 돌아갑니다.");
        Debug.Log("복원 날짜: " + checkpoint.day);
        Debug.Log("복원 정신력: " + checkpoint.mentalState);

        OnRollbackCompleted?.Invoke();
    }
}
