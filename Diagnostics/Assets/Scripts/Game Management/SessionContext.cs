using UnityEngine;
using KLib.Signals;
using Audiograms;
using C462.Shared;

/// <summary>
/// Session-wide signal context. Populated once when a subject is selected
/// and cleared when returning to the home screen.
/// All measurement scenes read from SessionContext.Signal rather than
/// loading audiograms or calibration data independently.
/// </summary>
public static class SessionContext
{
    public static SignalContext Signal { get; private set; }
    public static bool IsReady => Signal != null;

    public static void Initialize(AdapterMap adapterMap)
    {
        Signal = new SignalContext
        {
            AdapterMap = adapterMap,
        };
    }

    public static void SetTransducer(string transducer)
    {
        if (Signal == null)
        {
            Debug.LogError("SessionContext not initialized. Call Initialize() before setting transducer.");
            return;
        }
        Signal.Transducer = transducer;
        Signal.CalibrationFolder = SharedFileLocations.GetCalibrationFolder(transducer);
    }

    public static void SetAudiogram(string audiogramPath)
    {
        if (Signal == null)
        {
            Debug.LogError("SessionContext not initialized. Call Initialize() before setting audiogram.");
            return;
        }
        Signal.Audiogram = AudiogramData.Load(audiogramPath);
    }

    public static void SetLDL(string ldlPath)
    {
        if (Signal == null)
        {
            Debug.LogError("SessionContext not initialized. Call Initialize() before setting LDL.");
            return;
        }
        var ldl = AudiogramData.Load(ldlPath);
        if (ldl != null) ldl.ReplaceNaNWithMax(SessionContext.Signal);
        Signal.LDL = ldl;
    }

    public static void SetWavFolder(string folder)
    {
        if (Signal == null)
        {
            Debug.LogError("SessionContext not initialized. Call Initialize() before setting Wav Folder.");
            return;
        }
        Signal.WavFolder = folder;  
    }

    //public static void Clear()
    //{
    //    Signal = null;
    //    Debug.Log("SessionContext cleared");
    //}
}