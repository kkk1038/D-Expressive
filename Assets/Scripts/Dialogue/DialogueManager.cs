using UnityEngine;
using System.Collections;

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

    /// <summary>Ends a pending dialogue without recording a choice.</summary>
    public void EndDialogue()
    {
        currentNode = null;
        if (choiceUI != null) choiceUI.HideChoices();
    }

    private void ShowNode(string nodeId)
    {
        if (!dialogueDatabase.TryGetNode(nodeId, out currentNode))
        {
            Debug.LogError("Dialogue node was not found: " + nodeId);
            return;
        }

        bool heavy = (GameStateManager.Instance.CurrentState.day == 5 || GameStateManager.Instance.CurrentState.day == 6) &&
            (currentNode.nodeId.EndsWith("_mirror") || currentNode.nodeId.EndsWith("_medicine") || currentNode.nodeId.EndsWith("_meal") || currentNode.nodeId.EndsWith("_phone"));
        choiceUI.ShowDialogue(currentNode.speakerName, currentNode.dialogueText,
            currentNode.choices, SelectDialogueChoice, heavy);
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

        bool heavy = (GameStateManager.Instance.CurrentState.day == 5 || GameStateManager.Instance.CurrentState.day == 6) &&
            (currentNode.nodeId.EndsWith("_mirror") || currentNode.nodeId.EndsWith("_medicine") || currentNode.nodeId.EndsWith("_meal") || currentNode.nodeId.EndsWith("_phone"));
        if (heavy)
        {
            string nextNode = choice.nextDialogueNodeId;
            currentNode = null;
            StartCoroutine(ContinueAfterHeavyPause(nextNode));
            return;
        }

        if (string.IsNullOrWhiteSpace(choice.nextDialogueNodeId))
        {
            currentNode = null;
            return;
        }

        ShowNode(choice.nextDialogueNodeId);
    }

    private IEnumerator ContinueAfterHeavyPause(string nextNodeId)
    {
        yield return new WaitForSecondsRealtime(0.5f);
        if (!string.IsNullOrWhiteSpace(nextNodeId)) ShowNode(nextNodeId);
    }
}
