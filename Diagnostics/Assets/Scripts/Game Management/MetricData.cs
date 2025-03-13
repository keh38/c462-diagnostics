using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using KLib;
using ProtoBuf;

[System.Serializable]
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class MetricData
{
    public System.DateTime date;
    public float val;
    public MetricData()
    {
    }
    public MetricData(System.DateTime date, float val)
    {
        this.date = date;
        this.val = val;
    }
}

[System.Serializable]
[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class MetricPropVal
{
    public string name;
    public float val;
    public MetricPropVal()
    {
    }
    public MetricPropVal(string name, float val)
    {
        this.name = name;
        this.val = val;
    }
}
