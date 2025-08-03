using KLib.Signals.Calibration;
using UnityEngine;

public class UnitTests : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var acal = AcousticCalibration.Load("", "HD280", "Left");

        var cal = CalibrationFactory.Load(KLib.Signals.LevelUnits.dB_SPL, "HD280", "Left");
        float max = cal.GetMax(1000);
        Debug.Log($"max = {max}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
