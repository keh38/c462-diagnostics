using System;
using System.Collections;
using System.IO;

using UnityEngine;
using UnityEngine.SceneManagement;

public class PupilDynamicRange : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private Camera _camera;

    private float off1 = 1f;
    private float on = 5f;
    private float off2 = 1.5f;
    private int numReps = 4;

    private bool _isRunning = false;
    private float _curTime = 0;
    private float _modRateHz = 0.05f;
    private int _curPeriod = 0;

    void Start()
    {
        HTS_Server.SetCurrentScene("Pupil Dynamic Range", this);

        //StartCoroutine(RunTest());
    }

    void InitializeMeasurement(string data)
    {
        string fn = GameManager.Subject + "-PupilDR-" + System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
    }

    void Update()
    {
        if (!_isRunning) return;

        int nper = Mathf.FloorToInt(_curTime * _modRateHz) + 1;
        if (nper > _curPeriod)
        {
            _curPeriod++;
            //IPC.Instance.SendCommand("Trial", _curPeriod.ToString());

            if (_curPeriod > numReps)
            {
                _isRunning = false;
                StartCoroutine(EndTest());
            }
        }

        float intensity = 0.5f * (1 - Mathf.Cos(2 * Mathf.PI * _curTime * _modRateHz));
        SetScreenIntensity(intensity);
        _curTime += Time.deltaTime;
    }

    private void SetScreenIntensity(float intensity)
    {
        _camera.backgroundColor = new Color(intensity, intensity, intensity);
    }

    private IEnumerator RunTest()
    {

        //IPC.Instance.Connect();
        //IPC.Instance.StartRecording(fn);
        yield return new WaitForSeconds(2f);

        _curTime = 0;
        _curPeriod = 0;
        _isRunning = true;
        yield return null;
    }

    private IEnumerator EndTest()
    {
        yield return new WaitForSeconds(2f);
        //IPC.Instance.StopRecording();
        //IPC.Instance.Disconnect();

        //DiagnosticsManager.Instance.AdvanceProtocol();
        //Application.LoadLevel(DiagnosticsManager.Instance.ReturnToScene);
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A)
        {
            _isRunning = false;
            SceneManager.LoadScene("Home");
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        switch (command)
        {
            case "Initialize":
                InitializeMeasurement(data);
                break;
            case "StartSynchronizing":
                HardwareInterface.ClockSync.StartSynchronizing(Path.GetFileName(data));
                break;
            case "StopSynchronizing":
                HardwareInterface.ClockSync.StopSynchronizing();
                break;
            case "Begin":
                //Begin();
                break;
            case "Abort":
                //RpcAbort();
                break;
            case "SendSyncLog":
                var logPath = HardwareInterface.ClockSync.LogFile;
                if (!string.IsNullOrEmpty(logPath))
                {
                    HTS_Server.SendMessage("Turandot", $"ReceiveData:{Path.GetFileName(logPath)}:{File.ReadAllText(logPath)}");
                }
                break;
        }

    }
}

