
using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public void ApplyChoice(
        string choiceId,
        string optionId,
        string nextStoryId)
    {
        GameStateData state =
            GameStateManager.Instance.CurrentState;

        // 선택 결과 저장
        state.choiceHistory.Add(new ChoiceRecord
        {
            choiceId = choiceId,
            selectedOptionId = optionId
        });

        // 선택한 사건 기록
        state.storyFlags.Add(
            choiceId + "_" + optionId
        );

        // 다음 스토리 위치 변경
        state.storyNodeId = nextStoryId;

        Debug.Log("선택: " + optionId);
        Debug.Log("다음 스토리: " + nextStoryId);
    }
}
