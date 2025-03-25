using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

using KLib.Signals.Waveforms;
using Turandot;
using Turandot.Schedules;
using Turandot.Scripts;
using Turandot.Screen;

public class TurandotManager : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private TurandotEngine _engine;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private GameObject _finishPanel;

    //public UIProgressBar progressBar;

    Parameters _params = new Parameters();

    TurandotState _state = null;
    List<SCLElement> _SCL = new List<SCLElement>();
    TrialData _data = new TrialData();

    string _fileStem = "";
    string _mainDataFile = "";

    bool _waitingForResponse = false;
    bool _waitingForTap = false;
    bool _isRunning = false;
    bool _haveException = false;
    bool _isScripted = false;
    bool _usingServer = false;
    bool _waitForServer = false;

    string _paramFile = "";

    int _nominalNumBlocks;
    int _blockNum;
    int _numSinceLastBreak;
    int _numXDATrepeats = 5;

    float _progressBarStep = 1f;
    bool _runAborted = false;

    bool _performanceOK = false;

    List<string> _results = new List<string>();
    Results _resultsByStimulus = new Results();
    int _numBlocksAdded = 0;

    List<string> _synonymsForCorrect = new List<string>(new string[] { "hit", "withhold", "go", "right", "correct" });

    void Awake()
    {
        //Application.RegisterLogCallback(HandleException);
    }

    void OnDestroy()
    {
        //Application.logMessageReceived
    }
    
    //public string MainDataFile { get { return _mainDataFile; } }

    void Start()
    {
#if CONFIG_HACK
        SubjectManager.Instance.ChangeSubject("Scratch", "_Ken");
        //string configName = "RI-VAS";
        string configName = "TServer";
        //DiagnosticsManager.Instance.MakeExtracurricular("Turandot", "Turandot." + configName);
#else
        string configName = GameManager.DataForNextScene;
#endif

        _engine = GetComponent<TurandotEngine>();
        _engine.ClearScreen();

        _isScripted = (GameObject.Find("TurandotScripter") != null);

        //KLib.Expressions.Metrics = SubjectManager.Instance.Metrics;
        KLib.Expressions.Audiogram = Audiograms.AudiogramData.Load();
        var ldl = Audiograms.AudiogramData.Load(FileLocations.LDLPath);
        if (ldl != null) ldl.ReplaceNaNWithMax(GameManager.Transducer);
        KLib.Expressions.LDL = ldl;

        string localName = "";
        //if (!_isScripted && configName.Contains("TScript."))
        //{
        //    TurandotScripter.Instance.Initialize(DataFileLocations.ConfigFile(configName));
        //    _isScripted = true;
        //}

        //if (_isScripted)
        //{
        //    _params = TurandotScripter.Instance.Next();
        //    localName = TurandotScripter.Instance.ParamFile;
        //}
        //else
        {
            localName = FileLocations.ConfigFile("Turandot", configName);
            _params = KLib.FileIO.XmlDeserialize<Parameters>(localName);
        }

        _paramFile = Path.GetFileName(localName);
        _params.CheckParameters();
        _params.ApplyDefaultWavFolder(GameManager.Project);

        //try
        {
            _engine.Initialize(_params);//, SubjectManager.Instance.Transducer, SubjectManager.Instance.MaxLevelMargin);
        }
        //catch (Exception ex)
        //{
        //    //HandleException(ex.Message, ex.StackTrace, LogType.Exception);
        //    return;
        //}

        _state = new TurandotState(GameManager.Project, GameManager.Subject, configName);

        //if (_state.IsRunInProgress())
        //{
        ////    StartCoroutine(AskToResume());
        //}
        if (!string.IsNullOrEmpty(_params.instructions.Text))
        {
            StartCoroutine(ShowInstructions());
        }
        else
        {
            //StartRun();
        }
    }
    
    IEnumerator ShowInstructions()
    {
        //if (IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("Instructions", "started");

        yield return new WaitForSeconds(1);

        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = OnInstructionsFinished;
        _instructionPanel.ShowInstructions(_params.instructions);
    }
    private void OnInstructionsFinished()
    {
        _instructionPanel.gameObject.SetActive(false);
        StartRun();
    }

    /*
    void ShowMorePracticeInstructions()
    {
        diagnosticsUI.transform.position = new Vector2(0, 0);
        diagnosticsUI.SetHelpBalloonSize(1750, 800);
        diagnosticsUI.SetHelpBalloonPosition(new Vector3(0, 100, 0));
        diagnosticsUI.SetHelpBalloonAlpha(0.85f);
        var prompt = new List<string>() { "Let's practice a little more." };
        if (!string.IsNullOrEmpty(_params.schedule.performancePrompt))
        {
            prompt.Clear();
            prompt.Add(_params.schedule.performancePrompt);
        }
        diagnosticsUI.ShowInstructions(prompt, MorePractice);
    }

    void ShowBreakInstructions(string instructions)
    {
        if (string.IsNullOrEmpty(instructions))
        {
            instructions = "Great work!\nTake a short break if you need one.";
        }
        diagnosticsUI.transform.position = new Vector2(0, 0);
        diagnosticsUI.SetHelpBalloonSize(1750, 800);
        diagnosticsUI.SetHelpBalloonPosition(new Vector3(0, 100, 0));
        diagnosticsUI.SetHelpBalloonAlpha(0.85f);
        diagnosticsUI.ShowInstructions(new List<string>() { instructions }, EndBreak, "resume");
    }

    void ShowMoreAdaptationInstructions()
    {
        diagnosticsUI.transform.position = new Vector2(0, 0);
        diagnosticsUI.SetHelpBalloonSize(1750, 800);
        diagnosticsUI.SetHelpBalloonPosition(new Vector3(0, 100, 0));
        diagnosticsUI.SetHelpBalloonAlpha(0.85f);
        diagnosticsUI.ShowInstructions(new List<string>() { "Let's do a little more." }, MoreAdaptation);
    }

    void ShowMoreBlocksInstructions()
    {
        diagnosticsUI.transform.position = new Vector2(0, 0);
        diagnosticsUI.SetHelpBalloonSize(1750, 800);
        diagnosticsUI.SetHelpBalloonPosition(new Vector3(0, 100, 0));
        diagnosticsUI.SetHelpBalloonAlpha(0.85f);
        diagnosticsUI.ShowInstructions(new List<string>() { "Good work! Let's do a little more." }, MorePractice);
    }

    IEnumerator AskToResume()
    {
        yield return StartCoroutine(diagnosticsUI.AskYesNoQuestion("Continue previous work?"));
        if (diagnosticsUI.YesNoResult)
        {
            ResumeRun();
        }
        else if (_params.instructions.pages.Count > 0)
        {
            StartCoroutine(ShowInstructions());
        }
        else
        {
            StartRun();
        }
    }

    public void OnTap()
    {
        if (_waitingForTap)
        {
            _waitingForTap = false;

            if (_waitingForResponse)
                Return();
            else
            {
                prompt.Deactivate();
                StartRun();
            }
        }
    }

    void ResumeRun()
    {
        _state.RestoreProgress();
        _state.CanResume = false;
        _state.Save();

        _blockNum = _state.LastBlockCompleted + 1;
        _mainDataFile = _state.DataFile;
        _fileStem = _mainDataFile.Remove(_mainDataFile.Length - 5); // remove .json

        _params.Initialize();
        diagnosticsUI.transform.position = new Vector2(-2200, 0);

        if (_params.schedule.mode == Mode.Sequence || _params.schedule.mode == Mode.CS)
        {
//            _results.Clear();
            _engine.OnFinished = OnFlowchartFinished;
            StartCoroutine(NextBlock());
        }
        else // Adaptation
        {
            _engine.OnFinished = OnAdaptFlowchartFinished;
            _params.adapt.Initialize();
            InitializeProgressBar(_params.adapt.MaxNumberOfBlocks);
            NextBlockOfTracks();
        }
        NextBlock();
    }
    */
    void StartRun()
    {
        //if (IPC.Instance.Use && !_params.bypassIPC && _params.instructions.pages.Count > 0) IPC.Instance.SendCommand("Instructions", "finished");

        //if (_params.flowChart.Count == 0) 
        //    Return();
        //else
        StartCoroutine(StartRunAsync());
    }

    IEnumerator StartRunAsync()
    {
        _params.Initialize();

        InitDataFile();

        _blockNum = 0;
        _nominalNumBlocks = _params.schedule.numBlocks;
        _numSinceLastBreak = 0;
        _numBlocksAdded = 0;
        _resultsByStimulus.Clear();

        // What is the use case for not initializing the SCL?!?
        //if (!(_usingServer && _waitForServer))
        //if (!_usingServer || _waitForServer)
        {
            _state.SetMasterSCL(_params.schedule.CreateStimConList());
            string localName = Path.Combine(Application.persistentDataPath, "scldump.json");
            File.WriteAllText(localName, KLib.FileIO.JSONSerializeToString(_state.MasterSCL));
        }

        if (_params.schedule.mode == Mode.Sequence || _params.schedule.mode == Mode.CS)
        {
            _results.Clear();
            _engine.FlowchartFinished = OnFlowchartFinished;

            StartCoroutine(NextBlock());
        }
        //else if (_params.schedule.mode == Mode.Adapt) // Adaptation
        //{
        //    _engine.OnFinished = OnAdaptFlowchartFinished;
        //    _params.adapt.Initialize();
        //    InitializeProgressBar(_params.adapt.MaxNumberOfBlocks);
        //    NextBlockOfTracks();
        //}
        yield break;
    }
    /*
    public void InitializeProgressBar(int numSteps)
    {
        progressBar.numberOfSteps = numSteps + 1;
        _progressBarStep = 1f / progressBar.numberOfSteps;
        progressBar.value = 0;
    }
    */
    IEnumerator NextBlock()
    {
#if !KDEBUG
        //if (_params.schedule.training && _blockNum == _params.schedule.numBlocks && (_blockNum < _params.schedule.maxPracticeBlocks || _params.schedule.maxPracticeBlocks < 0))
        //{
        //    yield return StartCoroutine(CheckIsTrainingPerformanceOK());
        //    if (!_performanceOK)
        //    {
        //        progressBar.value = 0;

        //        _params.schedule.numBlocks += _nominalNumBlocks;
        //        _params.schedule.AppendNewStimConList(_state.MasterSCL, _nominalNumBlocks);
        //        _state.Save();

        //        ShowMorePracticeInstructions();
        //        yield break;
        //    }
        // }
#else
        yield return null;
#endif

        if (_blockNum < _params.schedule.numBlocks)
        {
            _SCL = _state.MasterSCL.FindAll(o => o.block == _blockNum + 1);
            string localName = Path.Combine(Application.persistentDataPath, "scldump.json");
            File.WriteAllText(localName, KLib.FileIO.JSONSerializeToString(_SCL));

            if (_blockNum == 0)
            {
                //InitializeProgressBar(_params.schedule.numBlocks * _SCL.Count);
            }

            _isRunning = true;
            AdvanceSequence();
        }
        else EndRun(false);
        yield break;
    }

    private void EndRun(bool abort)
    {
        //if (_params.schedule.mode != Mode.Adapt) StoreResults();

        //_audioTimer.StopThread();

        if (_usingServer)
        {
            _engine.ClearScreen();
            //if (!_waitForServer && IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("Run", "finished");
        }
        else
        {
            _engine.ClearScreen();

            //StopIPCRecording();

            _engine.WriteAudioLogFile(_mainDataFile.Replace(".json", ".audio.json"));
            //if (SubjectManager.Instance.UploadData) DataFileManager.UploadDataFile(_mainDataFile);
            //SubjectManager.Instance.DataFiles.Add(_mainDataFile);

            _state.Finish();

            bool finished = true;
            //if (_isScripted)
            //{
            //    TurandotScripter.Instance.Advance();
            //    finished = TurandotScripter.Instance.IsFinished;

            //    if (finished)
            //    {
            //        string linkTo = TurandotScripter.Instance.LinkTo;

            //        GameObject.Destroy(GameObject.Find("TurandotScripter"));
            //        if (!string.IsNullOrEmpty(linkTo))
            //        {
            //            finished = false;
            //            if (DiagnosticsManager.Instance.IsExtraCurricular)
            //                DiagnosticsManager.Instance.MakeExtracurricular("Turandot", System.IO.Path.GetFileNameWithoutExtension(linkTo).Replace("Turandot.", ""));
            //            else
            //                DiagnosticsManager.Instance.SettingsFile = System.IO.Path.GetFileNameWithoutExtension(linkTo).Replace("Turandot.", "");

            //            Application.LoadLevel("Turandot");
            //        }
            //    }
            //    else
            //    {
            //        Application.LoadLevel("Turandot");
            //    }
            //}
            //else if (!string.IsNullOrEmpty(_params.linkTo) && !abort)
            //{
            //    finished = false;
            //    if (_params.linkTo.Equals("return"))
            //    {
            //        DiagnosticsManager.Instance.AdvanceProtocol();
            //        Return();
            //    }
            //    else
            //    {
            //        if (DiagnosticsManager.Instance.IsExtraCurricular)
            //            DiagnosticsManager.Instance.MakeExtracurricular("Turandot", System.IO.Path.GetFileNameWithoutExtension(_params.linkTo).Replace("Turandot.", ""), DiagnosticsManager.Instance.ReturnToScene);
            //        else
            //            DiagnosticsManager.Instance.SettingsFile = System.IO.Path.GetFileNameWithoutExtension(_params.linkTo).Replace("Turandot.", "");
            //        Application.LoadLevel("Turandot");
            //    }
            //}

            if (finished)
            {
                //DiagnosticsManager.Instance.AdvanceProtocol();

                //string msg = "Finished! Press ENTER or tap screen to return.";
                //if (!string.IsNullOrEmpty(_params.screen.finalPrompt))
                //    msg = _params.screen.finalPrompt;

                //_message.color = 0x000800;
                //_message.text = msg;
                //prompt.Activate(_message);

                //_waitingForResponse = true;
                //_waitingForTap = true;
                _finishPanel.SetActive(true);
            }
        }
    }
