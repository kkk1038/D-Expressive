
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
        GameStateData checkpoint =
            checkpointManager.LoadCheckpoint();

        if (checkpoint == null) return;

        GameStateManager.Instance.SetState(checkpoint);

        Debug.Log("완전 회귀 실행!");
        Debug.Log("복원 날짜: " + checkpoint.day);
        Debug.Log("복원 정신력: " + checkpoint.mentalState);

        OnRollbackCompleted?.Invoke();
    }
}
