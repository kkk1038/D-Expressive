
using UnityEngine;
using UnityEngine.InputSystem;

public class ChoiceUITester : MonoBehaviour
{
    [SerializeField] private ChoiceUI choiceUI;
    [SerializeField] private DialogueManager dialogueManager;

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.tKey.wasPressedThisFrame)
        {
            Debug.Log("T key pressed!");
            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue();
            }
            else
            {
                choiceUI.ShowChoices();
            }
        }
    }
}