/* 
    IEnumerator CheckIsTrainingPerformanceOK()
    {
        _performanceOK = false;
        if (_params.schedule.targetPc > 0)
        {
            int nc = _results.FindAll(o => _synonymsForCorrect.Contains(o.ToLower())).Count;
            float pc = (float)nc / _results.Count;
            _performanceOK = (pc >= 0.01f * _params.schedule.targetPc);
            yield break;
        }
        else
        {
            yield return StartCoroutine(diagnosticsUI.AskYesNoQuestion("Getting the hang of it?\nPress NO to practice some more."));
            _performanceOK = diagnosticsUI.YesNoResult;
        }
    }

    void MorePractice()
    {
        diagnosticsUI.transform.position = new Vector2(-2200, 0);
        _results.Clear();
        StartCoroutine(NextBlock());
    }

    void EndBreak()
    {
        diagnosticsUI.transform.position = new Vector2(-2200, 0);
        if (_params.schedule.mode == Mode.Adapt)
        {
            NextBlockOfTracks();
        }
        else
        {
            StartCoroutine(NextBlock());
        }
    }

    void MoreAdaptation()
    {
        diagnosticsUI.transform.position = new Vector2(-2200, 0);
        NextBlockOfTracks();
    }

    void NextBlockOfTracks()
    {
        _params.adapt.InitBlock();
        StartCoroutine(NextTrack());
    }

    IEnumerator NextTrack()
    {
        _isRunning = true;
        AdvanceAdaptation();
        yield break;
    }
*/
    void InitDataFile()
    {
        var fileStemStart = GameManager.Subject;
        if (!string.IsNullOrEmpty(_params.tag))
        {
            fileStemStart += "-" + _params.tag;
        }
        else
        {
            fileStemStart += "-Turandot";
        }
        while (true)
        {
            _fileStem = $"{fileStemStart}-Run{GameManager.GetNextRunNumber("Turandot"):000}";
            _fileStem = Path.Combine(FileLocations.SubjectFolder, _fileStem);
            _mainDataFile = _fileStem + ".json";
            if (!File.Exists(_mainDataFile))
            {
                break;
            }
        }    
        
        FileHeader header = new FileHeader();
        header.Initialize(_mainDataFile, _paramFile);
        header.audioSamplingRate = AudioSettings.outputSampleRate;
        AudioSettings.GetDSPBufferSize(out header.audioBufferLength, out header.audioNumBuffers);

        string json = KLib.FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(header));
        json = KLib.FileIO.JSONStringAdd(json, "params", KLib.FileIO.JSONSerializeToString(_params));
        json += Environment.NewLine;

        File.WriteAllText(_mainDataFile, json);

        _state.SetDataFile(_mainDataFile);
    }
