using System;
using UnityEngine;

public class BasicMeasurementFileHeader
{
    public string date;
    public string measurementType;
    public string configName;
    public string subjectID;
    public string appName;
    public string version;

    public BasicMeasurementFileHeader()
    {
        date = DateTime.Now.ToString();
        appName = Application.productName;
        version = Application.version;
    }
}