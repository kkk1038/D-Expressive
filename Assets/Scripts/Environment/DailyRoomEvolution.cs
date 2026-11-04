using UnityEngine;

/// <summary>Small environmental story beats that make each day feel visually distinct.</summary>
public class DailyRoomEvolution : MonoBehaviour
{
    private DayManager dayManager;
    private GameObject[] clutter;
    private Renderer noteRenderer;

    private void Start()
    {
        dayManager = FindFirstObjectByType<DayManager>();
        BuildProps();
        if (dayManager != null)
        {
            dayManager.OnDayStarted += ApplyDay;
            ApplyDay(dayManager.CurrentDay);
        }
    }

    private void OnDestroy()
    {
        if (dayManager != null) dayManager.OnDayStarted -= ApplyDay;
    }

    private void BuildProps()
    {
        clutter = new[]
        {
            CreateProp("Daily Note", new Vector3(-1.75f, 2.8f, 6.27f), new Vector3(0.55f, 0.75f, 0.025f), new Color(0.68f, 0.62f, 0.44f)),
            CreateProp("Daily Clothes A", new Vector3(-0.85f, 0.12f, 2.1f), new Vector3(0.75f, 0.10f, 0.52f), new Color(0.16f, 0.18f, 0.25f)),
            CreateProp("Daily Clothes B", new Vector3(-0.15f, 0.12f, 2.2f), new Vector3(0.55f, 0.10f, 0.36f), new Color(0.28f, 0.22f, 0.20f)),
            CreateProp("Daily Papers", new Vector3(3.30f, 1.25f, 5.55f), new Vector3(0.48f, 0.035f, 0.38f), new Color(0.72f, 0.68f, 0.58f)),
            CreateProp("Daily Empty Package", new Vector3(2.85f, 1.25f, 5.68f), new Vector3(0.22f, 0.06f, 0.32f), new Color(0.48f, 0.31f, 0.20f))
        };
        noteRenderer = clutter[0].GetComponent<Renderer>();
    }

    private void ApplyDay(int day)
    {
        if (clutter == null) return;
        // The middle days feel increasingly stalled; the final days regain a
        // little order without pretending the room is magically fixed.
        int visibleCount = day switch
        {
            <= 2 => 2,
            <= 5 => 4,
            <= 8 => 5,
            9 => 3,
            _ => 2
        };
        for (int i = 0; i < clutter.Length; i++) clutter[i].SetActive(i < visibleCount);

        if (noteRenderer != null)
        {
            Color noteColor = day >= 9 ? new Color(0.64f, 0.78f, 0.69f) : new Color(0.68f, 0.62f, 0.44f);
            noteRenderer.material.color = noteColor;
        }
    }

    private static GameObject CreateProp(string propName, Vector3 position, Vector3 scale, Color color)
    {
        GameObject prop = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prop.name = propName;
        prop.transform.position = position;
        prop.transform.localScale = scale;
        prop.GetComponent<Renderer>().material.color = color;
        return prop;
    }
}