/*
    void Update()
    {
        if (_waitingForResponse && (Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("XboxA")))
        {
            Return();
        }

        if (_waitingForResponse && Input.GetKeyDown(KeyCode.R))
        {
            if (!_haveException)
            {
                _waitingForResponse = false;
                prompt.Deactivate();
                _runAborted = false;
                _isRunning = true;
                AdvanceSequence();
            }
        }

        if (_waitingForResponse && _usingServer && Input.GetKeyDown(KeyCode.S))
        {
            _engine.Abort();
            _engine.ClearScreen();
            prompt.Deactivate();
            _waitingForResponse = false;
        }
    }

    void OnGUI()
    {
        Event e = Event.current;
        if ((_isRunning || _usingServer) && e.control && e.keyCode == KeyCode.A && !_waitingForResponse)
        {
            if (_isRunning) _engine.Abort();
            _isRunning = false;
            _message.text = _haveException ? "Press ENTER or tap screen to quit" : "Paused. Press R to resume or ENTER to quit.";
            prompt.Activate(_message);
            _runAborted = true;
            _waitingForResponse = true;
        }
    }
    */
    void AdvanceSequence()
    {
        _data.NewTrial(_blockNum+1, _SCL[0].trial, _SCL[0].group);
        _data.type = _SCL[0].trialType;

        foreach (FlowElement fe in _params.flowChart)
        {
            fe.Initialize();
        }

        string error = "";
        foreach (PropValPair pv in _SCL[0].propValPairs)
        {
            _data.properties.Add(pv.property + "=" + pv.value);
           Debug.Log(pv.property + "=" + pv.value);
            error = _params.SetParameter(pv.property, pv.value);

            if (!string.IsNullOrEmpty(error))
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(error))
        {
            //HandleError(error);
        }
        else
        {
            string logPath = _fileStem + "-Block" + _data.block + "-Trial" + _data.trial + ".json";
            logPath = Path.Combine(Application.persistentDataPath, "shit.json");
            //if (IPC.Instance.Use && !_params.bypassIPC && !IPC.Instance.SendCommand("Trial", System.IO.Path.GetFileNameWithoutExtension(logPath)))
            //    throw new System.Exception("IPC error: " + IPC.Instance.LastError);

#if !KDEBUG
            StartCoroutine(_engine.ExecuteFlowchart(_SCL[0].trialType, _params.flags, logPath));
#else
            StartCoroutine(_engine.SimulateFlowchart(_SCL[0].trialType, _params.flags, logPath, "Go"));
#endif
        }
    }
    
    void OnFlowchartFinished()
    {
        Debug.Log("Finished!!!!!!!");
        Match p = Regex.Match(_engine.Result, "outcome=\"([\\w\\d\\s]+)\"");
        if (p.Success)
        {
            _results.Add(p.Groups[1].Value);
        }
        else
        {
            _results.Add(_engine.Result);
        }

        _resultsByStimulus.Add(_SCL[0].group, _SCL[0].ix, _SCL[0].iy, _engine.Result);

        _data.result = _engine.Result;
        _data.reactionTime = _engine.ReactionTime;
        _data.properties.AddRange(_params.GetPostTrialProperties(_data.properties));

        File.AppendAllText(_mainDataFile, _data.ToJSONString());

        //if (IPC.Instance.Use && !_params.bypassIPC && !IPC.Instance.SendCommand("Trial End", ""))
        //    throw new System.Exception("IPC error: " + IPC.Instance.LastError);

        if (_engine.FlowchartEndAction != EndAction.None)
        {
            EndRun(_engine.FlowchartEndAction == EndAction.AbortAll);
            return;
        }

        _SCL.RemoveAt(0);

        //progressBar.value += _progressBarStep;
        //if (_usingServer && !_waitForServer && IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("Progress", progressBar.value.ToString());

        if (_SCL.Count > 0)
        {
            AdvanceSequence();
        }
        else
        {
            _isRunning = false;
            _state.SetLastBlock(_blockNum);
            ++_blockNum;

            if (_params.schedule.offerBreakAfter > 0 && ++_numSinceLastBreak == _params.schedule.offerBreakAfter && _blockNum < _params.schedule.numBlocks)
            {
                _numSinceLastBreak = 0;
                _state.CanResume = true;
                _state.Save();
                //ShowBreakInstructions(_params.schedule.breakInstructions);
            }
            else if (!_usingServer)
            {
                StartCoroutine(NextBlock());
            }
        }
    }
    /*
    void StopIPCRecording()
    {
        if (IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.StopRecording();
    }

    void AdvanceAdaptation()
    {
        SCLElement sc = _params.adapt.InitTrial();

        _data.NewTrial(sc.block, sc.track, sc.trial, sc.group);
        _data.type = sc.trialType;

        foreach (FlowElement fe in _params.flowChart)
        {
            fe.Initialize();
        }

        string error = "";
        foreach (PropValPair pv in sc.propValPairs)
        {
            //Debug.Log(pv.property + "=" + pv.value);
            _data.properties.Add(pv.property + "=" + pv.value);
            error = _params.SetParameter(pv.property, pv.value);

            if (!string.IsNullOrEmpty(error))
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(error))
        {
            HandleError(error);
        }
        else
        {
            string logPath = _fileStem + "-Block" + _data.block + "-Track" + _data.track + "-Trial" + _data.trial + ".json";
            if (IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("Trial", _data.trial.ToString());

#if !KDEBUG
            StartCoroutine(_engine.ExecuteFlowchart(sc.trialType, _params.flags, logPath));
#else
            StartCoroutine(_engine.SimulateFlowchart(sc.trialType, _params.flags, logPath, _params.adapt.SimulatedResult(sc.trialType, 11.1f)));
#endif
        }
    }

    void OnAdaptFlowchartFinished()
    {
        if (IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("TrialEnd", _data.trial.ToString());
        Debug.Log("Trial #" + _data.trial + " finished");

        _data.result = _engine.Result;
        _data.reactionTime = _engine.ReactionTime;
        _data.properties.AddRange(_params.GetPostTrialProperties(_data.properties));
        KLib.FileIO.AppendTextFile(_mainDataFile, _data.ToJSONString());

        _params.adapt.Process(_data.result);

        if (_params.adapt.IsBlockFinished)
        {
            progressBar.value += _progressBarStep;

            string json = "";
            foreach (AdaptiveTrack at in _params.adapt.tracks)
            {
                json += KLib.FileIO.JSONStringAdd(json, at.name, KLib.FileIO.JSONSerializeToString(at.History));
            }
            string blockDataFile = _mainDataFile.Replace(".json", "-Block" + _params.adapt.BlockNumber + ".json");
            KLib.FileIO.WriteTextFile(blockDataFile, json);
            if (SubjectManager.Instance.UploadData) DataFileManager.UploadDataFile(blockDataFile);

            string logFile = _mainDataFile.Replace(".json", "-Block" + _params.adapt.BlockNumber + ".log");
            KLib.FileIO.WriteTextFile(logFile, _params.adapt.Log);
//            DataFileManager.UploadDataFile(logFile);

            if (_params.adapt.AllBlocksFinished)
            {
                FinishAdaptation();
            }
            else
            {
                if (_params.schedule.offerBreakAfter > 0 && ++_numSinceLastBreak == _params.schedule.offerBreakAfter)
                {
                    _numSinceLastBreak = 0;
                    ShowBreakInstructions(_params.schedule.breakInstructions);
                }
                else
                {
                    NextBlockOfTracks();
                }
            }
        }
        else
        {
            AdvanceAdaptation();
        }
    }

    void FinishAdaptation()
    {
        if (_params.adapt.CheckTrackConsistency())
        {
            List<TrackResult> final = _params.adapt.ComputeFinalThresholds();
            foreach (AdaptiveTrack at in _params.adapt.tracks.FindAll(t => !string.IsNullOrEmpty(t.storeThresholdAs)))
            {
                if (at.storeThresholdAs == "THR")
                {
                    StoreResultInAudiogram(at.state, at.chan, final.Find(f => f.name == at.name).threshold);
                }
                else
                {
                    SubjectManager.Instance.AddMetric(at.storeThresholdAs, final.Find(f => f.name == at.name).threshold);
                }
            }
            string json = KLib.FileIO.JSONStringAdd("", "trackResults", KLib.FileIO.JSONSerializeToString(_params.adapt.Results));
            KLib.FileIO.AppendTextFile(_mainDataFile, json);

            json = KLib.FileIO.JSONStringAdd("", "finalResults", KLib.FileIO.JSONSerializeToString(final));
            KLib.FileIO.AppendTextFile(_mainDataFile, json);

            StopIPCRecording();

            _isRunning = false;

            _engine.ClearScreen();

            if (SubjectManager.Instance.UploadData) DataFileManager.UploadDataFile(_mainDataFile);
            SubjectManager.Instance.DataFiles.Add(_mainDataFile);

            if (!string.IsNullOrEmpty(_params.linkTo))
            {
                if (DiagnosticsManager.Instance.IsExtraCurricular)
                    DiagnosticsManager.Instance.MakeExtracurricular("Turandot", System.IO.Path.GetFileNameWithoutExtension(_params.linkTo).Replace("Turandot.", ""), DiagnosticsManager.Instance.ReturnToScene);
                else
                    DiagnosticsManager.Instance.SettingsFile = System.IO.Path.GetFileNameWithoutExtension(_params.linkTo).Replace("Turandot.", "");
                Application.LoadLevel("Turandot");
            }
            else
            {
                DiagnosticsManager.Instance.AdvanceProtocol();
                _message.text = "Finished! Press ENTER to return.";
                prompt.Activate(_message);

                _waitingForResponse = true;
            }
        }
        else
        {
            ShowMoreAdaptationInstructions();
        }
    }

    void StartOptimization()
    {
        AdvanceOptimization();
    }

    void AdvanceOptimization()
    {
        SCLElement sc = _params.optimization.InitTrial();

        _data.NewTrial(sc.block, sc.track, sc.trial, sc.group);
        _data.type = sc.trialType;

        foreach (FlowElement fe in _params.flowChart)
        {
            fe.Initialize();
        }

        string error = "";
        foreach (PropValPair pv in sc.propValPairs)
        {
            _data.properties.Add(pv.property + "=" + pv.value);
            error = _params.SetParameter(pv.property, pv.value);

            if (!string.IsNullOrEmpty(error))
            {
                break;
            }
        }

        if (!string.IsNullOrEmpty(error))
        {
            HandleError(error);
        }
        else
        {
            string logPath = _fileStem + "-Trial" + _data.trial + ".json";
            if (IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("Trial", _data.trial.ToString());
#if !KDEBUG
            StartCoroutine(_engine.ExecuteFlowchart(sc.trialType, _params.flags, logPath));
#else
            StartCoroutine(_engine.SimulateFlowchart(sc.trialType, _params.flags, logPath, _params.optimization.Simulate()));
#endif
        }
    }

    void HandleError(string error)
    {
        _message.color = 0xF80000;
        //_message.text = "Unexpected error: " + Environment.NewLine + "Notify the investigator.";
        _message.text = "Error: " + error + Environment.NewLine + "Notify the investigator.";
        Debug.Log(error);

        if (IPC.Instance.Use && _usingServer && _waitForServer)
        {
            try
            {
                IPC.Instance.SendCommand("Error:" + error);
                return;
            }
            catch (Exception ex)
            {
            }
        }

        _runAborted = true;
        prompt.Activate(_message);
        _haveException = true;
        _waitingForResponse = true;
        _waitingForTap = true;
    }
*/

    public void OnFinishButtonClick()
    {
        Return();
    }

    void Return()
    {
        //if (IPC.Instance.Use && !_params.bypassIPC && !_usingServer)
        //{
        //    IPC.Instance.StopRecording();
        //    IPC.Instance.Disconnect();
        //}

        //GameObject.Destroy(GameObject.Find("TurandotScripter"));

        //string returnTo = DiagnosticsManager.Instance.ReturnToScene;
        //if (_runAborted && DiagnosticsManager.Instance.CurrentAutoRun)
        //{
        //    returnTo = ProjectManager.Instance.HasScheduleAssigned ? "Home Scene" : "Backdoor Scene";
        //}

        SceneManager.LoadScene("Home");
    }
