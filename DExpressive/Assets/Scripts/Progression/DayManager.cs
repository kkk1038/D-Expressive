using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Owns the game's day loop. A checkpoint is made at the beginning of every
/// configured day, before that day's dialogue can change the game state.
/// </summary>
public class DayManager : MonoBehaviour
{
    [Serializable]
    public class DayDefinition
    {
        [Min(1)] public int day = 1;
        [Tooltip("DialogueDatabase node started when this day begins.")]
        public string startingDialogueNodeId;
        [Tooltip("Story node recorded when this day begins.")]
        public string startingStoryNodeId;
        [Tooltip("Applied once when this day begins. Negative values reduce mental state.")]
        public int dayStartMentalChange;
        [Tooltip("This interaction must be completed before the player can sleep. Configure a different value for each day in the Inspector.")]
        public PrototypeInteractionTarget.InteractionType mainGoalInteraction = PrototypeInteractionTarget.InteractionType.Phone;
        [Tooltip("When set, every listed interaction is required before sleep and is the only set used for the sleep penalty.")]
        public PrototypeInteractionTarget.InteractionType[] mainGoalInteractions;
    }

    [Header("Existing Systems")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private MentalStateManager mentalStateManager;
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private RollbackManager rollbackManager;

    [Header("Day Content")]
    [SerializeField] private DayDefinition[] days;
    [SerializeField] private bool startCurrentDayOnStart = true;
    [SerializeField, Min(0)] private int incompleteInteractionPenalty = 10;

    private static readonly PrototypeInteractionTarget.InteractionType[] ChecklistInteractions =
    {
        PrototypeInteractionTarget.InteractionType.Medicine,
        PrototypeInteractionTarget.InteractionType.Meal,
        PrototypeInteractionTarget.InteractionType.Tidy,
        PrototypeInteractionTarget.InteractionType.Shower,
        PrototypeInteractionTarget.InteractionType.Mirror,
        PrototypeInteractionTarget.InteractionType.Phone
    };

    public int CurrentDay => GameStateManager.Instance == null ? 0 : GameStateManager.Instance.CurrentState.day;
    public event Action<int> OnDayStarted;
    public event Action<int> OnDayEnded;
    public event Action<int> OnDailyActionRegistered;

    private bool isBeginningDay;
    private bool rolledBackDuringSleep;
    private bool rolledBackDuringDayBegin;
    private bool isSleeping;
    private Image collapseFadeImage;
    private TMP_Text collapseLabel;

    private void Awake()
    {
        if (days == null || days.Length == 0)
        {
            // Usable defaults for the supplied DialogueDatabase asset. They can be
            // replaced in the Inspector as the story gains more days.
            days = new[]
            {
                new DayDefinition { day = 1, startingDialogueNodeId = "day1_start", startingStoryNodeId = "day1_start" },
                new DayDefinition { day = 2, startingDialogueNodeId = "day2_start", startingStoryNodeId = "day2_start", dayStartMentalChange = -3 },
                new DayDefinition
                {
                    day = 3,
                    startingDialogueNodeId = "day3_start",
                    startingStoryNodeId = "day3_start",
                    dayStartMentalChange = -15,
                    mainGoalInteractions = new[]
                    {
                        PrototypeInteractionTarget.InteractionType.Phone,
                        PrototypeInteractionTarget.InteractionType.Meal,
                        PrototypeInteractionTarget.InteractionType.Shower
                    }
                },
                new DayDefinition { day = 4, startingDialogueNodeId = "day4_start", startingStoryNodeId = "day4_start", dayStartMentalChange = -20, mainGoalInteractions = new[] { PrototypeInteractionTarget.InteractionType.Phone, PrototypeInteractionTarget.InteractionType.Meal, PrototypeInteractionTarget.InteractionType.Shower } },
                new DayDefinition { day = 5, startingDialogueNodeId = "day5_start", startingStoryNodeId = "day5_start", dayStartMentalChange = -15, mainGoalInteractions = new[] { PrototypeInteractionTarget.InteractionType.Mirror, PrototypeInteractionTarget.InteractionType.Medicine, PrototypeInteractionTarget.InteractionType.Meal } },
                new DayDefinition { day = 6, startingDialogueNodeId = "day6_start", startingStoryNodeId = "day6_start", dayStartMentalChange = -6, mainGoalInteractions = new[] { PrototypeInteractionTarget.InteractionType.Phone, PrototypeInteractionTarget.InteractionType.Mirror, PrototypeInteractionTarget.InteractionType.Meal } },
                new DayDefinition { day = 7, startingDialogueNodeId = "day7_start", startingStoryNodeId = "day7_start", mainGoalInteractions = new[] { PrototypeInteractionTarget.InteractionType.Phone } },
                new DayDefinition { day = 8, startingDialogueNodeId = "day8_start", startingStoryNodeId = "day8_start", dayStartMentalChange = -2, mainGoalInteractions = new[] { PrototypeInteractionTarget.InteractionType.Phone, PrototypeInteractionTarget.InteractionType.Meal, PrototypeInteractionTarget.InteractionType.Window } },
                new DayDefinition { day = 9, startingDialogueNodeId = "day9_start", startingStoryNodeId = "day9_start", mainGoalInteractions = new[] { PrototypeInteractionTarget.InteractionType.Phone, PrototypeInteractionTarget.InteractionType.Meal, PrototypeInteractionTarget.InteractionType.Door } },
                new DayDefinition { day = 10, startingDialogueNodeId = "day10_start", startingStoryNodeId = "day10_start" }
            };
        }
    }

    private void OnEnable()
    {
        if (rollbackManager != null)
        {
            rollbackManager.OnRollbackCompleted += RestartRestoredDay;
        }
    }

    private void OnDisable()
    {
        if (rollbackManager != null)
        {
            rollbackManager.OnRollbackCompleted -= RestartRestoredDay;
        }
    }

    private void Start()
    {
        if (startCurrentDayOnStart)
        {
            // ChoiceUI also initializes in Start. Waiting one frame prevents its
            // initial panel hide from concealing the first day's dialogue.
            StartCoroutine(BeginInitialDay());
        }
    }

    private IEnumerator BeginInitialDay()
    {
        yield return null;
        BeginDay(CurrentDay, true);
    }

    /// <summary>Call this from the sleep button or a bed interaction.</summary>
public void GoToSleep()
    {
        if (isSleeping) return;

        int currentDay = CurrentDay;
        DayDefinition current = GetDay(currentDay);
        if (current == null)
        {
            Debug.LogWarning("No day definition exists for day " + CurrentDay + ".");
            return;
        }

        int nextDay = currentDay + 1;
        if (GetDay(nextDay) == null)
        {
            Debug.Log("There is no configured next day after day " + CurrentDay + ".");
            return;
        }

        if (!HasCompletedMainGoal())
        {
            string remainingGoals = GetIncompleteMainGoalDescription();
            Debug.Log("메인 목표를 완료해야 잠을 잘 수 있습니다: " + remainingGoals);
            GameplayNoticeUI.Show("아직 잠들 수 없습니다.\n남은 메인 목표: " + remainingGoals, 3.2f);
            return;
        }

        if (currentDay == 4)
        {
            isSleeping = true;
            StartCoroutine(PlayDayFourCollapse());
            return;
        }

        int incompleteCount = GetIncompleteChecklistCount();
        int penalty = incompleteCount * incompleteInteractionPenalty;
        if (penalty > 0)
        {
            rolledBackDuringSleep = false;
            mentalStateManager.ChangeMentalState(-penalty);
            Debug.Log("완료하지 않은 상호작용 " + incompleteCount + "개로 정신력이 " + penalty + " 감소했습니다.");

            if (rolledBackDuringSleep) return;
        }

        OnDayEnded?.Invoke(currentDay);
        isSleeping = true;
        StartCoroutine(AdvanceToNextDayAfterFade(nextDay));
    }

    private IEnumerator AdvanceToNextDayAfterFade(int nextDay)
    {
        // SleepTransitionController reaches a full black screen at 0.35 seconds.
        // Prepare the new day's dialogue behind that screen so it never flashes
        // in front of the sleep transition.
        yield return new WaitForSeconds(0.38f);
        GameStateManager.Instance.ChangeDay(nextDay);
        BeginDay(nextDay, true);

        // Keep repeated bed interactions from stacking day transitions while
        // the existing fade finishes and the new dialogue takes focus.
        yield return new WaitForSeconds(1.2f);
        isSleeping = false;
    }

    private IEnumerator PlayDayFourCollapse()
    {
        CreateCollapseUI();
        if (collapseFadeImage == null)
        {
            isSleeping = false;
            yield break;
        }

        collapseFadeImage.transform.SetAsLastSibling();
        collapseLabel.text = "조금만... 누워 있자...";
        FirstPersonPlayerController player = FindFirstObjectByType<FirstPersonPlayerController>();
        if (player != null) player.enabled = false;

        Camera camera = Camera.main;
        Quaternion originalRotation = camera != null ? camera.transform.localRotation : Quaternion.identity;
        float originalAudioVolume = AudioListener.volume;
        int remainingDrops = Mathf.Min(12, Mathf.Max(0, GameStateManager.Instance.CurrentState.mentalState - 20));
        float elapsed = 0f;
        float nextDropAt = 0.18f;
        const float duration = 2.4f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (remainingDrops > 0 && elapsed >= nextDropAt)
            {
                mentalStateManager.ChangeMentalState(-1);
                remainingDrops--;
                nextDropAt += 0.18f;
            }

            float amount = Mathf.Clamp01(elapsed / duration);
            SetCollapseAlpha(Mathf.SmoothStep(0f, 1f, amount));
            AudioListener.volume = Mathf.Lerp(originalAudioVolume, 0f, amount);
            if (camera != null)
            {
                float sway = Mathf.Sin(elapsed * 13f) * (1f - amount) * 1.2f;
                camera.transform.localRotation = originalRotation * Quaternion.Euler(0f, 0f, amount * 9f + sway);
            }
            yield return null;
        }

        SetCollapseAlpha(1f);
        collapseLabel.text = "의식이 끊긴다.";
        yield return new WaitForSecondsRealtime(0.7f);

        if (camera != null) camera.transform.localRotation = originalRotation;
        GameStateManager.Instance.ChangeDay(5);
        BeginDay(5, true);

        float fadeElapsed = 0f;
        while (fadeElapsed < 0.65f)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            float amount = Mathf.Clamp01(fadeElapsed / 0.65f);
            SetCollapseAlpha(1f - amount);
            AudioListener.volume = Mathf.Lerp(0f, originalAudioVolume, amount);
            yield return null;
        }

