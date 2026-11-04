using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class InteriorKitGameVerification
{
    private const string Key = "InteriorKitGameVerification.Running";
    private static int frames;
    private static IEnumerator checks;
    private static double started;
    private static readonly System.Text.StringBuilder Report = new();
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static InteriorKitGameVerification() => EditorApplication.update += Tick;
    public static void Begin()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SessionState.SetBool(Key, true);
        EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || ++frames < 90) return;
        try
        {
            if (EditorApplication.isPaused)
            {
                Debug.Log("Verification resumes editor paused by startup indexing exception.");
                EditorApplication.isPaused = false;
            }
            EditorApplication.QueuePlayerLoopUpdate();
            if (started == 0) started = EditorApplication.timeSinceStartup;
            if (EditorApplication.timeSinceStartup - started > 120) throw new Exception("Verification timeout: game time=" + Time.time + ", scale=" + Time.timeScale);
            if (checks == null) checks = Checks();
            if (checks.MoveNext()) return;
            SessionState.SetBool(Key, false);
            File.WriteAllText("InteriorKitGameValidation.txt", Report.ToString());
            Debug.Log("INTERIOR_GAME_PLAYMODE_OK"); EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            SessionState.SetBool(Key, false);
            File.WriteAllText("InteriorKitGameValidation.txt", Report + "FAIL: " + e);
            Debug.LogException(e); EditorApplication.Exit(1);
        }
    }
    private static void Require(bool success, string message)
    {
        if (!success) throw new Exception(message);
        Report.AppendLine("PASS: " + message);
    }
    private static void Invoke(object target, string method, params object[] args) => target.GetType().GetMethod(method, Private).Invoke(target, args);
    private static void EndDialogue() => Object.FindFirstObjectByType<DialogueManager>().EndDialogue();
    private static void Choose(PrototypeInteractionTarget.InteractionType type)
    {
        PrototypeInteractionController.Interact(type);
        var dm = Object.FindFirstObjectByType<DialogueManager>();
        Require(Object.FindFirstObjectByType<ChoiceUI>().IsShowing, "Dialogue opens for " + type);
        Invoke(dm, "SelectDialogueChoice", 0);
        dm.EndDialogue();
    }
    private static IEnumerator Checks()
    {
        Require(Object.FindObjectsByType<PrototypeRoomBootstrap>(FindObjectsSortMode.None).Length == 1, "Single existing room bootstrap");
        Require(GameObject.Find("Interior Kit Architecture") && GameObject.Find("Interior Kit Furniture"), "New architecture and furniture created");
        Require(!GameObject.Find("Interior Test Player"), "No test-scene player or test controls");
        foreach (string propName in new[] { "Curtain Left", "Computer Monitor", "Daily Note" })
        {
            var prop = GameObject.Find(propName);
            Require(prop != null && prop.GetComponent<Renderer>().sharedMaterial.shader.name == "Universal Render Pipeline/Lit", "Runtime primitive uses packaged URP material: " + propName);
        }
        var ui = Object.FindFirstObjectByType<PrototypeGameUI>();
        Invoke(ui, "StartNewGame"); EndDialogue();
        // The user can keep using other apps during this background check. Ignore physical key presses.
        ui.enabled = false;
        Object.FindFirstObjectByType<ChoiceUI>().enabled = false;
        Object.FindFirstObjectByType<FirstPersonPlayerController>().enabled = false;
        Application.runInBackground = true;
        yield return null;
        var day = Object.FindFirstObjectByType<DayManager>();
        Require(day.CurrentDay == 1, "Original new-game menu starts Day 1");
        foreach (var type in Enum.GetValues(typeof(PrototypeInteractionTarget.InteractionType)).Cast<PrototypeInteractionTarget.InteractionType>())
            Require(Object.FindObjectsByType<PrototypeInteractionTarget>(FindObjectsSortMode.None).Any(t => t.Type == type), "World interaction retained: " + type);
        string[] untouched = { "Phone - Click to Read", "Meal - Interact", "Medicine - Interact", "Mirror - Click to Look", "Sink - Click to Wash", "Cup - Click to Tidy", "Laundry Basket", "Folded Clothes", "Curtain Left", "Curtain Right", "Curtain Rail", "Wall Notes", "Floor Clothes", "Shoe Cabinet", "Shoes", "Door Mat", "Bathroom Tile Floor", "Bathroom Door Frame", "Bathroom Shelf", "Wardrobe Body", "Wardrobe Door Left", "Wardrobe Door Right", "Wardrobe Handle", "Computer Monitor", "Computer Screen", "Keyboard", "Plant Pot", "Desk Lamp Stand", "Desk Lamp Shade" };
        string original = File.ReadAllText("Backups/PrototypeRoomBootstrap.original.cs.txt");
        bool hasAdditional=GameObject.Find("Additional Room Props")!=null;
        string[] replacedAdditional={"Phone - Click to Read","Meal - Interact","Mirror - Click to Look","Sink - Click to Wash","Bathroom Shelf","Shoes","Bathroom Door Frame","Computer Monitor","Computer Screen","Keyboard"};
        if(hasAdditional) untouched=untouched.Where(n=>!replacedAdditional.Contains(n)).ToArray();
        foreach (string name in untouched)
        {
            var obj = GameObject.Find(name);
            Require(obj, "Unmatched object preserved: " + name);
            var match = System.Text.RegularExpressions.Regex.Match(original, "CreateBlock\\([^,]+, \"" + System.Text.RegularExpressions.Regex.Escape(name) + "\", new Vector3\\(([^)]+)\\)");
            if (!match.Success) throw new Exception("Original prop coordinates missing: " + name);
            var values = match.Groups[1].Value.Split(',').Select(s => float.Parse(s.Trim().TrimEnd('f'), System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            Require(Vector3.Distance(obj.transform.position, new Vector3(values[0], values[1], values[2])) < 0.001f, "Original coordinates retained: " + name);
        }
        Require(Vector3.Distance(GameObject.Find("Medicine - Interact").transform.position, new Vector3(3.25f, 1.26f, 5.5f)) < 0.001f, "Medicine remains at original coordinates");
        Physics.SyncTransforms();
        Require(Physics.Raycast(new Vector3(0, 1, -0.8f), Vector3.down, 2), "Original spawn supported by new floor");
        var movement = Object.FindFirstObjectByType<CharacterController>();
        var spawn = movement.transform.position;
        movement.enabled = false; movement.transform.position = new Vector3(-4, 0.08f, 3); movement.enabled = true;
        for (int i = 0; i < 50; i++) movement.Move(Vector3.left * 0.05f);
        Require(movement.transform.position.x > -4.8f, "New wall blocks existing player capsule");
        movement.enabled = false; movement.transform.position = spawn; movement.enabled = true;
        Require(Physics.Raycast(new Vector3(1.75f, 3, 5.5f), Vector3.down, out var hit, 3) && hit.collider.GetComponentInParent<PrototypeInteractionTarget>()?.Type == PrototypeInteractionTarget.InteractionType.Meal, "Meal raycast reaches preserved object above new desk");
        if(hasAdditional)
        {
            var doorChecks=VerifyBathroomDoor();
            while(doorChecks.MoveNext())yield return doorChecks.Current;
            VerifyAdditionalProps();
        }
        foreach (var type in new[] { PrototypeInteractionTarget.InteractionType.Phone, PrototypeInteractionTarget.InteractionType.Window, PrototypeInteractionTarget.InteractionType.Mirror, PrototypeInteractionTarget.InteractionType.Shower, PrototypeInteractionTarget.InteractionType.Door }) Choose(type);
        GameStateManager.Instance.ChangeMentalState(-30);
        int before = GameStateManager.Instance.CurrentState.mentalState;
        Choose(PrototypeInteractionTarget.InteractionType.Meal);
        Require(GameStateManager.Instance.CurrentState.mentalState > before, "Meal choice changes original mental state");
        Require(!GameObject.Find("Meal - Interact"), "Consumed meal hidden by original visual system");
        Choose(PrototypeInteractionTarget.InteractionType.Medicine);
        Choose(PrototypeInteractionTarget.InteractionType.Tidy);
        if(hasAdditional)Require(!GameObject.Find("Trash Bin Contents"),"Tidy choice empties waste bin contents");
        Capture("IntegratedGame-Day1.png");
        PrototypeInteractionController.Interact(PrototypeInteractionTarget.InteractionType.Sleep);
        EndDialogue(); yield return null;
        Require(day.CurrentDay == 2, "New bed keeps original sleep/day progression");
        Require(GameObject.Find("Meal - Interact") && GameObject.Find("Medicine - Interact") && GameObject.Find("Cup - Click to Tidy"), "Daily consumables restored on Day 2");
        if(hasAdditional)Require(GameObject.Find("Meal - Interact").GetComponentsInChildren<MeshFilter>().Any(m=>AssetDatabase.GetAssetPath(m.sharedMesh).Contains("AdditionalProps")),"Next day restores new bento rather than legacy cube");
        if(hasAdditional)Require(GameObject.Find("Trash Bin Contents"),"Next day restores daily waste bin contents");
        day.RegisterDailyAction(); day.GoToSleep(); EndDialogue(); yield return null;
        Require(day.CurrentDay == 3, "Day 3 starts");
        VerifySaveRoundTrip();
        Object.FindFirstObjectByType<MentalStateManager>().ChangeMentalState(-100);
        EndDialogue(); yield return null;
        Require(day.CurrentDay == 2, "Mental depletion restores previous-day checkpoint");
        // Allow the existing rollback overlay to finish before taking later screenshots.
        double wait = Time.time + 2;
        while (Time.time < wait) yield return null;
        while (day.CurrentDay < 9)
        {
            int expected = day.CurrentDay + 1;
            GameStateManager.Instance.ChangeMentalState(100); day.RegisterDailyAction(); day.GoToSleep(); EndDialogue();
            yield return null; Require(day.CurrentDay == expected, "Existing day progression reaches Day " + expected);
        }
        Choose(PrototypeInteractionTarget.InteractionType.Door);
        wait = Time.time + 1.2;
        while (Time.time < wait) yield return null;
        var hinge = GameObject.Find("Entrance Door Hinge").transform;
        Require(Mathf.Abs(Mathf.DeltaAngle(hinge.localEulerAngles.y, 52)) < 1, "Day 9 choice animates new door using original ending controller (angle=" + hinge.localEulerAngles.y + ")");
        GameStateManager.Instance.ChangeMentalState(100); day.RegisterDailyAction(); day.GoToSleep(); EndDialogue(); yield return null;
        Require(day.CurrentDay == 10, "Day 10 starts");
        Choose(PrototypeInteractionTarget.InteractionType.Door);
        wait = Time.time + 2.7;
        while (Time.time < wait) yield return null;
        Require(GameObject.Find("Ending Screen")?.GetComponent<CanvasGroup>().alpha > 0.99f, "Day 10 original ending screen displayed");
        Capture("IntegratedGame-Ending.png");
    }
    private static void VerifySaveRoundTrip()
    {
        string path = (string)typeof(PrototypeSaveManager).GetProperty("SavePath", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        byte[] previous = File.Exists(path) ? File.ReadAllBytes(path) : null;
        if (previous != null) File.WriteAllBytes("Backups/UserSave.before-validation.json", previous);
        try
        {
            string expected = JsonUtility.ToJson(GameStateManager.Instance.CurrentState);
            PrototypeSaveManager.Save();
            GameStateManager.Instance.ChangeMentalState(-1);
            Require(PrototypeSaveManager.TryLoad(), "Existing save file can be loaded");
            Require(expected == JsonUtility.ToJson(GameStateManager.Instance.CurrentState), "Saved day, mental state and choice history round-trip");
        }
        finally
        {
            if (previous != null) File.WriteAllBytes(path, previous);
            else if (File.Exists(path)) File.Delete(path);
        }
    }
    private static void VerifyAdditionalProps()
    {
        string[] names={"Phone - Click to Read","Meal - Interact","Sink - Click to Wash","Mirror - Click to Look","Bathroom Shelf","Shoes","Refrigerator","Trash Bin - Tidy","Washing Machine","Bathroom Toilet","Toilet Paper Holder","Toilet Brush","Bathroom Shower - Wash","Bathroom Bathtub - Wash","Bathroom Towel Rail","Bathroom Hand Soap","Bathroom Bath Mat"};
        foreach(string name in names)
        {
            var obj=GameObject.Find(name);Require(obj,"Additional object present: "+name);
            Require(obj.GetComponentsInChildren<MeshFilter>().Any(m=>AssetDatabase.GetAssetPath(m.sharedMesh).Contains("AdditionalProps")),"Imported mesh used: "+name);
        }
        Require(GameObject.Find("Refrigerator").GetComponentsInChildren<PrototypeInteractionTarget>().Length==0,"Refrigerator has no meal or other interaction target");
        Require(Object.FindObjectsByType<PrototypeInteractionTarget>(FindObjectsSortMode.None).Count(t=>t.Type==PrototypeInteractionTarget.InteractionType.Meal)==1,"Only the food offers meal interaction");
        Require(Vector3.Distance(GameObject.Find("Trash Bin - Tidy").transform.position,new Vector3(2.4f,0,-1.05f))<0.001f,"Waste bin moved into gap beside entrance");
        Require(Mathf.Abs(Mathf.DeltaAngle(GameObject.Find("Interior Kit Office Chair").transform.eulerAngles.y,0))<0.1f,"Chair rotated 180 degrees from previous layout");
        Require(Vector3.Dot(GameObject.Find("Computer Screen").transform.TransformDirection(Vector3.left),Vector3.back)>0.99f,"Monitor rotated 90 degrees to face the chair");
        Require(GameObject.Find("Trash Bin - Tidy").GetComponent<PrototypeInteractionTarget>().Type==PrototypeInteractionTarget.InteractionType.Tidy,"Waste bin uses original tidy dialogue");
        Require(GameObject.Find("Trash Bin - Tidy").GetComponent<Collider>().bounds.size.y>0.45f,"Actual waste basket normalized to standing height");
        Require(!GameObject.Find("Trash Bin - Tidy").GetComponentsInChildren<Renderer>().Any(r=>r.GetComponentsInParent<Transform>().Any(t=>t.name.StartsWith("Sphere"))),"Viewer background sphere excluded from waste bin");
        Require(GameObject.Find("Bathroom Shower - Wash").GetComponent<PrototypeInteractionTarget>().Type==PrototypeInteractionTarget.InteractionType.Shower,"Shower uses original daily wash dialogue");
        Require(GameObject.Find("Shoes").transform.childCount==2,"Pair of imported shoes placed");
        Require(GameObject.Find("Refrigerator").GetComponent<Collider>().bounds.size.y>1.5f,"Refrigerator has room-scale height and collision");
        var cc=Object.FindFirstObjectByType<CharacterController>();var previous=cc.transform.position;
        cc.enabled=false;cc.transform.position=new Vector3(0,0.08f,-0.8f);cc.enabled=true;
        for(int i=0;i<14;i++)cc.Move(Vector3.forward*0.08f);
        for(int i=0;i<40;i++)cc.Move(Vector3.left*0.08f);
        if(cc.transform.position.x>= -2.8f)
        {
            var hits=Physics.OverlapCapsule(cc.transform.position+Vector3.up*0.33f,cc.transform.position+Vector3.up*1.47f,0.35f).Where(c=>c!=cc).Select(c=>c.name+" "+c.bounds).ToArray();
            var c=Camera.main;var oldPose=new Pose(c.transform.position,c.transform.rotation);c.transform.position=new Vector3(-1.4f,1.7f,0.4f);c.transform.LookAt(new Vector3(-3.5f,0.95f,-0.1f));Capture("AdditionalProps-Bathroom.png");c.transform.SetPositionAndRotation(oldPose.position,oldPose.rotation);
            throw new Exception("Bathroom route blocked at "+cc.transform.position+" by "+string.Join("; ",hits));
        }
        Require(true,"Bathroom entrance and central aisle remain walkable");
        cc.enabled=false;cc.transform.position=previous;cc.enabled=true;
        var camera=Camera.main;var pose=new Pose(camera.transform.position,camera.transform.rotation);float fov=camera.fieldOfView;
        Capture("AdditionalProps-Room.png");
        camera.fieldOfView=75;camera.transform.position=new Vector3(1.7f,1.7f,3.8f);camera.transform.LookAt(new Vector3(2.4f,1.2f,5.5f));Capture("AdditionalProps-Desk.png");
        camera.fieldOfView=90;camera.transform.position=new Vector3(-1.4f,1.7f,0.4f);camera.transform.LookAt(new Vector3(-3.5f,0.95f,-0.1f));Capture("AdditionalProps-Bathroom.png");
        camera.fieldOfView=75;camera.transform.position=new Vector3(1.5f,1.65f,1.1f);camera.transform.LookAt(new Vector3(4.4f,0.9f,0.15f));Capture("AdditionalProps-Appliances.png");
        camera.fieldOfView=65;camera.transform.position=new Vector3(2.45f,1.1f,0.4f);camera.transform.LookAt(new Vector3(2.4f,0.3f,-1.05f));Capture("AdditionalProps-TrashBin.png");
        camera.fieldOfView=65;camera.transform.position=new Vector3(2.7f,0.9f,0.2f);camera.transform.LookAt(new Vector3(3.55f,0.1f,-0.6f));Capture("AdditionalProps-Shoes.png");
        camera.transform.SetPositionAndRotation(pose.position,pose.rotation);camera.fieldOfView=fov;
        foreach(var r in GameObject.Find("Additional Room Props").GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)Require(m && m.shader.name=="Universal Render Pipeline/Lit","URP material on "+r.name);
    }
    private static IEnumerator VerifyBathroomDoor()
    {
        var door=Object.FindFirstObjectByType<BathroomDoorController>();Require(door,"Bathroom door controller present");
        var target=door.GetComponent<PrototypeInteractionTarget>();var cc=Object.FindFirstObjectByType<CharacterController>();var originalPosition=cc.transform.position;
        void Teleport(Vector3 p){cc.enabled=false;cc.transform.position=p;cc.enabled=true;Physics.SyncTransforms();}
        int choices=GameStateManager.Instance.CurrentState.choiceHistory.Count,actions=GameStateManager.Instance.CurrentState.completedInteractions.Count;
        Require(door.IsClosed && target.LocalPrompt=="화장실 문 열기","Bathroom door starts closed with open prompt");
        DoorImage("Layout-BathroomDoor-Closed.png");
        Teleport(new Vector3(-0.8f,0.08f,0.32f));
        for(int i=0;i<40;i++)cc.Move(Vector3.left*0.05f);
        Require(cc.transform.position.x> -1.4f,"Closed bathroom door blocks capsule from outside");
        Require(target.TryInteractLocally(),"F interaction dispatch handles bathroom door locally");
        float until=Time.time+1.1f;while(Time.time<until)yield return null;
        Require(door.IsOpen && target.LocalPrompt=="화장실 문 닫기","Bathroom door opens and switches to close prompt");
        DoorImage("Layout-BathroomDoor-Open.png");
        Teleport(new Vector3(-0.8f,0.08f,0.32f));for(int i=0;i<40;i++)cc.Move(Vector3.left*0.06f);
        Require(cc.transform.position.x< -2.8f,"Open bathroom door allows capsule passage");
        target.TryInteractLocally();until=Time.time+1.1f;while(Time.time<until)yield return null;
        Require(door.IsClosed,"Bathroom door closes from inside");
        Teleport(new Vector3(-2.2f,0.08f,0.32f));for(int i=0;i<30;i++)cc.Move(Vector3.right*0.06f);
        Require(cc.transform.position.x< -1.9f,"Closed bathroom door blocks capsule from inside");
        Teleport(new Vector3(-3.2f,0.08f,0.32f));target.TryInteractLocally();until=Time.time+1.1f;while(Time.time<until)yield return null;
        Require(door.IsOpen,"Bathroom door reopens from inside");
        Teleport(new Vector3(-1.65f,0.08f,0.32f));target.TryInteractLocally();until=Time.time+1.1f;while(Time.time<until)yield return null;
        Require(!door.IsClosed,"Door waits instead of closing through player");
        Teleport(new Vector3(-0.8f,0.08f,0.32f));until=Time.time+1.1f;while(Time.time<until)yield return null;
        Require(door.IsClosed,"Door finishes closing after player clears doorway");
        Require(choices==GameStateManager.Instance.CurrentState.choiceHistory.Count && actions==GameStateManager.Instance.CurrentState.completedInteractions.Count,"Bathroom door does not change story choices or daily actions");
        Teleport(originalPosition);target.TryInteractLocally();until=Time.time+1.1f;while(Time.time<until)yield return null;
        Require(door.IsOpen,"Bathroom door left open for remaining route verification");
    }
    private static void DoorImage(string path)
    {
        var camera=Camera.main;var old=new Pose(camera.transform.position,camera.transform.rotation);float fov=camera.fieldOfView;
        camera.fieldOfView=70;camera.transform.position=new Vector3(0.15f,1.55f,0.4f);camera.transform.LookAt(new Vector3(-1.65f,1.1f,0.43f));Capture(path);
        camera.transform.SetPositionAndRotation(old.position,old.rotation);camera.fieldOfView=fov;
    }
    private static void Capture(string path)
    {
        var camera = Camera.main;
        var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32); rt.Create();
        var request = new RenderPipeline.StandardRequest { destination = rt };
        for (int i = 0; i < 3; i++) RenderPipeline.SubmitRenderRequest(camera, request);
        var previous = RenderTexture.active; RenderTexture.active = rt;
        var texture = new Texture2D(1600, 1000, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, 1600, 1000), 0, 0); texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG()); RenderTexture.active = previous;
        rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(texture);
    }
}
