using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildScript
{
    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToArray();
    }

    public static void BuildAndroid()
    {
        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("No enabled scenes found in EditorBuildSettings!");
            EditorApplication.Exit(1);
            return;
        }

        string buildPath = Path.Combine("build", "android");
        Directory.CreateDirectory(buildPath);
        string buildFilePath = Path.Combine(buildPath, "tile-match.apk");

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildFilePath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        Debug.Log("Starting Android build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"Android Build Succeeded! Path: {buildFilePath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"Android Build Failed with {summary.totalErrors} errors.");
            EditorApplication.Exit(1);
        }
    }

    public static void BuildIOS()
    {
        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("No enabled scenes found in EditorBuildSettings!");
            EditorApplication.Exit(1);
            return;
        }

        string buildPath = Path.Combine("build", "ios");
        Directory.CreateDirectory(buildPath);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        Debug.Log("Starting iOS build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"iOS Build Succeeded! Path: {buildPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"iOS Build Failed with {summary.totalErrors} errors.");
            EditorApplication.Exit(1);
        }
    }
}
