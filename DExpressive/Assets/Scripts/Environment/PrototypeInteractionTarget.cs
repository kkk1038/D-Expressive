using UnityEngine;

public class PrototypeInteractionTarget : MonoBehaviour
{
    public enum InteractionType { Sleep, Phone, Window, Meal, Medicine, Door, Mirror, Shower, Tidy }

    [SerializeField] private InteractionType interactionType;
    public InteractionType Type => interactionType;

    public void SetType(InteractionType value)
    {
        interactionType = value;
    }
}
