using UnityEngine;
using System.Collections;

using KLib.Signals;
using KLib.Signals.Waveforms;

using Turandot;

public class TurandotUI : MonoBehaviour
{
    // TURANDOT FIX
    /*
    public UIInput levelInput;
    public UIInput bwInput;
    public UIInput tonInput;
    public UIInput toffInput;
    public UIInput npulseInput;
    public UIInput cfexprInput;
    public UIInput speedInput;
    public UIToggle contactToggle;
    public UIToggle fixedToggle;
    public UIPopupList paramPopup;

    Parameters _params;
    FlowElement _fe;
    SignalManager _sigMan;
    Channel _ch;
    Noise _noise;

    bool _ignoreEvents = true;
    bool _started = false;

	void Start ()
    {
#if CONFIG_HACK
        SubjectManager.Instance.ChangeSubject("Scratch", "Ken");
#endif
        paramPopup.value = "RI";
        SetParameters();

        _started = true;
    }


    private void SetParameters()
    {
        var localName = DataFileLocations.ConfigFile("Turandot", paramPopup.value);
        _params = KLib.FileIO.XmlDeserialize<Parameters>(localName);

        _fe = _params.flowChart.Find(o => o.name == "Stimulus");
        _ch = _fe.sigMan["Signal"];
        _noise = _ch.waveform as Noise;

        _ignoreEvents = true;

        levelInput.value = _ch.level.Value.ToString();
        bwInput.value = _noise.filter.BW.ToString();

        tonInput.value = (0.001f * _ch.gate.Duration_ms).ToString();
        toffInput.value = (0.001f * (_ch.gate.Period_ms - _ch.gate.Duration_ms)).ToString();

        npulseInput.value = (float.Parse(_fe.timeOuts[0].expr) / (0.001f * _ch.gate.Period_ms)).ToString();

        cfexprInput.value = _params.schedule.families[0].variables[0].expression;

        speedInput.value = _params.screen.grapherLayout.speed.ToString();
        contactToggle.value = _params.screen.grapherLayout.mustContactStylus;
        fixedToggle.value = _params.screen.grapherLayout.stylusPositionFixed;

        _ignoreEvents = false;
    }

    public void OnParamsChange()
    {
        if (_started) SetParameters();
    }

    public void OnLevelChange()
    {
        if (!_ignoreEvents)
        {
            _ch.level.Value = float.Parse(levelInput.value);
        }
    }

    public void OnBWChange()
    {
        if (!_ignoreEvents)
        {
            _noise.filter.BW = float.Parse(bwInput.value);
        }
    }

    public void OnTonChange()
    {
        if (!_ignoreEvents)
        {
            _ch.gate.Duration_ms = 1000 * float.Parse(tonInput.value);
            _ch.gate.Period_ms = 1000 * float.Parse(toffInput.value) + _ch.gate.Duration_ms;
            _fe.timeOuts[0].expr = (int.Parse(npulseInput.value) * 0.001f * _ch.gate.Period_ms).ToString();
        }
    }

    public void OnToffChange()
    {
        if (!_ignoreEvents)
        {
            _ch.gate.Period_ms = 1000 * float.Parse(toffInput.value) + _ch.gate.Duration_ms;
            _fe.timeOuts[0].expr = (int.Parse(npulseInput.value) * 0.001f * _ch.gate.Period_ms).ToString();
        }
    }

    public void OnNpulseChange()
    {
        if (!_ignoreEvents)
        {
            _fe.timeOuts[0].expr = (int.Parse(npulseInput.value) * 0.001f * _ch.gate.Period_ms).ToString();
        }
    }

    public void OnCFexprChange()
    {
        if (!_ignoreEvents)
        {
            _params.schedule.families[0].variables[0].expression = cfexprInput.value;
        }
    }

    public void OnSpeedChange()
    {
        if (!_ignoreEvents)
        {
            _params.screen.grapherLayout.speed = float.Parse(speedInput.value);
        }
    }

    public void OnContactChange()
    {
        if (!_ignoreEvents)
        {
            _params.screen.grapherLayout.mustContactStylus = contactToggle.value;
        }
    }

    public void OnFixedChange()
    {
        if (!_ignoreEvents)
        {
            _params.screen.grapherLayout.stylusPositionFixed = fixedToggle.value;
            _params.screen.grapherLayout.stylusX = fixedToggle.value ? 1.4f : -1.5f;
        }
    }

    public void OnStartButton()
    {
        var localName = DataFileLocations.ConfigFile("Turandot", paramPopup.value);
        KLib.FileIO.XmlSerialize(_params, localName);
        Debug.Log(localName);
        DiagnosticsManager.Instance.MakeExtracurricular("Turandot", paramPopup.value, "Turandot UI");
        Application.LoadLevel("Turandot");
    }

    public void Back()
    {
        Application.LoadLevel("Backdoor Scene");
    }
*/
}
