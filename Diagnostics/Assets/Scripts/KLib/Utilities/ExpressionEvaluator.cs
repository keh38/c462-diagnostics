using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ExpressionEvaluator : MonoBehaviour
{
    public float[] values = null;

    public string Evaluate(string expression)
    {
        string error = "";

        try
        {
            //SubjectManager.Instance.ChangeSubject("HypDiagnostics", "_Ken");
            //KLib.Expressions.Audiogram = Audiograms.AudiogramData.Load();
            //KLib.Expressions.LDL = Audiograms.AudiogramData.Load(DataFileLocations.LDLPath);
            //KLib.Expressions.Metrics = SubjectManager.Instance.Metrics;

            List<KLib.Expressions.PropVal> propVals = new List<KLib.Expressions.PropVal>();
            propVals.Add(new KLib.Expressions.PropVal("X", 500));

            values = KLib.Expressions.Evaluate(expression, propVals);
            foreach (var v in values) Debug.Log(v);
        }
        catch (System.Exception ex)
        {
            values = null;
            error = ex.Message;
        }

        return error;
    }

    public bool EvaluateComparison(string expression)
    {
        bool result = false;

        try
        {
            //SubjectManager.Instance.ChangeSubject("CISpeech", "_Test");
            //KLib.Expressions.Audiogram = Audiograms.AudiogramData.Load();
            //KLib.Expressions.LDL = Audiograms.AudiogramData.Load(DataFileLocations.LDLPath);
            //KLib.Expressions.Metrics = SubjectManager.Instance.Metrics;

            result = KLib.Expressions.EvaluateComparison(expression);
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }

        return result;
    }

}
