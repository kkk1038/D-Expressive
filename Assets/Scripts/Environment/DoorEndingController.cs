using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>Turns the late-story door choices into a physical door opening and a complete ending.</summary>
public class DoorEndingController : MonoBehaviour
{
    private ChoiceManager choiceManager;
    private Transform doorHinge;
    private CanvasGroup endingCanvas;
    private bool opened;

    private void Start()
    {
        choiceManager = FindFirstObjectByType<ChoiceManager>();
        if (choiceManager != null) choiceManager.OnChoiceSelected += OnChoiceSelected;
        doorHinge = GameObject.Find("Entrance Door Hinge")?.transform;
        CreateEndingUI();
    }

    private void OnDestroy()
    {
        if (choiceManager != null) choiceManager.OnChoiceSelected -= OnChoiceSelected;
    }

    private void OnChoiceSelected(string choiceId, string optionId, int mentalChange)
    {
        if (optionId == "day9_open_door") StartCoroutine(OpenDoor(52f));
        if (optionId == "day10_step_out") StartCoroutine(PlayEnding());
    }

    private IEnumerator OpenDoor(float targetAngle)
    {
        if (doorHinge == null) yield break;
        if (opened && Mathf.Abs(doorHinge.localEulerAngles.y - targetAngle) < 0.5f) yield break;
        opened = true;
        Quaternion from = doorHinge.localRotation;
        Quaternion to = Quaternion.Euler(0f, targetAngle, 0f);
        for (float t = 0f; t < 0.75f; t += Time.deltaTime)
        {
            doorHinge.localRotation = Quaternion.Slerp(from, to, t / 0.75f);
            yield return null;
        }
        doorHinge.localRotation = to;
    }

    private IEnumerator PlayEnding()
    {
        yield return OpenDoor(90f);
        if (endingCanvas == null) yield break;
        endingCanvas.gameObject.SetActive(true);
        endingCanvas.alpha = 0f;
        for (float t = 0f; t < 1.3f; t += Time.deltaTime)
        {
            endingCanvas.alpha = t / 1.3f;
            yield return null;
        }
        endingCanvas.alpha = 1f;
        FirstPersonPlayerController.UnlockCursor();
    }

    private void CreateEndingUI()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        GameObject panel = new GameObject("Ending Screen", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.025f, 0.04f, 0.08f, 0.98f);
        endingCanvas = panel.GetComponent<CanvasGroup>();
        panel.SetActive(false);

        TMP_Text title = CreateText(panel.transform, new Vector2(0f, 75f), new Vector2(900f, 170f), 46);
        title.text = "D-EXPRESSIVE\n\n끝이 아니라, 다음 걸음.";
        TMP_Text message = CreateText(panel.transform, new Vector2(0f, -90f), new Vector2(760f, 80f), 21);
        message.text = "당신이 고른 작은 행동들이\n오늘의 문을 열었습니다.";
        CreateRestartButton(panel.transform);
    }

    private static TMP_Text CreateText(Transform parent, Vector2 position, Vector2 size, float fontSize)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position; rect.sizeDelta = size;
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.alignment = TextAlignmentOptions.Center; text.fontSize = fontSize; text.color = Color.white;
        return text;
    }

    private static void CreateRestartButton(Transform parent)
    {
        GameObject buttonObject = new GameObject("처음으로", typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -215f);
        rect.sizeDelta = new Vector2(240f, 52f);
        buttonObject.GetComponent<Image>().color = new Color(0.18f, 0.34f, 0.55f, 1f);
        buttonObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        });
        TMP_Text text = CreateText(buttonObject.transform, Vector2.zero, rect.sizeDelta, 21f);
        text.text = "처음으로";
    }
}
