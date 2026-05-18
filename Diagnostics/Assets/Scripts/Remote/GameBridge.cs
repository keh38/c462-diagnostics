using System;
using System.Linq;
using MathNet.Numerics;
using Microsoft.Win32;

using C462.Shared;

public static class GameBridge
{
    private const string RegistryKeyName = @"SOFTWARE\EPL\C462\SOS";
    private const string RegistryValue = "InstallPath";
    private const string GameExecutableName = "SoundOfSilence";

    public static void LaunchGame()
    {
        var process = System.Diagnostics.Process.GetProcessesByName(GameExecutableName).FirstOrDefault();
        if (process != null)
        {
            UnityEngine.Debug.Log($"[GameBridge] Game already running with PID {process.Id}, granting foreground permission.");
            WindowManager.GrantForegroundPermission(process.Id);    
            return;
        }

        string exeFolder = GetExecutableFolder();
        var executablePath = System.IO.Path.Combine(exeFolder, $"{GameExecutableName}.exe");
        var processStartInfo = new System.Diagnostics.ProcessStartInfo(executablePath);
        processStartInfo.WorkingDirectory = exeFolder;
        process = System.Diagnostics.Process.Start(processStartInfo);
    }

    private static string GetExecutableFolder()
    {
        using (var view64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
        {
            using (var subKey = view64.OpenSubKey(RegistryKeyName, false))
            {
                return subKey?.GetValue(RegistryValue) as string;
            }
        }
    }


}