using System;
using UnityEngine;

public class BasicMeasurementFileHeader
{
    public string date;
    public string measurementType;
    public string configName;
    public string subjectID;
    public string version;

    public BasicMeasurementFileHeader()
    {
        date = DateTime.Now.ToString();
        version = Application.version;
    }
}