
using UnityEngine;

public class ChoiceManager : MonoBehaviour
{
    public event System.Action<string, string, int> OnChoiceSelected;
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
        OnChoiceSelected?.Invoke(choiceId, optionId, mentalChange);
    }
}
