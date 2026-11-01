using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private DialogueDatabase dialogueDatabase;
    [SerializeField] private string startingDialogueNodeId = "friend_message";

    [Header("Existing Systems")]
    [SerializeField] private ChoiceUI choiceUI;
    [SerializeField] private ChoiceManager choiceManager;

    private DialogueNode currentNode;

    public void StartDialogue()
    {
        StartDialogue(startingDialogueNodeId);
    }

    public void StartDialogue(string nodeId)
    {
        if (dialogueDatabase == null || choiceUI == null || choiceManager == null)
        {
            Debug.LogError("DialogueManager references are not assigned.");
            return;
        }

        ShowNode(nodeId);
    }

    private void ShowNode(string nodeId)
    {
        if (!dialogueDatabase.TryGetNode(nodeId, out currentNode))
        {
            Debug.LogError("Dialogue node was not found: " + nodeId);
            return;
        }

        choiceUI.ShowDialogue(currentNode.speakerName, currentNode.dialogueText,
            currentNode.choices, SelectDialogueChoice);
    }

    private void SelectDialogueChoice(int optionIndex)
    {
        if (currentNode == null || currentNode.choices == null ||
            optionIndex < 0 || optionIndex >= currentNode.choices.Length)
        {
            Debug.LogError("The selected dialogue choice is invalid.");
            return;
        }

        DialogueChoice choice = currentNode.choices[optionIndex];
        string nextStoryId = string.IsNullOrWhiteSpace(choice.nextStoryId)
            ? GameStateManager.Instance.CurrentState.storyNodeId
            : choice.nextStoryId;

        choiceManager.SelectChoice(currentNode.nodeId, choice.optionId,
            choice.mentalChange, nextStoryId);

        if (string.IsNullOrWhiteSpace(choice.nextDialogueNodeId))
        {
            currentNode = null;
            return;
        }

        ShowNode(choice.nextDialogueNodeId);
    }
}
