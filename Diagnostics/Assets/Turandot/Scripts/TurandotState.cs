using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

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
    public float Progress = 0;
    public List<SCLElement> CurrentBlockSCL = null;

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
        //Debug.Log(StateFile);
        if (File.Exists(StateFile))
        {
            TurandotState savedState = KLib.FileIO.JSONDeserialize<TurandotState>(StateFile);
            //Debug.Log(savedState.ToString());

            result = savedState.Project == Project && 
                savedState.Subject == Subject && 
                savedState.ConfigFile == ConfigFile && 
                !savedState.Finished && 
                savedState.CanResume;
        }

        return result;
    }

    public void RestoreProgress()
    {
        TurandotState savedState = KLib.FileIO.JSONDeserialize<TurandotState>(StateFile);
        DataFile = savedState.DataFile;
        LastBlockCompleted = savedState.LastBlockCompleted;
        MasterSCL = savedState.MasterSCL;
        CurrentBlockSCL = savedState.CurrentBlockSCL;
        Progress = savedState.Progress;
        Finished = savedState.Finished;
        CanResume = savedState.CanResume;
    }

    public void SetMasterSCL(StimConList scl)
    {
        MasterSCL = scl;
        Save();
    }

    public void SetDataFile(string name)
    {
        DataFile = name;
        //Save();
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

    public override string ToString()
    {
        string result = $"{Project}/{Subject}{Environment.NewLine}" +
            $"Config file = {ConfigFile}{Environment.NewLine}" +
            $"Data path = {DataFile}{Environment.NewLine}" +
            $"Last block = {LastBlockCompleted}{Environment.NewLine}" +
            $"Finished = {Finished}{Environment.NewLine}" +
            $"Can resume = {CanResume}{Environment.NewLine}";
        return result;
    }
}