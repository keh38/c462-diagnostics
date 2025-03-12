using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using KLib;

public class GameManager : MonoBehaviour
{
    private AppState _appState = new AppState();
    private bool _initialized = false;

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

    public static void SetSubject(string project, string subject) { instance._setSubject(project, subject); }
    public static void SetSubject(string projectAndSubject)
    {
        var parts = projectAndSubject.Split('/');
        SetSubject(parts[0], parts[1]);
    }
    #endregion

    #region Private methods
    // Private methods
    private void Init()
    {
        _appState = AppState.Restore();
        if (!string.IsNullOrEmpty(_appState.project) && !string.IsNullOrEmpty(_appState.subject))
        {
            Debug.Log($"[GameManager] Restored '{_appState.project}/{_appState.subject}'");
            FileLocations.SetSubject(_appState.project, _appState.subject);
        }
        else
        {
            Debug.Log($"[GameManager] no subject restored");
        }
    }

    private void _setSubject(string project, string subject)
    {
        if (!project.Equals(_appState.project) || !subject.Equals(_appState.subject))
        {
            _appState.project = project;
            _appState.subject = subject;
            _appState.Save();

            Debug.Log($"Subject changed to '{_appState.subject}'");

            FileLocations.SetSubject(_appState.project, _appState.subject);
        }
    }

    #endregion
}
