
using UnityEngine;

public class GameStateTester : MonoBehaviour
{
    [SerializeField]
    private ChoiceManager choiceManager;

    [SerializeField]
    private CheckpointManager checkpointManager;

    private void Start()
    {
        GameStateManager game =
            GameStateManager.Instance;

        // 체크포인트 생성
        game.ChangeDay(3);
        game.CurrentState.mentalState = 80;
        game.CurrentState.storyNodeId = "day3_start";

        checkpointManager.SaveCheckpoint();

        Debug.Log("=== 체크포인트 저장 ===");

        // 4일 차 진행
        game.ChangeDay(4);

        // 첫 번째 선택
        choiceManager.SelectChoice(
            "friend_message",
            "reply",
            10,
            "friend_conversation"
        );

        // 두 번째 선택 (정신력 감소)
        choiceManager.SelectChoice(
            "stress_event",
            "ignore",
            -100,
            "stress_story"
        );

        // 회귀 후 결과 확인
        GameStateData state = game.CurrentState;

        Debug.Log("=== 회귀 결과 ===");
        Debug.Log("날짜: " + state.day);
        Debug.Log("정신력: " + state.mentalState);
        Debug.Log("스토리: " + state.storyNodeId);

        Debug.Log(
            "선택 기록 수: " +
            state.choiceHistory.Count
        );

        Debug.Log(
            "스토리 플래그 수: " +
            state.storyFlags.Count
        );
    }
}
