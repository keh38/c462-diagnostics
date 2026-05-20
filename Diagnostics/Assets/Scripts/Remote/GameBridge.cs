using System;
using System.Linq;
using MathNet.Numerics;
using Microsoft.Win32;

using C462.Shared;
using C462.Shared.Protocol.DTOs;
using KLibU.Net;
using System.Net;

public static class GameBridge
{
    private const string RegistryKeyName = @"SOFTWARE\EPL\C462\SOS";
    private const string RegistryValue = "InstallPath";
    private const string GameExecutableName = "SoundOfSilence";

    private static NotificationDescriptor _notification;


    public static void RetakeControl(RunMeasurementsPayload runMeasurementsPayload)
    {
        _notification = runMeasurementsPayload.Notification;
        GameManager.SetSubject($"{runMeasurementsPayload.Project}/{runMeasurementsPayload.Subject}");
        HTS_Server.SendRequest("ChangedSubject", $"{runMeasurementsPayload.Project}/{runMeasurementsPayload.Subject}");
        HardwareInterface.Resume();
        WindowManager.BringToFront();
    }

    public static void RestoreControlToGame()
    {
        if (_notification != null)
        {
            WindowManager.GrantForegroundPermission(_notification.ProcessId);
            SendNotification(_notification);
            _notification = null;
            return;
        }

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

    private static void SendNotification(NotificationDescriptor notification)
    {
        var gameEndPoint = new IPEndPoint(
            IPAddress.Parse(notification.Address),
            notification.Port);

        // Fire and forget — the Game's listener just needs the knock.
        KTcpClient.SendRequest(gameEndPoint, TcpMessage.Request(notification.Command));  // "MeasurementsComplete"
    }


}