using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Turandot;
using Turandot.Schedules;
using Turandot.Screen;
using Turandot.Scripts;

public class TurandotEngine : MonoBehaviour
{
    [SerializeField] private TurandotCueController _cueController;
    [SerializeField] private TurandotInputMonitor _inputMonitor;

    [SerializeField] private GameObject _audioPrefab;
    [SerializeField] private GameObject _audioDummyPrefab;

    Parameters _params;

    FlowElement _currentFlowElement = null;
    History _log = null;
    TrialType _trialType = TrialType.NoResult;
    TermType _termType = TermType.Any;
    string _logPath = null;
    string _result = "";
    float _reactionTime = float.NaN;
    string _stateEndReason = "";
    float _stateStartTime = 0;
    string _deferredLinkTo = "";
    EndAction _endAction = EndAction.None;

    bool _actionInProgress;

    List<TurandotAudio> _audio = new List<TurandotAudio>();

    public delegate void FlowchartFinishedDelegate();
    public FlowchartFinishedDelegate FlowchartFinished;
    private void OnFlowchartFinished() { FlowchartFinished?.Invoke(); }

    public string Result
    {
        get { return _result; }
    }

    public float ReactionTime
    {
        get { return _reactionTime; }
    }

    public EndAction FlowchartEndAction
    {
        get { return _endAction; }
    }

    public void ClearScreen()
    {
        _inputMonitor.ClearScreen();
        _cueController.ClearScreen();
        Cursor.visible = true;
    }

    public void Initialize(Parameters par)
    {
        _log = new History();
        _params = par;
        _currentFlowElement = null;
        _actionInProgress = false;

        CreateAudioPlayers();

        _inputMonitor.EventChanged = OnInputEventChanged;
        _inputMonitor.Initialize(_params.screen.Inputs, _params.inputEvents);

        _cueController.Initialize(_params.screen.Cues);
    }

    void CreateAudioPlayers()
    {
        foreach (TurandotAudio a in _audio)
        {
            Destroy(a.gameObject);
        }
        _audio.Clear();

        foreach (FlowElement fe in _params.flowChart)
        {
            GameObject o = GameObject.Instantiate(fe.sigMan != null ? _audioPrefab : _audioDummyPrefab);
            o.name = fe.name;
            o.transform.parent = _audioPrefab.transform.parent;

            TurandotAudio a = o.GetComponent<TurandotAudio>();
            a.name = fe.name;
            a.TimeOut = OnStateTimeout;
            a.Initialize(fe.sigMan);
            //if (fe.isAction)
            //{
            //    a.OnTimeOut = EndActionAudio;
            //}

            _audio.Add(a);
        }
    }

    public IEnumerator ExecuteFlowchart(TrialType trialType, List<Flag> flags, string logPath)
    {
        _trialType = trialType;
        if (_trialType == TrialType.CSplus) _termType = TermType.CSplus;
        else if (_trialType == TrialType.CSminus) _termType = TermType.CSminus;
        else _termType = TermType.Any;

        _logPath = logPath;

        _result = "";
        _reactionTime = float.NaN;
        _stateEndReason = "";
        _actionInProgress = false;

        foreach (TurandotAudio a in _audio)
        {
            a.Reset();
        }

        _inputMonitor.StartMonitor(flags);
        _cueController.ClearLog();
        _cueController.SetFlags(flags);

        yield return null; // the shit above takes time, first state time stamp will not be accurate without this

        _log.Clear();
        _log.Add(Time.timeSinceLevelLoad, HistoryEvent.StartTrial);

        NextState(_params.firstState);

        yield break;
    }

