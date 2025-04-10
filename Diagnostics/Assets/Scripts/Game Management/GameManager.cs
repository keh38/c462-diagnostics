using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using KLib;

public class GameManager : MonoBehaviour
{
    private AppState _appState = new AppState();
    private bool _initialized = false;
    private SubjectMetadata _subjectMetadata;
    private Project.Settings _projectSettings;

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
    public static string DataForNextScene { get; set; }

    public static void SetSubject(string project, string subject) { instance._SetSubject(project, subject); }
    public static void SetSubject(string projectAndSubject)
    {
        var parts = projectAndSubject.Split('/');
        SetSubject(parts[0], parts[1]);
    }

    public static Project.Settings ProjectSettings { get { return instance._projectSettings; } }
    public static string Transducer
    {
        get { return instance._subjectMetadata?.Transducer; }
        set { instance._SetTransducer(value); }
    }

    public static Color BackgroundColor
    {
        get { return instance._GetBackgroundColor(); }
        set { instance._SetBackgroundColor(value); }
    }

    public static string SerializeSubjectMetadata()
    {
        return FileIO.XmlSerializeToString(instance._subjectMetadata);
    }

    public static void DeserializeSubjectMetadata(string data)
    {
        instance._subjectMetadata = FileIO.XmlDeserializeFromString<SubjectMetadata>(data);
        instance._SaveSubjectMetadata();
    }

    public static List<string> EnumerateTransducers()
    {
        var transducers = new List<string>();
        var localCalFolder = Path.Combine(FileLocations.BasicResourcesFolder, "Calibration");
        foreach (string path in Directory.GetFiles(localCalFolder))
        {
            var fn = Path.GetFileNameWithoutExtension(path);
            if (!fn.Contains("_") && (GameManager.ProjectSettings.ValidTransducers == null || GameManager.ProjectSettings.ValidTransducers.Contains(fn)))
            {
                transducers.Add(fn);
            }
        }
        transducers.Sort();

        return transducers;
    }

    public static int GetNextRunNumber(string measurementType)
    {
        int number = Mathf.Max(1, instance._subjectMetadata.runCounter[measurementType]);
        instance._subjectMetadata.runCounter[measurementType] = number + 1;
        instance._SaveSubjectMetadata();
        return number;
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
            _SetSubject(_appState.project, _appState.subject);
        }
        else
        {
            Debug.Log($"[GameManager] no subject restored");
        }
    }

    private void _SetSubject(string project, string subject)
    {
        if (!project.Equals(_appState.project) || !subject.Equals(_appState.subject))
        {
            _appState.project = project;
            _appState.subject = subject;
            _appState.Save();

            Debug.Log($"Subject changed to '{_appState.subject}'");

        }
        FileLocations.SetSubject(_appState.project, _appState.subject);

        //Debug.Log($"[GameManager] project settings");
        if (File.Exists(FileLocations.ConfigFile("Project.Settings")))
        {
            _projectSettings = FileIO.XmlDeserialize<Project.Settings>(FileLocations.ConfigFile("Project.Settings"));
        }
        else
        {
            _projectSettings = new Project.Settings();
        }

        //Debug.Log($"[GameManager] subject metadata");
        if (File.Exists(FileLocations.SubjectMetadataPath))
        {
            _subjectMetadata = FileIO.XmlDeserialize<SubjectMetadata>(FileLocations.SubjectMetadataPath);
        }
        else
        {
            _subjectMetadata = new SubjectMetadata()
            {
                ID = subject,
                Project = project,
                Transducer = _projectSettings.DefaultTransducer,
                Laterality = KLib.Signals.Laterality.Binaural,
                BackgroundColor = -1
            };
            _SaveSubjectMetadata();
        }
    }

    private void _SetTransducer(string transducer)
    {
        _subjectMetadata.Transducer = transducer;
        _SaveSubjectMetadata();
    }

    private Color _GetBackgroundColor()
    {
        if (_subjectMetadata.BackgroundColor > 0)
        {
            return KLib.ColorTranslator.ColorFromARGB(_subjectMetadata.BackgroundColor);
        }
        return new Color(214f / 255, 214f / 255, 214f / 255);
    }

    private void _SetBackgroundColor(Color color)
    {
        _subjectMetadata.BackgroundColor = KLib.ColorTranslator.ColorInt(color);
        _SaveSubjectMetadata();
    }

    private void _SaveSubjectMetadata()
    {
        if (!Directory.Exists(FileLocations.SubjectMetaFolder))
        {
            Directory.CreateDirectory(FileLocations.SubjectMetaFolder);
        }
        FileIO.XmlSerialize(_subjectMetadata, FileLocations.SubjectMetadataPath);
    }

    #endregion
}
