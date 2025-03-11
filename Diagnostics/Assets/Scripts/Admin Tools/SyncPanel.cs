using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using KLib.MSGraph;

public class SyncPanel : MonoBehaviour
{
    [SerializeField] private Toggle _logsToggle;
    [SerializeField] private Button _syncButton;
    [SerializeField] private ProgressBar _progressBar;

    private int _numSelected = 0;

    public void OnLogsToggleClick(bool pressed)
    {
        _numSelected += pressed ? 1 : -1;
        _syncButton.interactable = _numSelected > 0;
    }

    public void OnSyncButtonClick()
    {
        _logsToggle.interactable = false;
        _syncButton.interactable = false;
        StartCoroutine(DoSync());
    }

    private IEnumerator DoSync()
    {
        _progressBar.gameObject.SetActive(true);
        yield return null;

        if (_logsToggle.isOn)
        {
            yield return StartCoroutine(UploadLogs());
            _numSelected--;
        }

        _progressBar.gameObject.SetActive(false);
        _logsToggle.interactable = true;
    }

    private IEnumerator UploadLogs()
    {
        string remoteFolder = $"{GameManager.Project}/Subjects/{GameManager.Subject}";
        Debug.Log($"Uploading logs to '{remoteFolder}'");
        KLib.KLogger.Log.FlushLog();

        var folder = Path.Combine(Application.persistentDataPath, "Logs");
        var appLogList = Directory.GetFiles(folder, "*.log");

        folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "EPL", "Logs");
        var streamerLogList = Directory.GetFiles(folder, "EPLib.Audio.*.log");

        var logList = new List<string>();
        logList.AddRange(appLogList);
        logList.AddRange(streamerLogList);
  
        _progressBar.Label.text = "Uploading logs...";

        for (int k = 0; k < logList.Count; k++)
        {
            Debug.Log(logList[k]);
            _progressBar.SetProgress(k + 1, logList.Count);
            MSGraphClient.UploadFile(remoteFolder, logList[k]);
            yield return null;
        }

        _logsToggle.SetIsOnWithoutNotify(false);
    }
}
