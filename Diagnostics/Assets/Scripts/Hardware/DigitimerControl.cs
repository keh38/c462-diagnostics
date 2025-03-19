using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using D128NET;

public class DigitimerControl : MonoBehaviour
{
    private D128ExAPI _d128 = null;

    void Start()
    {

    }

    private void OnDestroy()
    {
        if (_d128 != null)
        {
            _d128.Close();
        }
    }

    public void Initialize()
    {
        try
        {
            var d128 = new D128ExAPI();
            d128.Initialize();
            d128.GetState();
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to initialize Digitimer control: " + ex.Message);
        }
    }

    public bool EnableDevice(int deviceNum, KLib.Signals.Waveforms.Digitimer digitimer)
    {
        if (_d128 == null) return false;

        _d128[deviceNum].Mode = FloatToMode(digitimer.PulseMode);
        _d128[deviceNum].Polarity = FloatToPolarity(digitimer.PulsePolarity);
        _d128[deviceNum].Width = (int)digitimer.Width;
        _d128[deviceNum].Recovery = (int)digitimer.Recovery;
        _d128[deviceNum].Dwell = (int)digitimer.Dwell;
        _d128[deviceNum].Enable = EnableState.Enabled;

        var result = _d128.SetState();
        if (result != ErrorCode.Success)
        {
            Debug.Log($"Error setting Digitimer {deviceNum}: {result}");
        }

        return result == ErrorCode.Success;
    }

    private PulseMode FloatToMode(float value)
    {
        return ((int)value == 0) ? PulseMode.Monophasic : PulseMode.Biphasic;
    }

    private PulsePolarity FloatToPolarity(float value)
    {
        if ((int)value == 0)
        {
            return PulsePolarity.Positive;
        }
        else if ((int)value == 1)
        {
            return PulsePolarity.Positive;
        }
        return PulsePolarity.Alternating;
    }

    public bool DisableDevice(int deviceNum)
    {
        if (_d128 == null) return false;

        _d128[deviceNum].Enable = EnableState.Disabled;

        var result = _d128.SetState();
        if (result != ErrorCode.Success)
        {
            Debug.Log($"Error disabling Digitimer {deviceNum}: {result}");
        }

        return result == ErrorCode.Success;
    }

}
