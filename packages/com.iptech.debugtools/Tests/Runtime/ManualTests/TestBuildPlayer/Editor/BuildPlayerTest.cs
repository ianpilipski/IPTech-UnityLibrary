using UnityEngine;
using System.Collections;
using System.IO;
using System;

using UnityEditor;

public class BuildPlayerTest {
    
    private bool isLoggingHooked;
    private bool wasRemoved;

    [MenuItem("Test/BuildPlayer")]
    public static void Execute() {
        BuildPlayerTest bpt = new BuildPlayerTest();
        bpt.ExecuteTests();
    }

    public void ExecuteTests() {
        HookLogs();
        try {
            RunEachTest();
        } catch(Exception e) {
            UnHookLogs();
            throw e;
        } finally {
            UnHookLogs();
        }

        if(wasRemoved) {
            Debug.LogError("was removed");
        }
    }

    private void RunEachTest() {
        BuildNonDevelopmentRemovesConsoleAsset();
        BuildDevelopmentDoesNotRemoveConsoleAsset();
    }

    private void HookLogs() {
        if(!isLoggingHooked) {
            Application.logMessageReceived += HandleLogMessageReceived;
            isLoggingHooked = true;
        }
    }

    private void UnHookLogs() {
        if(isLoggingHooked) {
            Application.logMessageReceived -= HandleLogMessageReceived;
            isLoggingHooked = false;
        }
    }

    private void HandleLogMessageReceived (string condition, string stackTrace, LogType type)
    {
        if(condition.Contains("Removing In Game Console Resource")) {
            wasRemoved = true;
        }
    }
        
    private void BuildNonDevelopmentRemovesConsoleAsset() {
        Debug.Log("Start BuildNonDevelopmentRemovesConsoleAsset");
        wasRemoved = false;
        BuildPlayer(BuildOptions.None);
        if(!wasRemoved) {
            throw new Exception("The debug console was not removed when it should have been!");
        }
        Debug.Log("Stop BuildNonDevelopmentRemovesConsoleAsset");
    }

    private void BuildDevelopmentDoesNotRemoveConsoleAsset() {
        Debug.Log("Start BuildDevelopmentDoesNotRemoveConsoleAsset");
        wasRemoved = false;
        BuildPlayer(BuildOptions.Development);
        if(wasRemoved) {
            throw new Exception("The debug console was removed when it should have been left in!");
        }
        Debug.Log("Stop BuildDevelopmentDoesNotRemoveConsoleAsset");
    }

    private void BuildPlayer(BuildOptions options) {
        string testScenePattern = "*empty_scene.unity";
        string[] scenes = Directory.GetFiles("Assets", testScenePattern, SearchOption.AllDirectories);
        string outputPath = Path.GetTempFileName();

		UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(scenes, outputPath, BuildTarget.Android, options);

        try{
            File.Delete(outputPath);
        } catch {}

		if(report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded) {
			throw new Exception("build failed");
		}
    }
}
