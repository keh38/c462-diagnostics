using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KLib;

public class GameManager : MonoBehaviour
{
    private AppState _appState = new AppState();
    private bool _initialized = false;

    //private VersionInfo _versionInfo = new VersionInfo("Training App Sandbox", 0, 2, 0);

    #region Singleton creation
    // Singleton
    private static GameManager _instance;
    private static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gobj = GameObject.Find("GameManager");
                if (gobj != null)
                {
                    _instance = gobj.GetComponent<GameManager>();
                }
                else
                {
                    _instance = new GameObject("GameManager").AddComponent<GameManager>();
                }
                DontDestroyOnLoad(_instance);
                _instance.Init();
            }
            return _instance;
        }
    }
    #endregion

    #region Public static accessors
    // Public static accessors
    public static bool Initialized
    {
        set { instance._initialized = value; }
        get { return instance._initialized; }
    }
    public static string Subject { get { return instance._appState.subject; } }
    public static string Project { get { return instance._appState.project; } }

    public static string LastDemo
    {
        get { return instance._appState.lastDemo; }
        set
        {
            instance._appState.lastDemo = value;
            instance._appState.Save();
        }
    }

    public static void SetSubject(string project, string subject) { instance.m_setSubject(project, subject); }
    #endregion

    #region Private methods
    // Private methods
    private void Init()
    {
        _appState = AppState.Restore();
        Debug.Log($"Restored '{_appState.project}/{_appState.subject}'");
        FileLocations.SetID(_appState.project, _appState.subject);
    }

    private void m_setSubject(string project, string subject)
    {
        _appState.project = project;
        _appState.subject = subject;
        _appState.Save();

        Debug.Log($"Subject changed to '{_appState.subject}'");

        FileLocations.SetID(_appState.project, _appState.subject);
    }

    #endregion
}