/*
    public void HandleException(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Log || type == LogType.Warning || condition.Contains("Capability 'microphone'") || condition.Contains("<RI.Hid>"))
        {
            return;
        }

        try
        {
            _engine.Abort();
        }
        catch { }

        //Debug.Log(condition);
        //Debug.Log(stackTrace);

        ExceptionLog log = new ExceptionLog(condition, stackTrace, type);

        DataFileManager dfm = new DataFileManager();
        dfm.StartDataFile(DataFileType.ExceptionLog);
        dfm.AddToDataJson(log, "Exception");
        dfm.EndDataFile("Exception-" + System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));
        if (SubjectManager.Instance.UploadData) dfm.UploadDataFile();

        HandleError(condition);
    }

    private void StoreResults()
    {
        var data = KLib.FileIO.ReadTextFile(_mainDataFile);

        string trialIndicator = "{\"block\":";
        int splitAt = data.IndexOf(trialIndicator);
        if (splitAt < 0) return;

        data = data.Substring(splitAt);
        List<string> trials = new List<string>(data.Split(new string[] { trialIndicator }, StringSplitOptions.RemoveEmptyEntries));

        string ipcString = "";
        foreach (var f in _params.schedule.families.FindAll(o => !string.IsNullOrEmpty(o.storeResultAs)))
        {
            string expr = "";
            Match p = Regex.Match(f.resultExpression, "{([\\w\\d\\s\\.]+)}");
            string operand = p.Groups[1].Value;

            if (operand == "Pc")
            {
                int nc = _results.FindAll(o => _synonymsForCorrect.Contains(o.ToLower())).Count;
                float pc = (float)nc / _results.Count;
                expr = pc.ToString();
            }
            else if (operand.StartsWith("Fcn."))
            {
                expr = AnalysisFunctions.Evaluate(operand.Substring(4), trials.FindAll(o => o.Contains("\"family\":\"" + f.name)));
            }
            else
            {
                expr = "[";
                foreach (string t in trials.FindAll(o => o.Contains("\"family\":\"" + f.name)))
                {
                    Match r = Regex.Match(t, "\"" + operand + "\":\\[([-\\d\\.\\s]+)\\]"); // what is the use case for this?
                    if (r.Success) expr += r.Groups[1].Value + " ";
                    else
                    {
                        r = Regex.Match(t, "\"" + operand + "\":([-\\d\\.\\s]+)");
                        if (r.Success) expr += r.Groups[1].Value + " ";
                    }
                }
                expr += "]";
            }

            string resultExpr = f.resultExpression.Replace("{" + operand + "}", expr);
            float v = KLib.Expressions.EvaluateToFloatScalar(resultExpr);
            SubjectManager.Instance.AddMetric(f.storeResultAs, v);
            ipcString += f.storeResultAs + "=" + v.ToString() + ";";
        }

        if (!string.IsNullOrEmpty(ipcString) && _usingServer && IPC.Instance.Use && !_params.bypassIPC) IPC.Instance.SendCommand("Store", ipcString);
    }
    */
    void IRemoteControllable.ChangeScene(string newScene)
    {

    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {

    }

    /* 

    public void RpcLoadParameters(string configName)
    {
        string localName = DataFileLocations.ConfigFile("Turandot", configName);
        _params = KLib.FileIO.XmlDeserialize<Parameters>(localName);

        _paramFile = "Turandot TCP Server";
        _params.CheckParameters();
        _params.ApplyDefaultWavFolder(SubjectManager.Instance.Project);

        Debug.Log(_params.schedule.numBlocks);
        _state = new TurandotState(SubjectManager.Instance.Project, SubjectManager.CurrentSubject, configName);
    }

    public void RpcSetParameters(string xml)
    {
        _params = KLib.FileIO.XmlDeserializeFromString<Parameters>(xml);
        _paramFile = "Turandot TCP Server";
        _params.CheckParameters();
        _params.ApplyDefaultWavFolder(SubjectManager.Instance.Project);

        _state = new TurandotState(SubjectManager.Instance.Project, SubjectManager.CurrentSubject, "fromServer");
        _state.Save();
    }

    public void RpcStart(bool wait)
    {
        _waitForServer = wait;

        _engine.Initialize(_params, SubjectManager.Instance.Transducer, SubjectManager.Instance.MaxLevelMargin);

        if (_params.instructions.pages.Count > 0)
        {
            StartCoroutine(ShowInstructions());
        }
        else
        {
            StartRun();
        }
    }

    public void RpcAddBlock(string expr)
    {
        _params.schedule.families.Clear();

        var f = new Family("TCPServer");
        foreach (var e in expr.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string line = e.Replace("\n", "").Replace("\r", "");

            var parts = line.Split(new char[] { '=' });
            if (parts.Length == 2)
            {
                var subvar = parts[0].Split(new char[] { '.' });
                if (subvar.Length > 2)
                {
                    Variable v = new Variable(subvar[0], subvar[1], subvar[2].Trim());
                    for (int k = 3; k < subvar.Length; k++) v.property += "." + subvar[k].Trim();
                    v.expression = parts[1];
                    f.variables.Add(v);
                }
            }
        }

        f.number = 1;
        _params.schedule.families.Add(f);

        if (_state.MasterSCL == null)
        {
            _params.schedule.numBlocks = 1;
            _state.SetMasterSCL(_params.schedule.CreateStimConList());
        }
        else
        {
            _params.schedule.numBlocks++;
            _params.schedule.AppendNewStimConList(_state.MasterSCL, 1);
        }

        _state.Save();
    }

    public void RpcMessage(string msg)
    {
        _message.text = msg;
        prompt.Activate(_message);
    }

    public void RpcNextBlock()
    {
        prompt.Deactivate();
        _engine.ClearScreen();
        StartCoroutine(NextBlock());
    }

    public void RpcAbort()
    {
        _isRunning = false;
        _engine.Abort();
        _engine.ClearScreen();
        _blockNum++;
        if (!_waitForServer && IPC.Instance.Use) IPC.Instance.SendCommand("Run", "aborted");
    }

    public void RpcShowState(string state)
    {
        _engine.ShowState(_params, state);
    }

    public void RpcExit()
    {
        Return();
    }
*/
}
