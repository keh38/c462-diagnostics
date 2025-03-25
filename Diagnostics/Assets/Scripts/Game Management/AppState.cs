using System;
using System.Collections.Generic;
using System.IO;

using KLib;

[Serializable]
public class AppState
{
    public string project;
    public string subject;
    public string lastDemo;
    public List<string> updatesApplied = new List<string>();
    public SerializeableDictionary<string> lastUsedItems = new SerializeableDictionary<string>();

    public AppState()
    {
        project = "";
        subject = "";
        lastDemo = "";
    }

    public static string GetLastUsedItem(string key)
    {
        var state = Restore();
        return state.lastUsedItems[key];
    }

    public static void SetLastUsedItem(string key, string value)
    {
        var state = Restore();
        state.lastUsedItems[key] = value;
        state.Save();
    }

    public static void AddUpdate(string update)
    {
        AppState state = Restore();
        state.updatesApplied.Add(update);
        state.Save();
    }

    public static AppState Restore()
    {
        AppState state = new AppState();
        if (File.Exists(FileLocations.StateFile))
        {
            state = FileIO.XmlDeserialize<AppState>(FileLocations.StateFile);
        }
        else
        {
            state.Save();
        }
        return state;
    }

    public void Save()
    {
        FileIO.XmlSerialize(this, FileLocations.StateFile);
    }


}