    public void ShowState(Parameters par, string state)
    {
        ClearScreen();

//        _cueController.Initialize(par.screen.Cues);
        //inputMonitor.Initialize(par.screen, par.buttons, par.inputEvents, par.InputsUsed);

        _inputMonitor.Activate(par[state].inputs, null, 0);
        _cueController.Activate(par[state].cues);
    }

#if KDEBUG
    public IEnumerator SimulateFlowchart(TrialType trialType, List<Flag> flags, string logPath, string simResult)
    {
        _trialType = trialType;
        if (_trialType == TrialType.CSplus) _termType = TermType.CSplus;
        else if (_trialType == TrialType.CSminus) _termType = TermType.CSminus;
        else _termType = TermType.Any;

        _logPath = logPath;

        _result = "";
        _reactionTime = float.NaN;
        _stateEndReason = "";

        _log.Clear();
        _log.Add(Time.timeSinceLevelLoad, HistoryEvent.StartTrial);

        foreach (TurandotAudio a in _audio)
        {
            a.Reset();
        }
 
        inputMonitor.StartMonitor(flags);
        cueController.ClearLog();

        yield return new WaitForSeconds(0.2f);

        _reactionTime = 0.5f;
        _result = simResult;

        NextState("");
    }
#endif

    void NextState(string nextState)
    {
        if (string.IsNullOrEmpty(nextState))
        {
            _log.Add(Time.timeSinceLevelLoad, HistoryEvent.EndTrial);

            _inputMonitor.StopMonitor();
            if (_params.trialLogOption != TrialLogOption.None) WriteLogFile();

#if KDEBUG
            _endAction = EndAction.None;
#else
            _endAction = _currentFlowElement.endAction;
#endif
            OnFlowchartFinished();
       }
        else
        {
            _currentFlowElement = _params.flowChart.Find(fe => fe.name == nextState);
            _stateEndReason = "";
            _deferredLinkTo = "";
            _stateStartTime = Time.timeSinceLevelLoad;

            Debug.Log("State: " + _currentFlowElement.name);
            Cursor.visible = !_currentFlowElement.hideCursor;

            var timeOut = _currentFlowElement.GetTimeout(_trialType).Value;
            var a = _audio.Find(o => o.name == _currentFlowElement.name);
            a.Activate(timeOut, _params.flags);

            HTS_Server.SendMessage("Turandot", $"State:{_currentFlowElement.name}");

            _log.Add(Time.timeSinceLevelLoad, HistoryEvent.StartState, _currentFlowElement.name);

            _cueController.Activate(_currentFlowElement.cues);
            _inputMonitor.Activate(_currentFlowElement.inputs, a, timeOut);
            _inputMonitor.PollEvents();
        }
    }
    private void DoAction(string name)
    {
        // TURANDOT FIX 
//        if (IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("Action", name);

        FlowElement actionState = _params.flowChart.Find(fe => fe.name == name);
        _cueController.Activate(actionState.cues);

        var a = _audio.Find(o => o.name == actionState.name);
        _inputMonitor.Activate(actionState.inputs, a, 0);

        _audio.Find(x => x.name == _currentFlowElement.name).PauseAudio(true);
        a.Activate(a.SigMan.GetMaxInterval(1) / 1000f, null);
        _log.Add(Time.timeSinceLevelLoad, HistoryEvent.StartAction, name);
        //_log.Add(Time.realtimeSinceStartup, HistoryEvent.StartAction, name);
    }

    private void EndActionAudio(string name)
    {
        _audio.Find(x => x.name == _currentFlowElement.name).PauseAudio(false);
        _log.Add(Time.timeSinceLevelLoad, HistoryEvent.EndAction, name);

        _actionInProgress = false;

        var actionState = _params.flowChart.Find(fe => fe.name == name);
        if (actionState.HasSequence)
        {
            if (actionState.AdvanceSequence())
            {
                var a = _audio.Find(o => o.name == actionState.name);
                a.Reset();
            }
            else
            {
                OnStateTimeout(_currentFlowElement.name);
            }
        }
        else
        {
            var a = _audio.Find(o => o.name == actionState.name);
            a.Reset();
        }
    }

    public void Abort()
    {
        if (_currentFlowElement != null) _audio.Find(a => a.name == _currentFlowElement.name).KillAudio();
        WriteLogFile();
        _inputMonitor.StopMonitor();
        _inputMonitor.Deactivate();
        _cueController.Deactivate();
    }

