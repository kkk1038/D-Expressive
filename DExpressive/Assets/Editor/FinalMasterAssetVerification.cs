using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class FinalMasterAssetVerification
{
    private const string Key = "FinalMasterAssetVerification.Running";
    private static readonly StringBuilder Report = new();
    private static readonly Stack<IEnumerator> Routines = new();
    private static double started;
    private static int frames;
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private static DayManager Day => Object.FindFirstObjectByType<DayManager>();
    private static DialogueManager Dialogue => Object.FindFirstObjectByType<DialogueManager>();
    private static GameStateData State => GameStateManager.Instance.CurrentState;
    static FinalMasterAssetVerification() => EditorApplication.update += Tick;

    public static void Begin()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SessionState.SetBool(Key, true);
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || ++frames < 100) return;
        try
        {
            EditorApplication.isPaused = false;
            EditorApplication.QueuePlayerLoopUpdate();
            if (started == 0) { started = EditorApplication.timeSinceStartup; Routines.Push(Checks()); }
            if (EditorApplication.timeSinceStartup - started > 300) throw new Exception("Master integration verification timeout");
            var player = Object.FindFirstObjectByType<FirstPersonPlayerController>();
            if (player != null) player.enabled = false;
            var choiceUI = Object.FindFirstObjectByType<ChoiceUI>();
            if (choiceUI != null) choiceUI.enabled = false;
            while (Routines.Count > 0)
            {
                var routine = Routines.Peek();
                if (!routine.MoveNext()) { Routines.Pop(); continue; }
                if (routine.Current is IEnumerator child) { Routines.Push(child); continue; }
                return;
            }
            Finish(0, "FINAL_MASTER_ASSETS_PLAYMODE_OK");
        }
        catch (Exception e) { Report.AppendLine("FAIL: " + e); Debug.LogException(e); Finish(1, "FINAL_MASTER_ASSETS_FAILED"); }
    }

    private static void Finish(int code, string marker)
    {
        SessionState.SetBool(Key, false);
        File.WriteAllText("FinalMasterAssetValidation.txt", Report.ToString());
        Debug.Log(marker);
        EditorApplication.Exit(code);
    }

    private static void Require(bool value, string message)
    {
        if (!value) throw new Exception(message);
        Report.AppendLine("PASS: " + message);
    }
    private static object Invoke(object target, string method, params object[] args) => target.GetType().GetMethod(method, Private).Invoke(target, args);
    private static IEnumerator Wait(float seconds) { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
    private static GameObject Find(string name) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(t => t.name == name)?.gameObject;
    private static void Teleport(Vector3 p) { var cc = Object.FindFirstObjectByType<CharacterController>(); cc.enabled = false; cc.transform.position = p; cc.enabled = true; Physics.SyncTransforms(); }

    private static IEnumerator Act(PrototypeInteractionTarget.InteractionType type)
    {
        Dialogue.EndDialogue();
        PrototypeInteractionController.Interact(type);
        float until = Time.realtimeSinceStartup + 4;
        var ui = Object.FindFirstObjectByType<ChoiceUI>();
        while (!ui.IsShowing && Time.realtimeSinceStartup < until) yield return null;
        Require(ui.IsShowing, "Master dialogue opens: day " + Day.CurrentDay + " / " + type);
        Invoke(Dialogue, "SelectDialogueChoice", 0);
        Dialogue.EndDialogue();
        yield return Wait(0.6f);
        Require((Day.CurrentDay == 10 && type == PrototypeInteractionTarget.InteractionType.Door) || Day.IsInteractionCompleted(type), "Master checklist/repeatable ending interaction: day " + Day.CurrentDay + " / " + type);
    }

    private static IEnumerator Checks()
    {
        Application.runInBackground = true;
        Require(Object.FindObjectsByType<PrototypeRoomBootstrap>(FindObjectsSortMode.None).Length == 1, "One master bootstrap");
        Require(GameObject.Find("Interior Kit Architecture") && GameObject.Find("Interior Kit Furniture"), "Imported architecture and furniture active");
        Require(!GameObject.Find("Interior Test Player"), "No independent test player in main game");
        Require(Object.FindFirstObjectByType<WakeUpTransitionController>() != null, "Master wake-up controller retained");
        Require(Object.FindFirstObjectByType<MentalStateBgmController>() != null, "Master mental-state BGM controller retained");
        Require(Object.FindFirstObjectByType<InteractionSoundController>() != null, "Master interaction sounds retained");
        foreach (var clip in new[] { "Audio/BGM/Melancholic_Room_Normal", "Audio/BGM/Melancholic_Room_Mental_50", "Audio/BGM/Melancholic_Room_Mental_10", "Audio/BGM/Peaceful_Room_Normal", "Audio/SFX/Phone_Vibration_Notification", "Audio/SFX/Meal_Cutlery_In_Ceramic", "Audio/SFX/Shower_Real_Water", "Audio/SFX/Door_Key_In_Lock" })
            Require(Resources.Load<AudioClip>(clip) != null, "Master audio resource loads: " + clip);
        foreach (string name in new[] { "Meal - Interact", "Phone - Click to Read", "Trash Bin - Tidy", "Refrigerator", "Bathroom Door", "Interior Kit Office Chair" })
            Require(GameObject.Find(name) != null, "Integrated object: " + name);
        foreach (var type in Enum.GetValues(typeof(PrototypeInteractionTarget.InteractionType)).Cast<PrototypeInteractionTarget.InteractionType>())
            Require(Object.FindObjectsByType<PrototypeInteractionTarget>(FindObjectsSortMode.None).Any(t => t.Type == type), "Interaction retained: " + type);
        Require(GameObject.Find("Refrigerator").GetComponentsInChildren<PrototypeInteractionTarget>().Length == 0, "Refrigerator has no meal interaction");
        Require(Object.FindObjectsByType<PrototypeInteractionTarget>(FindObjectsSortMode.None).Count(t => t.Type == PrototypeInteractionTarget.InteractionType.Meal) == 1, "Only food supplies meal interaction");
        Require(Vector3.Distance(GameObject.Find("Trash Bin - Tidy").transform.position, new Vector3(2.4f, 0, -1.05f)) < 0.01f, "Requested entrance trash-bin placement");
        Require(Mathf.Abs(Mathf.DeltaAngle(GameObject.Find("Interior Kit Office Chair").transform.eulerAngles.y, 0)) < 0.1f, "Requested chair rotation");
        Require(Vector3.Dot(-GameObject.Find("Computer Screen").transform.right, Vector3.back) > 0.99f, "Monitor faces the chair after 90 degree turn");
        foreach (var renderer in GameObject.Find("Additional Room Props").GetComponentsInChildren<Renderer>())
            foreach (var material in renderer.sharedMaterials) Require(material != null && material.shader.name == "Universal Render Pipeline/Lit", "URP material: " + renderer.name);
        foreach (string name in new[] { "Curtain Left", "Computer Monitor", "Daily Note" })
            Require(GameObject.Find(name).GetComponent<Renderer>().sharedMaterial.shader.name == "Universal Render Pipeline/Lit", "Runtime primitive has explicit URP material: " + name);
        var tub = GameObject.Find("Bathroom Bathtub - Wash");
        var tubBounds = tub.GetComponentsInChildren<Renderer>().Select(r => r.bounds).Aggregate((a,b) => { a.Encapsulate(b); return a; });
        Require(tubBounds.size.x > 1.9f && tubBounds.size.x < 2.1f && tubBounds.size.z > .9f, "Bathtub enlarged to approximately 2m by 0.94m");
        Require(Mathf.Abs(Mathf.DeltaAngle(tub.transform.eulerAngles.y,270)) < .1f, "Bathtub faucet orientation retained");
        Require(tubBounds.min.x > -4.9f && tubBounds.max.x < -2.7f && tubBounds.min.z > -1.3f, "Enlarged bathtub fits bathroom wall bounds");
        Capture("Bathtub-Final-v0.2.2.png", new Vector3(-2.65f,1.7f,.35f),new Vector3(-3.82f,.45f,-.72f),70);
        var ui = Object.FindFirstObjectByType<PrototypeGameUI>();
        Invoke(ui, "StartNewGame"); ui.enabled = false; Dialogue.EndDialogue();
        yield return Wait(1.2f);
        Require(Day.CurrentDay == 1 && !Day.HasCompletedMainGoal(), "New master game starts with incomplete main objective");
        var cc = Object.FindFirstObjectByType<CharacterController>();
        Vector3 p = FirstPersonPlayerBootstrap.WakeUpPosition;
        var overlaps = Physics.OverlapCapsule(p + Vector3.up * 0.34f, p + Vector3.up * 1.46f, 0.30f, ~0, QueryTriggerInteraction.Ignore).Where(c => c != cc && c.enabled).ToArray();
        Require(overlaps.Length == 0, "Wake-up position clear of imported furniture: " + string.Join(",", overlaps.Select(c => c.name)));
        Capture("FinalMaster-Room.png", new Vector3(-0.6f, 1.7f, 1.3f), new Vector3(0.8f, 1.2f, 5.3f));
        Day.GoToSleep(); yield return Wait(0.5f);
        Require(Day.CurrentDay == 1, "Master main-goal sleep gate retained");
        yield return VerifyDoor();

        // Exercise the final code's real ordered dialogue and delayed interactions, not the older day loop.
        for (int day = 1; day <= 10; day++)
        {
            State.mentalState = 100;
            Day.BeginDay(day, true, false); Dialogue.EndDialogue();
            yield return Wait(1.2f);
            Require(Day.CurrentDay == day, "Master day begins: " + day);
            foreach (var type in Day.GetMainGoalInteractions()) yield return Act(type);
            Require(Day.HasCompletedMainGoal(), "Master main goals complete: day " + day);
            if (!Day.IsInteractionCompleted(PrototypeInteractionTarget.InteractionType.Meal)) yield return Act(PrototypeInteractionTarget.InteractionType.Meal);
            if (day == 3) Require(State.choiceHistory.Any(r => r.choiceId.StartsWith("day3_meal_")), "Master ordered Day 3 food dialogue retained");
            if (day != 3 || State.choiceHistory.Any(r => r.choiceId.StartsWith("day3_meal_") && !r.selectedOptionId.EndsWith("_third")))
                Require(!Find("Meal - Interact").activeSelf, "Food visibility follows master meal choice: day " + day);
            if (day == 2)
            {
                yield return Act(PrototypeInteractionTarget.InteractionType.Tidy);
                Require(Find("Trash Bin Contents").activeSelf && Find("Trash Bin - Tidy").activeSelf, "Master Day 2 defer-cleaning choice keeps contents");
                SaveRoundTrip();
                Require(!Find("Meal - Interact").activeSelf && Find("Trash Bin Contents").activeSelf, "Load restores food and deferred-cleaning visuals");
            }
            if (day == 4)
            {
                yield return Act(PrototypeInteractionTarget.InteractionType.Tidy);
                Require(!Find("Trash Bin Contents").activeSelf && Find("Trash Bin - Tidy").activeSelf, "Positive cleaning choice removes contents but keeps bin");
                SaveRoundTrip();
                Require(!Find("Meal - Interact").activeSelf && !Find("Trash Bin Contents").activeSelf, "Load restores consumed food and cleaned bin");
            }
            if (day == 7)
            {
                Require(State.storyFlags.Contains("day7_contact_recovery"), "Master Day 7 contact recovery flag retained");
                int before = State.mentalState;
                Object.FindFirstObjectByType<MentalStateManager>().ChangeMentalState(-5);
                Require(State.mentalState == before, "Master contact-recovery flag prevents further mental loss");
            }
            if (day == 9)
            {
                yield return Wait(1);
                Require(Mathf.Abs(Mathf.DeltaAngle(GameObject.Find("Entrance Door Hinge").transform.localEulerAngles.y, 0)) < 1, "Final master Day 9 wait choice leaves entrance closed");
            }
            if (day < 10)
            {
                // Complete the checklist to avoid testing a deliberately depleted branch here.
                foreach (var type in new[] { PrototypeInteractionTarget.InteractionType.Phone, PrototypeInteractionTarget.InteractionType.Meal, PrototypeInteractionTarget.InteractionType.Medicine, PrototypeInteractionTarget.InteractionType.Shower, PrototypeInteractionTarget.InteractionType.Tidy, PrototypeInteractionTarget.InteractionType.Mirror, PrototypeInteractionTarget.InteractionType.Window, PrototypeInteractionTarget.InteractionType.Door })
                    Day.RegisterInteraction(type);
                State.mentalState = 100;
                Day.GoToSleep();
                float until = Time.realtimeSinceStartup + 8;
                while (Day.CurrentDay == day && Time.realtimeSinceStartup < until) yield return null;
                Require(Day.CurrentDay == day + 1, "Master sleep/collapse advances day: " + day);
                Dialogue.EndDialogue();
                yield return Wait(2);
                Require(Find("Meal - Interact").activeSelf && Find("Trash Bin Contents").activeSelf, "Next day restores daily props: " + (day + 1));
            }
        }
        // The final door can be repeated in the master code; preserve its ending choices.
        yield return Act(PrototypeInteractionTarget.InteractionType.Door);
        yield return Wait(2.5f);
        var ending = Find("Ending Screen");
        Require(ending != null && ending.activeSelf, "Master Day 10 ending displayed");
        Day.BeginDay(3, true, false); Dialogue.EndDialogue();
        State.storyFlags.Remove("day7_contact_recovery"); // Simulate the pre-recovery branch for depletion testing.
        State.mentalState = 1;
        Object.FindFirstObjectByType<MentalStateManager>().ChangeMentalState(-5);
        Require(Day.CurrentDay == 2, "Master mental-depletion rollback preserved");
    }

    private static void SaveRoundTrip()
    {
        string path = (string)typeof(PrototypeSaveManager).GetProperty("SavePath", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        byte[] previous = File.Exists(path) ? File.ReadAllBytes(path) : null;
        try
        {
            string expected = JsonUtility.ToJson(State);
            PrototypeSaveManager.Save(); State.mentalState = 2;
            Require(PrototypeSaveManager.TryLoad() && JsonUtility.ToJson(State) == expected, "Master state and checkpoints save/load round trip");
            Day.ResumeLoadedDay(); Dialogue.EndDialogue();
        }
        finally { if (previous != null) File.WriteAllBytes(path, previous); else if (File.Exists(path)) File.Delete(path); }
    }

    private static IEnumerator VerifyDoor()
    {
        var door = Object.FindFirstObjectByType<BathroomDoorController>();
        var target = door.GetComponent<PrototypeInteractionTarget>();
        var cc = Object.FindFirstObjectByType<CharacterController>();
        Vector3 previous = cc.transform.position;
        int choices = State.choiceHistory.Count, actions = State.completedInteractions.Count;
        Require(door.IsClosed && target.LocalPrompt == "화장실 문 열기", "Bathroom starts closed with local prompt");
        Capture("FinalMaster-Bathroom-Closed.png", new Vector3(0.15f, 1.55f, 0.4f), new Vector3(-1.65f, 1.1f, 0.43f));
        Teleport(new Vector3(-0.8f, 0.08f, 0.32f));
        for (int i = 0; i < 40; i++) cc.Move(Vector3.left * 0.05f);
        Require(cc.transform.position.x > -1.4f, "Closed bathroom door blocks entry");
        Require(target.TryInteractLocally(), "Bathroom local interaction bypasses story Door type");
        yield return Wait(1.2f);
        Require(door.IsOpen && target.LocalPrompt == "화장실 문 닫기", "Bathroom opens and prompt changes");
        Capture("FinalMaster-Bathroom-Open.png", new Vector3(0.15f, 1.55f, 0.4f), new Vector3(-1.65f, 1.1f, 0.43f));
        Teleport(new Vector3(-0.8f, 0.08f, 0.32f));
        for (int i = 0; i < 40; i++) cc.Move(Vector3.left * 0.06f);
        Require(cc.transform.position.x < -2.8f, "Open bathroom door allows entry");
        target.TryInteractLocally(); yield return Wait(1.2f);
        Require(door.IsClosed, "Bathroom closes from inside");
        Teleport(new Vector3(-2.2f, 0.08f, 0.32f));
        for (int i = 0; i < 30; i++) cc.Move(Vector3.right * 0.06f);
        Require(cc.transform.position.x < -1.9f, "Closed bathroom door blocks exit");
        Teleport(new Vector3(-3.2f, 0.08f, 0.32f)); target.TryInteractLocally(); yield return Wait(1.2f);
        Teleport(new Vector3(-1.65f, 0.08f, 0.32f)); target.TryInteractLocally(); yield return Wait(1.2f);
        Require(!door.IsClosed, "Bathroom anti-pinch stops for player");
        Teleport(new Vector3(-0.8f, 0.08f, 0.32f)); yield return Wait(1.2f);
        Require(door.IsClosed, "Bathroom finishes closing when clear");
        Require(choices == State.choiceHistory.Count && actions == State.completedInteractions.Count, "Bathroom does not change master checklist or choices");
        Teleport(previous);
    }

    private static void Capture(string path, Vector3 position, Vector3 lookAt, float fov = 75)
    {
        var camera = Camera.main; var pose = new Pose(camera.transform.position, camera.transform.rotation);
        float originalFov = camera.fieldOfView; camera.fieldOfView = fov; camera.transform.position = position; camera.transform.LookAt(lookAt);
        var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32); rt.Create();
        var request = new RenderPipeline.StandardRequest { destination = rt };
        for (int i = 0; i < 3; i++) RenderPipeline.SubmitRenderRequest(camera, request);
        var previous = RenderTexture.active; RenderTexture.active = rt;
        var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply(); File.WriteAllBytes(path, texture.EncodeToPNG());
        RenderTexture.active = previous; rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(texture);
        camera.transform.SetPositionAndRotation(pose.position, pose.rotation); camera.fieldOfView = originalFov;
    }
}
