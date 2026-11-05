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
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class DeskPropsVerification
{
    private const string Key = "DeskPropsVerification.Active";
    private const string ReviewKey = "DeskPropsVerification.Review";
    private static readonly Stack<IEnumerator> Routines = new();
    private static readonly StringBuilder Report = new();
    private static int frames;
    private static double started;
    private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    static DeskPropsVerification() => EditorApplication.update += Tick;
    public static void Begin() { DeskPropsSetup.Apply(); Start(false); }
    public static void Review()
    {
        SessionState.SetBool("DeskPropsVerification.Bathtub", false);
        Start(true);
    }
    public static void ReviewBathtub()
    {
        SessionState.SetBool("DeskPropsVerification.Bathtub", true);
        Start(true);
    }
    private static void Start(bool review)
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SessionState.SetBool(Key, true); SessionState.SetBool(ReviewKey, review); EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || ++frames < 100) return;
        try
        {
            EditorApplication.isPaused = false; EditorApplication.QueuePlayerLoopUpdate();
            if (started == 0) { started = EditorApplication.timeSinceStartup; Routines.Push(SessionState.GetBool(ReviewKey, false) ? ShowDesk() : Checks()); }
            if (EditorApplication.timeSinceStartup - started > 90) throw new Exception("Desk props verification timeout");
            while (Routines.Count > 0)
            {
                var routine = Routines.Peek();
                if (!routine.MoveNext()) { Routines.Pop(); continue; }
                if (routine.Current is IEnumerator child) { Routines.Push(child); continue; }
                return;
            }
            SessionState.SetBool(Key, false);
            if (!SessionState.GetBool(ReviewKey, false)) { File.WriteAllText("DeskPropsValidation.txt", Report.ToString()); Debug.Log("DESK_PROPS_PLAYMODE_OK"); EditorApplication.Exit(0); }
        }
        catch (Exception e) { SessionState.SetBool(Key, false); File.WriteAllText("DeskPropsValidation.txt", Report + "FAIL: " + e); Debug.LogException(e); if (Application.isBatchMode) EditorApplication.Exit(1); }
    }
    private static object Invoke(object target, string method, params object[] args) => target.GetType().GetMethod(method, Private).Invoke(target, args);
    private static void Require(bool ok, string message) { if (!ok) throw new Exception(message); Report.AppendLine("PASS: " + message); }
    private static IEnumerator Wait(float seconds) { float until = Time.realtimeSinceStartup + seconds; while (Time.realtimeSinceStartup < until) yield return null; }
    private static GameObject Find(string name) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).First(t => t.name == name && t.gameObject.scene.IsValid()).gameObject;
    private static void End() => Object.FindFirstObjectByType<DialogueManager>().EndDialogue();
    private static void StartGame()
    {
        Invoke(Object.FindFirstObjectByType<PrototypeGameUI>(), "StartNewGame"); End();
    }
    private static IEnumerator Act(PrototypeInteractionTarget.InteractionType type, int choice)
    {
        End(); PrototypeInteractionController.Interact(type);
        float until = Time.realtimeSinceStartup + 4;
        while (!Object.FindFirstObjectByType<ChoiceUI>().IsShowing && Time.realtimeSinceStartup < until) yield return null;
        Require(Object.FindFirstObjectByType<ChoiceUI>().IsShowing, "Existing dialogue opens: " + type);
        Invoke(Object.FindFirstObjectByType<DialogueManager>(), "SelectDialogueChoice", choice); End(); yield return Wait(0.6f);
    }
    private static IEnumerator Checks()
    {
        StartGame();
        var player = Object.FindFirstObjectByType<FirstPersonPlayerController>(); player.enabled = false;
        Object.FindFirstObjectByType<ChoiceUI>().enabled = false;
        Object.FindFirstObjectByType<PrototypeGameUI>().enabled = false;
        yield return Wait(1.2f);
        var papers = Find("Cup - Click to Tidy"); var medicine = Find("Medicine - Interact");
        Require(papers.GetComponentInChildren<MeshFilter>() != null, "Paper cluster replaces original cup");
        Require(medicine.GetComponentsInChildren<MeshFilter>().Length == 4, "Medicine bag and three dose packets replace block");
        Require(papers.GetComponent<PrototypeInteractionTarget>().Type == PrototypeInteractionTarget.InteractionType.Tidy, "Papers retain Tidy interaction");
        Require(medicine.GetComponent<PrototypeInteractionTarget>().Type == PrototypeInteractionTarget.InteractionType.Medicine, "Bag retains Medicine interaction");
        foreach (var obj in new[] { papers, medicine })
        {
            Require(obj.GetComponent<BoxCollider>() != null, "Interaction collider: " + obj.name);
            var bounds = obj.GetComponent<BoxCollider>().bounds;
            Require(bounds.min.y >= 1.15f && bounds.max.y < 1.35f, "Prop rests on tabletop: " + obj.name);
            var ray = new Ray(bounds.center + Vector3.up * 0.6f, Vector3.down);
            var target = (PrototypeInteractionTarget)Invoke(player, "FindInteractionTarget", ray);
            Require(target && target.gameObject == obj, "First-person F ray selects: " + obj.name);
            foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                foreach (var material in renderer.sharedMaterials) Require(material && material.shader.name == "Universal Render Pipeline/Lit", "URP material: " + renderer.name);
        }
        Require(papers.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture != null, "Paper atlas texture retained");
        Require(medicine.GetComponentsInChildren<Renderer>().SelectMany(r => r.sharedMaterials).Count(m => m.mainTexture != null) >= 4, "Pharmacy printing and dose labels retained");
        Capture("DeskProps-Desk.png", new Vector3(2.35f, 2.5f, 3.6f), new Vector3(2.35f, 1.17f, 5.12f), 55);
        Capture("DeskProps-Papers.png", new Vector3(1.3f, 1.95f, 4.45f), new Vector3(1.3f, 1.17f, 5.0f), 48);
        Capture("DeskProps-Medicine.png", new Vector3(3.4f, 1.88f, 4.48f), new Vector3(3.4f, 1.17f, 5.0f), 42);
        var day = Object.FindFirstObjectByType<DayManager>();
        day.BeginDay(4, true, false); End(); yield return Wait(1.2f);
        yield return Act(PrototypeInteractionTarget.InteractionType.Tidy, 0);
        Require(!papers.activeSelf && !Find("Trash Bin Contents").activeSelf, "Existing clean choice removes papers and bin contents together");
        day.BeginDay(5, true, false); End(); yield return Wait(1.2f);
        Require(papers.activeSelf && medicine.activeSelf, "Next day restores papers and medicine");
        yield return Act(PrototypeInteractionTarget.InteractionType.Medicine, 0);
        Require(!medicine.activeSelf, "Existing take-medicine choice hides bag");
        string path = (string)typeof(PrototypeSaveManager).GetProperty("SavePath", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        byte[] previous = File.Exists(path) ? File.ReadAllBytes(path) : null;
        try
        {
            PrototypeSaveManager.Save();
            day.BeginDay(6, false, false); End();
            Require(medicine.activeSelf, "Medicine visible on a fresh day");
            Require(PrototypeSaveManager.TryLoad(), "Existing saved state loads");
            day.ResumeLoadedDay(); End();
            Require(!medicine.activeSelf && papers.activeSelf, "Load restores medicine consumption without hiding papers");
        }
        finally { if (previous != null) File.WriteAllBytes(path, previous); else if (File.Exists(path)) File.Delete(path); }
        day.BeginDay(6, true, false); End(); yield return Wait(1.2f);
        yield return Act(PrototypeInteractionTarget.InteractionType.Medicine, 1);
        Require(medicine.activeSelf, "Skip medicine keeps the new bag visible");
    }

    private static IEnumerator ShowDesk()
    {
        StartGame(); yield return Wait(1.2f);
        End();
        var player = Object.FindFirstObjectByType<FirstPersonPlayerController>();
        var cc = player.GetComponent<CharacterController>(); cc.enabled = false;
        bool bathroom = SessionState.GetBool("DeskPropsVerification.Bathtub", false);
        player.transform.position = bathroom ? new Vector3(-2.65f, 0.05f, 0.35f) : new Vector3(2.1f, 0.05f, 2.9f);
        player.transform.rotation = Quaternion.Euler(0, bathroom ? -130 : 7, 0); cc.enabled = true;
        float pitch = bathroom ? 32 : 13;
        typeof(FirstPersonPlayerController).GetField("pitch", Private).SetValue(player, pitch);
        Camera.main.transform.localRotation = Quaternion.Euler(pitch, 0, 0); Camera.main.fieldOfView = bathroom ? 70 : 62;
        if (bathroom) Capture("Bathtub-180.png", new Vector3(-2.5f, 1.7f, 0.55f), new Vector3(-3.95f, 0.55f, -0.8f), 70);
        FirstPersonPlayerController.UnlockCursor();
        var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        var view = EditorWindow.GetWindow(type); view.maximized = true; view.Focus();
    }

    private static void Capture(string path, Vector3 position, Vector3 lookAt, float fov)
    {
        typeof(FinalMasterAssetVerification).GetMethod("Capture", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[] { path, position, lookAt, fov });
    }
}