        SetCollapseAlpha(0f);
        collapseLabel.text = string.Empty;
        AudioListener.volume = originalAudioVolume;
        if (player != null) player.enabled = true;
        isSleeping = false;
    }

    private void CreateCollapseUI()
    {
        if (collapseFadeImage != null) return;
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject overlay = new GameObject("Day 4 Collapse", typeof(RectTransform), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        collapseFadeImage = overlay.GetComponent<Image>();
        collapseFadeImage.color = new Color(0f, 0f, 0f, 0f);
        collapseFadeImage.raycastTarget = false;

        GameObject labelObject = new GameObject("Collapse Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(overlay.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(700f, 90f);
        collapseLabel = labelObject.GetComponent<TMP_Text>();
        collapseLabel.alignment = TextAlignmentOptions.Center;
        collapseLabel.fontSize = 32f;
        collapseLabel.color = new Color(0.9f, 0.9f, 0.95f, 0.9f);
    }

    private void SetCollapseAlpha(float alpha)
    {
        Color color = collapseFadeImage.color;
        color.a = alpha;
        collapseFadeImage.color = color;
    }

    /// <summary>Called by a world interaction to update the day's checklist.</summary>
    public void RegisterInteraction(PrototypeInteractionTarget.InteractionType interaction)
    {
        string actionFlag = GetInteractionFlag(CurrentDay, interaction);
        GameStateData state = GameStateManager.Instance.CurrentState;
        if (!state.completedInteractions.Contains(actionFlag))
        {
            state.completedInteractions.Add(actionFlag);
            string dailyActionFlag = GetDailyActionFlag(CurrentDay);
            if (!state.completedInteractions.Contains(dailyActionFlag))
            {
                state.completedInteractions.Add(dailyActionFlag);
            }
            OnDailyActionRegistered?.Invoke(CurrentDay);
        }
    }

    public bool IsInteractionCompleted(PrototypeInteractionTarget.InteractionType interaction)
    {
        return GameStateManager.Instance != null && GameStateManager.Instance.CurrentState.completedInteractions.Contains(GetInteractionFlag(CurrentDay, interaction));
    }

    public int GetIncompleteChecklistCount()
    {
        int incomplete = 0;
        foreach (PrototypeInteractionTarget.InteractionType interaction in GetPenaltyInteractions())
        {
            if (!IsInteractionCompleted(interaction)) incomplete++;
        }
        return incomplete;
    }

    public PrototypeInteractionTarget.InteractionType GetMainGoalInteraction()
    {
        DayDefinition definition = GetDay(CurrentDay);
        return definition == null ? PrototypeInteractionTarget.InteractionType.Phone : definition.mainGoalInteraction;
    }

    public bool HasCompletedMainGoal()
    {
        foreach (PrototypeInteractionTarget.InteractionType interaction in GetMainGoalInteractions())
        {
            if (!IsInteractionCompleted(interaction)) return false;
        }
        return true;
    }

    public PrototypeInteractionTarget.InteractionType[] GetMainGoalInteractions()
    {
        DayDefinition definition = GetDay(CurrentDay);
        if (definition != null && definition.mainGoalInteractions != null && definition.mainGoalInteractions.Length > 0)
        {
            return definition.mainGoalInteractions;
        }

        return new[] { GetMainGoalInteraction() };
    }

    public PrototypeInteractionTarget.InteractionType[] GetPenaltyInteractions()
    {
        DayDefinition definition = GetDay(CurrentDay);
        if (definition != null && definition.mainGoalInteractions != null && definition.mainGoalInteractions.Length > 0)
        {
            return definition.mainGoalInteractions;
        }

        return ChecklistInteractions;
    }

    public string GetMainGoalDescription()
    {
        PrototypeInteractionTarget.InteractionType[] interactions = GetMainGoalInteractions();
        string description = string.Empty;
        foreach (PrototypeInteractionTarget.InteractionType interaction in interactions)
        {
            if (description.Length > 0) description += " / ";
            description += GetInteractionLabel(interaction);
        }
        return description;
    }

    public string GetIncompleteMainGoalDescription()
    {
        string description = string.Empty;
        foreach (PrototypeInteractionTarget.InteractionType interaction in GetMainGoalInteractions())
        {
            if (IsInteractionCompleted(interaction)) continue;
            if (description.Length > 0) description += " / ";
            description += GetInteractionLabel(interaction);
        }
        return description;
    }

    public static string GetInteractionLabel(PrototypeInteractionTarget.InteractionType interaction)
    {
        return interaction switch
        {
            PrototypeInteractionTarget.InteractionType.Medicine => "약",
            PrototypeInteractionTarget.InteractionType.Meal => "식사",
            PrototypeInteractionTarget.InteractionType.Tidy => "청소",
            PrototypeInteractionTarget.InteractionType.Shower => "세면대",
            PrototypeInteractionTarget.InteractionType.Mirror => "거울",
            PrototypeInteractionTarget.InteractionType.Phone => "핸드폰 확인",
            PrototypeInteractionTarget.InteractionType.Window => "창문",
            PrototypeInteractionTarget.InteractionType.Door => "현관문",
            _ => interaction.ToString()
        };
    }

    public void BeginDay(int day, bool saveCheckpoint, bool applyStartMentalChange = true)
    {
        DayDefinition definition = GetDay(day);
        if (definition == null)
        {
            Debug.LogWarning("No day definition exists for day " + day + ".");
            return;
        }

        isBeginningDay = true;
        rolledBackDuringDayBegin = false;
        GameStateData state = GameStateManager.Instance.CurrentState;
        state.day = definition.day;
        state.storyNodeId = string.IsNullOrWhiteSpace(definition.startingStoryNodeId)
            ? state.storyNodeId
            : definition.startingStoryNodeId;

        if (applyStartMentalChange && definition.dayStartMentalChange != 0)
        {
            mentalStateManager.ChangeMentalState(definition.dayStartMentalChange);
        }

        // A zero mental state can synchronously restore an earlier checkpoint.
        // Do not continue this abandoned day's setup or overwrite that checkpoint.
        if (rolledBackDuringDayBegin)
        {
            isBeginningDay = false;
            BeginDay(CurrentDay, false, false);
            return;
        }

        state.dayStartMentalState = state.mentalState;

        // Save only after the day and its automatic mental effect are established,
        // but before any player choice for this day is recorded.
        if (saveCheckpoint)
        {
            checkpointManager.SaveCheckpoint();
        }

        isBeginningDay = false;
        OnDayStarted?.Invoke(definition.day);

        if (!string.IsNullOrWhiteSpace(definition.startingDialogueNodeId))
        {
            dialogueManager.StartDialogue(definition.startingDialogueNodeId);
        }
    }

    /// <summary>Refreshes the loaded day's UI without applying its start penalty again.</summary>
    public void ResumeLoadedDay()
    {
        DayDefinition definition = GetDay(CurrentDay);
        if (definition == null) return;

        OnDayStarted?.Invoke(definition.day);
        if (!string.IsNullOrWhiteSpace(definition.startingDialogueNodeId))
        {
            dialogueManager.StartDialogue(definition.startingDialogueNodeId);
        }
    }

    private void RestartRestoredDay()
    {
        rolledBackDuringSleep = true;
        rolledBackDuringDayBegin = true;
        if (!isBeginningDay)
        {
            // A checkpoint already includes the restored day's automatic change.
            BeginDay(CurrentDay, false, false);
        }
    }

    private static string GetDailyActionFlag(int day)
    {
        return "day_" + day + "_action_taken";
    }

    private static string GetInteractionFlag(int day, PrototypeInteractionTarget.InteractionType interaction)
    {
        return "day_" + day + "_interaction_" + interaction.ToString().ToLowerInvariant();
    }

    private DayDefinition GetDay(int day)
    {
        if (days == null) return null;

        foreach (DayDefinition definition in days)
        {
            if (definition != null && definition.day == day)
            {
                return definition;
            }
        }

        return null;
    }
}