    void OnStateTimeout(string source)
    {
        string linkTo = "";

        if (source == _currentFlowElement.name) // to eliminate race conditions
        {
            //Debug.Log(_currentFlowElement.name + ": " +_stateEndReason);
            if (_stateEndReason == "") // not deferred ending from prior input event
            {
                _stateEndReason = "Timeout";
                string r = _currentFlowElement.GetTimeout(_trialType).result;
                if (!string.IsNullOrEmpty(r))
                {
                    _result = ExpandResult(_result, r);
                }
                linkTo = _currentFlowElement.GetTimeout(_trialType).linkTo;
            }
            else
            {
                linkTo = _deferredLinkTo;
            }

            _log.Add(Time.timeSinceLevelLoad, HistoryEvent.EndState, _stateEndReason);

            _cueController.Deactivate();
            _inputMonitor.Deactivate();
            NextState(linkTo);
        }
    }

    void OnInputEventChanged(string whichEvent)
    {
//#if KDEBUG
        if (_currentFlowElement == null) return;
//#endif
        Termination term = _currentFlowElement.term.Find(t => t.source == whichEvent && (t.type == TermType.Any || t.type == _termType));
        if (term != null && _stateEndReason=="" && Time.timeSinceLevelLoad - _stateStartTime >= 0.001f*term.latency_ms)
        {
            bool nextIsAction = false;
            if (!string.IsNullOrEmpty(term.linkTo)) nextIsAction = _params.flowChart.Find(fe => fe.name == term.linkTo).isAction;

            if (!nextIsAction) _stateEndReason = whichEvent;

            _log.Add(Time.timeSinceLevelLoad, HistoryEvent.TermCond, whichEvent);
            Debug.Log(whichEvent + ": " + term.action);

            if (!string.IsNullOrEmpty(term.result))
            {
                Debug.Log(term.source + ": " + term.result);
                _result = ExpandResult(_result, term.result);
            }

            if (!string.IsNullOrEmpty(term.flagExpr) && (!nextIsAction || !_actionInProgress))
            {
                EvaluateFlagExpression(term.flagExpr);
            }

            if (_currentFlowElement.name == _params.schedule.decisionState)
            {
                _reactionTime = Time.timeSinceLevelLoad - _stateStartTime;
            }

            if (term.action == TerminationAction.EndImmediately && !nextIsAction)
            {
                _cueController.Deactivate();
                _inputMonitor.Deactivate();
                _audio.Find(a => a.name == _currentFlowElement.name).KillAudio();
                _log.Add(Time.timeSinceLevelLoad, HistoryEvent.EndState, _stateEndReason);
                NextState(term.linkTo);
            }
            else
            {
                if (nextIsAction)
                {
                    if (!_actionInProgress)
                    {
                        _actionInProgress = true;
                        DoAction(term.linkTo);
                    }
                }
                else
                {
                    _deferredLinkTo = term.linkTo;
                }
            }
        }
    }

    string ExpandResult(string previous, string result)
    {
        string expanded = result;
        if (!expanded.Contains("=") && !expanded.Contains("{")) expanded = "outcome=\"" + result + "\";";

        var regex = new Regex(@"{([a-zA-Z0-9\.\s]+)}");
        foreach (var match in regex.Matches(result).Cast<Match>().Select(x => x.Groups[1].Value))
        {
            Debug.Log(match);
            if (_inputMonitor.Contains(match))
            {
                expanded = expanded.Replace($"{{{match}}}", _inputMonitor.ExpandResult(match));
            }
        }

        return previous + expanded;
    }

    string SubstituteResult(string result)
    {
        string expanded = result;

        if (result.ToLower().Contains("{scale}"))
        {
            expanded = expanded.Replace("{scale}", _inputMonitor.SliderSubstitution);
        }
        if (result.ToLower().Contains("{param}"))
        {
            expanded = expanded.Replace("{param}", _inputMonitor.ParamSubstitution);
        }
        expanded = ExpandSignalExpression(expanded);

        return expanded;
    }

    string ExpandSignalExpression(string expression)
    {
        string pattern = @"{([a-zA-Z0-9]+)\.([a-zA-Z0-9]+)\.([a-zA-Z0-9\.]+)}";
        Match m = Regex.Match(expression, pattern);

        if (m.Success)
        {
            var state = (m.Groups[1].Value == "sigMan") ? _currentFlowElement : _params[m.Groups[1].Value];
            if (state != null)
                expression = expression.Replace(m.Groups[0].Value, state.sigMan.GetParameter(m.Groups[2].Value, m.Groups[3].Value).ToString());
        }

        return expression;
    }

