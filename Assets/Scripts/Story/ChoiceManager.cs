
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    [SerializeField]
    private StoryManager storyManager;

    [SerializeField]
    private MentalStateManager mentalManager;

    public void SelectChoice(
        string choiceId,
        string optionId,
        int mentalChange,
        string nextStoryId)
    {
        storyManager.ApplyChoice(
            choiceId,
            optionId,
            nextStoryId
        );

        mentalManager.ChangeMentalState(mentalChange);
    }
}
