using System;
using System.IO;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using Protocols;

using KLib;
using System.Diagnostics.Eventing.Reader;

public class ProtocolManager : MonoBehaviour
{
    private string _protocolName;
    private Protocol _protocol;
    private ProtocolHistory _history;
    private string _historyPath;

    private bool _active = false;
    private int _nextTestIndex = 0;

    private DateTime _lastTime = DateTime.MinValue;

    #region Singleton creation
    // Singleton
    private static ProtocolManager _instance;
    private static ProtocolManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gobj = GameObject.Find("ProtocolManager");
                if (gobj != null)
                {
                    _instance = gobj.GetComponent<ProtocolManager>();
                }
                else
                {
                    _instance = new GameObject("ProtocolManager").AddComponent<ProtocolManager>();
                }
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }
    #endregion

    public static bool IsActive { get { return instance._active; } }
    public static void Clear() { instance._active = false; }
    public static Protocol Protocol { get { return instance._protocol; } }
    public static ProtocolHistory History { get { return instance._history; } }
    public static int NextTestIndex { get { return instance._nextTestIndex; } }
    public static DateTime LastestTime { get { return instance._history.LatestTime; } }
    public static bool InitializeProtocol(string protocolName) { return instance._InitializeProtocol(protocolName); }
    public static void StartProtocol(bool resume) { instance._StartProtocol(resume); }
    public static void Advance() { instance._Advance(); }
    public static void FinishTest(string datafile) { instance._FinishTest(datafile); }

    private bool _InitializeProtocol(string protocolName)
    {
        bool canResume = false;

        _protocolName = protocolName;

        string protocolPath = Path.Combine(FileLocations.ProtocolFolder, $"{protocolName}.xml");
        
        _protocol = FileIO.XmlDeserialize<Protocol>(protocolPath);
        _history = null;
        
        var fileList = Directory.GetFiles(FileLocations.SubjectFolder, $"{GameManager.Subject}-{protocolName}-History-*.json").ToList();
        if (fileList.Count > 0)
        {
            fileList.Sort((x, y) => File.GetCreationTime(y).CompareTo(File.GetCreationTime(x)));

            _history = FileIO.JSONDeserialize<ProtocolHistory>(fileList[0]);
            if (_history.Matches(_protocol) && !_history.Finished)
            {
                _historyPath = fileList[0];
                _nextTestIndex = _history.NextTextIndex;
                canResume = true;
            }
        }

        return canResume;
    }

    private void _StartProtocol(bool resume)
    {
        if (!resume || _history == null)
        {
            _history = new ProtocolHistory(_protocol);
            _nextTestIndex = 0;
            _historyPath = Path.Combine(
                FileLocations.SubjectFolder,
                $"{GameManager.Subject}-{_protocolName}-History-{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.json");
        }

        _active = true;
        if (_protocol.FullAuto)
        {
            _Advance();
        }
        else
        {
            SceneManager.LoadScene("Protocol");
        }
    }

    private void _Advance()
    {
        var nextTest = _history.Data[_nextTestIndex];
        GameManager.DataForNextScene = nextTest.Settings;

        if (nextTest.Scene == "Turandot")
        {
            SceneManager.LoadScene("Turandot");
        }
        else if (nextTest.Scene == "TScript")
        {
            var script = FileIO.XmlDeserialize<Turandot.Schedules.Script>(FileLocations.ConfigFile("TScript", nextTest.Settings));
            script.Apply(FileLocations.ProtocolFolder);
            _FinishTest("none");
        }
    }

    private void _FinishTest(string datafile)
    {
        _history.LatestTime = DateTime.Now;
        _history.Data[_nextTestIndex].Date = _history.LatestTime.ToString();
        _history.Data[_nextTestIndex].DataFile = datafile;
        _nextTestIndex++;
      
        FileIO.JSONSerialize(_history, _historyPath);

        if (!_history.Finished)
        {
            if (_protocol.FullAuto)
            {
                _Advance();
            }
            else if (_protocol.Tests[_nextTestIndex].AutoAdvance)
            {
                _Advance();
            }
            else
            {
                SceneManager.LoadScene("Protocol");
            }
        }
        else
        {
            SceneManager.LoadScene("Home");
        }

    }
}
