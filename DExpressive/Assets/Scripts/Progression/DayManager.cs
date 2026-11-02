using System;
using System.Collections;
using UnityEngine;

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
    }

    [Header("Existing Systems")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private MentalStateManager mentalStateManager;
    [SerializeField] private CheckpointManager checkpointManager;
    [SerializeField] private RollbackManager rollbackManager;

    [Header("Day Content")]
    [SerializeField] private DayDefinition[] days;
    [SerializeField] private bool startCurrentDayOnStart = true;
    [SerializeField, Min(0)] private int noActionSleepMentalPenalty = 30;

    public int CurrentDay => GameStateManager.Instance == null ? 0 : GameStateManager.Instance.CurrentState.day;
    public event Action<int> OnDayStarted;
    public event Action<int> OnDayEnded;
    public event Action<int> OnDailyActionRegistered;

    private bool isBeginningDay;
    private bool rolledBackDuringSleep;

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
                new DayDefinition { day = 3, startingDialogueNodeId = "day3_start", startingStoryNodeId = "day3_start", dayStartMentalChange = -4 },
                new DayDefinition { day = 4, startingDialogueNodeId = "day4_start", startingStoryNodeId = "day4_start", dayStartMentalChange = -5 },
                new DayDefinition { day = 5, startingDialogueNodeId = "day5_start", startingStoryNodeId = "day5_start", dayStartMentalChange = -5 },
                new DayDefinition { day = 6, startingDialogueNodeId = "day6_start", startingStoryNodeId = "day6_start", dayStartMentalChange = -6 },
                new DayDefinition { day = 7, startingDialogueNodeId = "day7_start", startingStoryNodeId = "day7_start", dayStartMentalChange = -3 },
                new DayDefinition { day = 8, startingDialogueNodeId = "day8_start", startingStoryNodeId = "day8_start", dayStartMentalChange = -2 },
                new DayDefinition { day = 9, startingDialogueNodeId = "day9_start", startingStoryNodeId = "day9_start" },
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

        if (!HasTakenActionToday())
        {
            rolledBackDuringSleep = false;
            mentalStateManager.ChangeMentalState(-noActionSleepMentalPenalty);
            Debug.Log("오늘 아무 행동도 하지 않아 정신력이 " + noActionSleepMentalPenalty + " 감소했습니다.");

            if (rolledBackDuringSleep)
            {
                return;
            }
        }

        OnDayEnded?.Invoke(currentDay);
        GameStateManager.Instance.ChangeDay(nextDay);
        BeginDay(nextDay, true);
    }

    /// <summary>Called by a world interaction to mark this game day as lived.</summary>
    public void RegisterDailyAction()
    {
        string actionFlag = GetDailyActionFlag(CurrentDay);
        GameStateData state = GameStateManager.Instance.CurrentState;
        if (!state.completedInteractions.Contains(actionFlag))
        {
            state.completedInteractions.Add(actionFlag);
            OnDailyActionRegistered?.Invoke(CurrentDay);
        }
    }

    public void BeginDay(int day, bool saveCheckpoint)
    {
        DayDefinition definition = GetDay(day);
        if (definition == null)
        {
            Debug.LogWarning("No day definition exists for day " + day + ".");
            return;
        }

        isBeginningDay = true;
        GameStateData state = GameStateManager.Instance.CurrentState;
        state.day = definition.day;
        state.storyNodeId = string.IsNullOrWhiteSpace(definition.startingStoryNodeId)
            ? state.storyNodeId
            : definition.startingStoryNodeId;

        if (definition.dayStartMentalChange != 0)
        {
            mentalStateManager.ChangeMentalState(definition.dayStartMentalChange);
        }

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
        if (!isBeginningDay)
        {
            BeginDay(CurrentDay, false);
        }
    }

    private bool HasTakenActionToday()
    {
        return GameStateManager.Instance.CurrentState.completedInteractions.Contains(GetDailyActionFlag(CurrentDay));
    }

    private static string GetDailyActionFlag(int day)
    {
        return "day_" + day + "_action_taken";
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
