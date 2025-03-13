using UnityEngine;
#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

using Turandot.Schedules;

public class TurandotState
{
    public string Project;
    public string Subject;
    public string ConfigFile;
    public string DataFile;
    public int LastBlockCompleted = -1;
    public bool Finished = false;
    public StimConList MasterSCL = null;
    public bool CanResume = false;

    public TurandotState() { }


    public TurandotState(string project, string subject, string configFile)
    {
        Project = project;
        Subject = subject;
        ConfigFile = configFile;
    }

    public bool IsRunInProgress()
    {
        bool result = false;
        Debug.Log(StateFile);
        if (File.Exists(StateFile))
        {
            TurandotState savedState = KLib.FileIO.JSONDeserialize<TurandotState>(StateFile);
            result = savedState.Project == Project && savedState.Subject == Subject && savedState.ConfigFile == ConfigFile && !savedState.Finished && savedState.CanResume;
        }

        return result;
    }

    public void RestoreProgress()
    {
        TurandotState savedState = KLib.FileIO.JSONDeserialize<TurandotState>(StateFile);
        DataFile = savedState.DataFile;
        LastBlockCompleted = savedState.LastBlockCompleted;
        MasterSCL = savedState.MasterSCL;
    }

    public void SetMasterSCL(StimConList scl)
    {
        MasterSCL = scl;
        Save();
    }

    public void SetDataFile(string name)
    {
        DataFile = name;
        Save();
    }

    public void SetLastBlock(int block)
    {
        LastBlockCompleted = block;
        Save();
    }

    public void Finish()
    {
        Finished = true;
        Save();
    }

    public void Save()
    {
        KLib.FileIO.JSONSerialize(this, StateFile);
    }

    private string StateFile
    {
        get { return Path.Combine(Application.persistentDataPath, "TurandotState.json"); }
    }

}