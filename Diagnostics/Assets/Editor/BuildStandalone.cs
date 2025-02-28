using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildStandalone : MonoBehaviour
{
    [MenuItem("Tools/Build Windows standalone")]
    public static void MyBuild()
    {
        ClearConsole();
        Debug.Log("Beginning Windows standalone build...");

        var scenesInBuild = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

        var buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenesInBuild.Select(x => x.path).ToArray();
        buildPlayerOptions.locationPathName = "Build/HearingDiagnostics.exe";
        buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded.");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed.");
            return;
        }

        /*
        Debug.Log("Creating installer...");
        var proc = new System.Diagnostics.Process();
        proc.StartInfo.FileName = "\"C:\\Program Files (x86)\\Inno Setup 6\\iscc.exe\"";
        //proc.StartInfo.Arguments = " /DrevStr=\"" + VersionInfo.Version + "\"";
        proc.StartInfo.Arguments = " \"D:\\Development\\Jenks\\jenks-navi-vr\\Installers\\NAVI combined.iss\"";

        Debug.Log(proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);

        proc.Start();
        proc.WaitForExit();
        */
        Debug.Log("Build pipeline complete.");

    }

    static void ClearConsole()
    {
        var logEntries = System.Type.GetType("UnityEditor.LogEntries, UnityEditor.dll");
        var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
        clearMethod.Invoke(null, null);
    }
}