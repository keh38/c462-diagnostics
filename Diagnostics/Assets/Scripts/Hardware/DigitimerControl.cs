using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using C462.Digitimer.Abstractions;
using D128NET;

public class DigitimerControl : MonoBehaviour
{
    private ID128ExAPI _d128 = null;
    private DigitimerTrigger _trigger = null;

    public void CleanUp()
    {
        if (_d128 != null)
        {
            _d128.Close();
        }
    }

    public void CloseHandle()
    {
        CleanUp();
    }

    public void OpenHandle()
    {
        Initialize();
    }

    public bool VerifyTriggersDisabled()
    {
        bool allDisabled = true;
        if (_d128 != null)
        {
            foreach (var device in _d128.Devices)
            {
                if (_d128[device].Enable == EnableState.Enabled)
                {
                    Debug.Log($"Warning: Digitimer {device} is still enabled.");
                    allDisabled = false;
                }
            }
        }
        return allDisabled;
    }

    public void Inject(ID128ExAPI api)
    {
        _d128 = api;
    }

    public bool Initialize()
    {
        bool success = false;
        try
        {
            _d128 ??= new D128ExAPI();
            _d128.Initialize();
            _d128.GetState();
            success = true;
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to initialize Digitimer control: " + ex.Message);
        }
        return success;
    }

    public bool InitializeTrigger(string comPort)
    {
        _trigger = new DigitimerTrigger();
        return _trigger.Initialize(comPort);
    }

    public void EnableDevices(List<KLib.Signals.Channel> channels)
    {
        if (channels.Count == 0) return; // return true;

        bool success = true;

        foreach (var c in channels)
        {
            if (int.TryParse(c.MyEndpoint.transducer.Substring(("DS8R").Length), out int id))
            {
                success &= EnableDevice(id, c.Digitimer);
            }
        }

        if (_trigger.Connected)
        {
            success &= _trigger.EnableTrigger();
        }

        if (!success)
            throw new Exception("Failed to set the state of one or more Digitimer devices.");

        //return success;
    }

    public bool ZeroDevices(List<KLib.Signals.Channel> channels)
    {
        if (channels.Count == 0) return true;
        bool success = true;

        if (_trigger.Connected)
        {
            success = _trigger.DisableTrigger();
        }

        foreach (var c in channels)
        {
            if (int.TryParse(c.MyEndpoint.transducer.Substring(("DS8R").Length), out int id))
            {
                c.Digitimer.Demand = 0;
                success &= EnableDevice(id, c.Digitimer);
            }
        }
        return success;
    }

    public bool DisableDevices(List<KLib.Signals.Channel> channels)
    {
        if (channels.Count == 0) return true;
        bool success = true;

        if (_trigger.Connected)
        {
            success = _trigger.DisableTrigger();
        }

        foreach (var c in channels)
        {
            if (int.TryParse(c.MyEndpoint.transducer.Substring(("DS8R").Length), out int id))
            {
                success &= DisableDevice(id);
            }
        }
        return success;
    }

    public bool EnableDevice(int deviceNum, KLib.Signals.Digitimer digitimer)
    {
        if (_d128 == null || _d128[deviceNum] == null) return false;

        _d128[deviceNum].Mode = FloatToMode(digitimer.PulseMode);
        _d128[deviceNum].Polarity = FloatToPolarity(digitimer.PulsePolarity);
        _d128[deviceNum].Width = (int)digitimer.Width;
        _d128[deviceNum].Recovery = (int)digitimer.Recovery;
        _d128[deviceNum].Dwell = (int)digitimer.Dwell;
        _d128[deviceNum].Source = IntToDemandSource((int)digitimer.Source);
        _d128[deviceNum].Demand = (int)(digitimer.Demand * 10);
        _d128[deviceNum].Enable = EnableState.Enabled;

        var result = _d128.SetState();
        if (result != ErrorCode.Success)
        {
            Debug.Log($"Error setting Digitimer {deviceNum}: {result}");
            throw new Exception($"Error setting Digitimer {deviceNum}: {result}");
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

    private DemandSource IntToDemandSource(int value)
    {
        if (value == 1)
        {
            return DemandSource.External;
        }
        return DemandSource.Internal;
    }

    public bool DisableDevice(int deviceNum)
    {
        if (_d128 == null || _d128[deviceNum] == null) return false;

        _d128[deviceNum].Enable = EnableState.Disabled;

        var result = _d128.SetState();
        if (result != ErrorCode.Success)
        {
            Debug.Log($"Error disabling Digitimer {deviceNum}: {result}");
        }

        return result == ErrorCode.Success;
    }

    public bool DisableAllDevices()
    {
        bool success = true;

        if (_trigger.Connected)
        {
            success = _trigger.DisableTrigger();
        }

        if (_d128 != null)
        {
            var result = _d128.DisableAll();
            success &= (result == ErrorCode.Success);
            //foreach (var device in _d128.Devices)
            //{
            //    success &= DisableDevice(device);
            //}
        }
        return success;    
     }

}
