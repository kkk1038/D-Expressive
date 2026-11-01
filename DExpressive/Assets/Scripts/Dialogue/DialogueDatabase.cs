using System;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueDatabase", menuName = "D-Expressive/Dialogue Database")]
public class DialogueDatabase : ScriptableObject
{
    [SerializeField] private DialogueNode[] nodes;

    public bool TryGetNode(string nodeId, out DialogueNode node)
    {
        if (nodes == null)
        {
            node = null;
            return false;
        }

        foreach (DialogueNode candidate in nodes)
        {
            if (candidate != null && candidate.nodeId == nodeId)
            {
                node = candidate;
                return true;
            }
        }

        if (StoryContentFactory.TryCreate(nodeId, out node))
        {
            return true;
        }

        node = null;
        return false;
    }
}

[Serializable]
public class DialogueNode
{
    [Tooltip("This value must be unique. Use this value for Next Dialogue Node Id.")]
    public string nodeId;
    public string speakerName;

    [TextArea(3, 8)] public string dialogueText;

    [Tooltip("The current ChoiceUI supports one or two choices.")]
    public DialogueChoice[] choices;
}

[Serializable]
public class DialogueChoice
{
    [Tooltip("An ID saved to GameStateData.choiceHistory.")]
    public string optionId;
    public string buttonText;

    [Tooltip("Applied through the existing MentalStateManager.")]
    public int mentalChange;

    [Tooltip("Leave empty to keep the current story node.")]
    public string nextStoryId;

    [Tooltip("Leave empty to end this conversation after this choice.")]
    public string nextDialogueNodeId;
}
