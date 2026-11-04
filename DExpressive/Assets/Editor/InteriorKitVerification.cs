using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

/// <summary>Opt-in batch verification, with no automatic scene writes on project open.</summary>
[InitializeOnLoad]
public static class InteriorKitVerification
{
    private const string Running = "InteriorKitVerification.Running";
    private static int frames;
    static InteriorKitVerification()
    {
        EditorApplication.update += Tick;
    }

    public static void Begin()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/InteriorKitTest.unity");
        SessionState.SetBool(Running, true);
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(Running, false) || !EditorApplication.isPlaying || ++frames < 80) return;
        SessionState.SetBool(Running, false);
        try
        {
            InteriorKitSetup.ValidateScene();
            var controller = Object.FindFirstObjectByType<CharacterController>();
            var originalPosition = controller.transform.position;
            // CharacterController checks use actual capsule motion rather than raycasts alone.
            for (int i = 0; i < 90; i++) controller.Move(Vector3.down * 0.02f);
            if (controller.transform.position.y < -0.05f) throw new Exception("Player fell through floor");
            controller.enabled = false; controller.transform.position = new Vector3(-3.8f, 0.08f, 0.7f); controller.enabled = true;
            for (int i = 0; i < 40; i++) controller.Move(Vector3.left * 0.08f);
            if (controller.transform.position.x < -4.8f) throw new Exception("Capsule passed through wall");
            var door = Object.FindFirstObjectByType<InteriorKitTestDoor>();
            door.Toggle();
            controller.enabled = false; controller.transform.position = new Vector3(0, 0.08f, -3.2f); controller.enabled = true;
            for (int i = 0; i < 25; i++) controller.Move(Vector3.back * 0.08f);
            if (controller.transform.position.z > -4.6f) throw new Exception("Capsule cannot pass open door");
            door.Toggle();
            controller.enabled = false; controller.transform.position = originalPosition; controller.enabled = true;
            Physics.SyncTransforms();
            Capture(Camera.main, "InteriorKitTest-preview.png");
            var camera = Camera.main;
            camera.transform.position = new Vector3(4, 1.7f, -3.2f);
            camera.transform.LookAt(new Vector3(-1.2f, 1.1f, 1));
            Capture(camera, "InteriorKitTest-corner.png");
            camera.transform.position = new Vector3(-3, 1.7f, 2.1f);
            camera.transform.LookAt(new Vector3(3.5f, 1, -1.5f));
            Capture(camera, "InteriorKitTest-kitchen.png");
            File.AppendAllText("InteriorKitValidation.txt", "PASS: actual Unity Play Mode; capsule floor support, wall blocking, open-door traversal; URP camera renders captured.\n");
            Debug.Log("INTERIOR_PLAYMODE_OK");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            File.AppendAllText("InteriorKitValidation.txt", "FAIL: " + e + "\n");
            EditorApplication.Exit(1);
        }
    }

    private static void Capture(Camera camera, string path)
    {
        var rt = new RenderTexture(1600, 1000, 24, RenderTextureFormat.ARGB32);
        rt.Create();
        var request = new RenderPipeline.StandardRequest { destination = rt };
        // Warm up shader/texture bindings before reading the final render back.
        RenderPipeline.SubmitRenderRequest(camera, request);
        RenderPipeline.SubmitRenderRequest(camera, request);
        RenderPipeline.SubmitRenderRequest(camera, request);
        var previous = RenderTexture.active; RenderTexture.active = rt;
        var texture = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        texture.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0); texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        RenderTexture.active = previous; rt.Release(); Object.DestroyImmediate(rt); Object.DestroyImmediate(texture);
    }
}
