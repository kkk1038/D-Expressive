using UnityEngine;

/// <summary>Plays short one-shot sounds tied to room interactions.</summary>
public class InteractionSoundController : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float phoneVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float interactionVolume = 0.48f;

    private static InteractionSoundController instance;
    private AudioSource soundSource;
    private AudioClip phoneInteractionClip;
    private AudioClip mealClip;
    private AudioClip showerClip;
    private AudioClip doorClip;
    private ChoiceManager choiceManager;

    private void Awake()
    {
        instance = this;
        soundSource = gameObject.AddComponent<AudioSource>();
        soundSource.playOnAwake = false;
        soundSource.spatialBlend = 0f;
        phoneInteractionClip = Resources.Load<AudioClip>("Audio/SFX/Phone_Vibration_Notification");
        mealClip = Resources.Load<AudioClip>("Audio/SFX/Meal_Cutlery_In_Ceramic");
        showerClip = Resources.Load<AudioClip>("Audio/SFX/Shower_Real_Water");
        doorClip = Resources.Load<AudioClip>("Audio/SFX/Door_Key_In_Lock");
        if (phoneInteractionClip == null || mealClip == null || showerClip == null || doorClip == null)
        {
            Debug.LogError("상호작용 효과음 파일을 불러오지 못했습니다.");
        }
    }

    private void Start()
    {
        choiceManager = FindFirstObjectByType<ChoiceManager>();
        if (choiceManager != null) choiceManager.OnChoiceSelected += HandleChoiceSelected;
    }

    private void OnDestroy()
    {
        if (choiceManager != null) choiceManager.OnChoiceSelected -= HandleChoiceSelected;
    }

    public static void PlayPhoneInteraction()
    {
        if (instance == null || instance.phoneInteractionClip == null) return;
        instance.soundSource.PlayOneShot(instance.phoneInteractionClip, instance.phoneVolume);
    }

    public static void PlayShowerInteraction()
    {
        if (instance == null || instance.showerClip == null) return;
        instance.soundSource.PlayOneShot(instance.showerClip, instance.interactionVolume);
    }

    private void HandleChoiceSelected(string choiceId, string optionId, int mentalChange)
    {
        if (optionId == "day7_phone_parent" || optionId == "day7_phone_friend")
        {
            GameStateData state = GameStateManager.Instance.CurrentState;
            if (!state.storyFlags.Contains("day7_contact_recovery")) state.storyFlags.Add("day7_contact_recovery");
            state.mentalState = Mathf.Max(51, state.mentalState);
        }

        if (optionId == "day10_step_out" && doorClip != null)
        {
            soundSource.PlayOneShot(doorClip, interactionVolume);
            return;
        }

        if (IsEatingChoice(optionId) && mealClip != null)
        {
            soundSource.PlayOneShot(mealClip, interactionVolume);
        }
    }

    private static bool IsEatingChoice(string optionId)
    {
        if (string.IsNullOrWhiteSpace(optionId)) return false;
        if (optionId == "eat" || optionId == "day2_meal" || optionId == "day3_meal_first" || optionId == "day3_meal_second") return true;
        return optionId.Contains("_meal") && !optionId.EndsWith("_skip") && !optionId.EndsWith("_third");
    }
}
