using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;


using KLib;
using KLib.Signals;

[System.Serializable]
public class SubjectMetadata
{
    public string ID { set; get; }
    public string Project { set; get; }
    public string Transducer { set; get; }
    public Laterality Laterality { set; get; }
    public int BackgroundColor { set; get; }

    public SerializeableDictionary<int> runCounter = new SerializeableDictionary<int>();
    public SerializeableDictionary<string> metrics = new SerializeableDictionary<string>();
    //public Dictionary<string, MetricData> metrics = new Dictionary<string, MetricData>();
    //public System.Guid lastMsgID;

    //public Laterality laterality;

    //public string project = "";

    //public System.DateTime lastActivity;

    //public System.DateTime lastMessageTime = System.DateTime.MinValue;


    public SubjectMetadata()
    {
    }

 /*   public void AddMetric(string name, float val)
    {
        AddMetric(name, val, true);
    }

    public void AddMetric(string name, float val, bool autoSave)
    {
        MetricData m = new MetricData(System.DateTime.Now, val);
        if (metrics.ContainsKey(name))
        {
            metrics[name] = m;
        }
        else
        {
            metrics.Add(name, m);
        }

        if (autoSave) Save();
    }

    public float GetMetric(string name, float defaultVal)
    {
        float val = defaultVal;
        if (metrics.ContainsKey(name))
        {
            val = metrics[name].val;
        }
        return val;
    }

    public List<MetricPropVal> GetMetricPropVals()
    {
        var result = new List<MetricPropVal>();
        foreach (var key in metrics.Keys)
        {
            result.Add(new MetricPropVal(key, metrics[key].val));
        }
        return result;
    }
*/
}