    void EvaluateFlagExpression(string expr)
    {
        string[] subExpr = expr.Split(new char[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (var e in subExpr)
        {
            var orExpr = e.Split(new string[] { "|=" }, System.StringSplitOptions.RemoveEmptyEntries);

            if (orExpr.Length == 2)
            {
                _params.OrFlag(orExpr[0].Trim(), KLib.Expressions.EvaluateToIntScalar(orExpr[1]));
            }
            else
            {
                string[] exprParts = e.Split('=');
                if (exprParts.Length == 2)
                {
                    string lh = exprParts[0].Trim();
                    string rh = SubstituteResult(exprParts[1]);

                    string pattern = @"(M\([a-zA-Z0-9\._,]+\))";
                    Match m = Regex.Match(lh, pattern);

                    if (m.Success)
                    {
                        lh = m.Groups[0].Value;
                        lh = lh.Substring(2, lh.Length - 3);
                        // TURANDOT FIX 
                        //SubjectManager.Instance.AddMetric(lh, KLib.Expressions.EvaluateToFloatScalar(rh));
                    }
                    else
                        _params.SetFlag(exprParts[0].Trim(), KLib.Expressions.EvaluateToIntScalar(rh));
                }
                else if (e.Substring(e.Length - 2, 2) == "++")
                {
                    _params.IncrementFlag(e.Substring(0, e.Length - 2));
                }
            }
        }
    }

    void WriteLogFile()
    {
        _log.Trim();

        string json = KLib.FileIO.JSONStringAdd("", "history", KLib.FileIO.JSONSerializeToString(_log));
        json = KLib.FileIO.JSONStringAdd(json, "events", _inputMonitor.EventLogJSONString);

        string cueJson = _cueController.LogJSONString;
        if (!string.IsNullOrEmpty(cueJson))
            json = KLib.FileIO.JSONStringAdd(json, "cues", cueJson);

        string inputJson = _inputMonitor.InputLogJSONString;
        if (!string.IsNullOrEmpty(inputJson))
        {
            json = KLib.FileIO.JSONStringAdd(json, "inputs", inputJson);
        }

        foreach (var fe in _params.flowChart.FindAll(x => x.HasSequence))
        {
            json = KLib.FileIO.JSONStringAdd(json, fe.name, fe.ActionSequenceJSONString);
        }

        foreach (var fe in _params.flowChart)
        {
            json = KLib.FileIO.JSONStringAdd(json, fe.name + "Audio", KLib.FileIO.JSONSerializeToString(_audio.Find(x => x.name.Equals(fe.name)).Log.Trim()));
        }

        File.WriteAllText(_logPath, json);
        // TURANDOT FIX
//        if (SubjectManager.Instance.UploadData && _params.trialLogOption == TrialLogOption.Upload) DataFileManager.UploadDataFile(_logPath);
    }

    public string GetEventsAsJSON()
    {
        var names = new StringBuilder(1000);
        var times = new StringBuilder(1000);
        foreach (var fe in _params.flowChart)
        {
            var a = _audio.Find(x => x.name.Equals(fe.name));
            for (int k=0; k < a.NumEvents; k++)
            {
                names.Append($"\"{fe.name}\", ");
                times.Append($"{a.EventTimes[k]:0.000000}, ");
            }
        }
        return names.Length > 0 ? $"\"Events\" : {{\"name\" : [{names.ToString()}], \"time\" : [{times.ToString()}]}}" : "";
    }

    public void WriteAudioLogFile(string path)
    {
        string json = "";
        foreach (var fe in _params.flowChart)
        {
            json = KLib.FileIO.JSONStringAdd(json, fe.name, KLib.FileIO.JSONSerializeToString(_audio.Find(x => x.name.Equals(fe.name)).Log.Trim()));
        }

        File.WriteAllText(path, json);
        //        if (SubjectManager.Instance.UploadData && _params.trialLogOption == TrialLogOption.Upload) DataFileManager.UploadDataFile(path);
        //if (SubjectManager.Instance.UploadData) DataFileManager.UploadDataFile(path);
    }
}
