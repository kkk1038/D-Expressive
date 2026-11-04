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

    /// <summary>Reserved for future non-visual focus behavior. The room keeps its natural colors.</summary>
    public void SetFocused(bool focused)
    {
        // Intentionally no material or color change.
    }
}
