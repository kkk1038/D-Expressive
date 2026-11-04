using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class ReleaseBuild
{
    [MenuItem("DExpressive/Build Windows Release")]
    public static void Windows()
    {
        Directory.CreateDirectory("Assets/Resources/Materials");
        const string materialPath = "Assets/Resources/Materials/RuntimeRoom.mat";
        if (AssetDatabase.LoadAssetAtPath<Material>(materialPath) == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new Exception("URP Lit shader missing");
            var material = new Material(shader);
            material.SetFloat("_Smoothness", 0.22f);
            AssetDatabase.CreateAsset(material, materialPath);
            AssetDatabase.SaveAssets();
        }
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.bundleVersion = "0.1.0";
        Directory.CreateDirectory("Builds/Windows");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/SampleScene.unity" },
            locationPathName = "Builds/Windows/DExpressive.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        });
        var summary = report.summary;
        File.WriteAllText("Builds/build-result.txt",
            $"Result: {summary.result}\nPlatform: Windows x64\nUnity: {Application.unityVersion}\n" +
            $"Size: {summary.totalSize}\nErrors: {summary.totalErrors}\nWarnings: {summary.totalWarnings}\n" +
            $"Duration: {summary.totalTime}\nBuilt UTC: {DateTime.UtcNow:O}\n");
        if (summary.result != BuildResult.Succeeded) throw new Exception("Windows build failed: " + summary.result);
        Debug.Log("DEXPRESSIVE_WINDOWS_BUILD_OK");
    }
}
