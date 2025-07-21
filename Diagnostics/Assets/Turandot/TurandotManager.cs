using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;

using KLib.Signals.Waveforms;
using Turandot;
using Turandot.Schedules;
using Turandot.Scripts;
using Turandot.Screen;
using UnityEngine.Video;

public class TurandotManager : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private Camera _camera;
    [SerializeField] private TurandotEngine _engine;
    [SerializeField] private GameObject _titleBar;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;

    [SerializeField] private VideoPlayer _videoPlayer;

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
    bool _isRemote = false;

    string _paramFile = "";

    int _nominalNumBlocks;
    int _blockNum;
    int _numSinceLastBreak;

    float _progressBarStep = 1f;
    float _progress = 0;

    bool _runAborted = false;

    bool _performanceOK = false;

    List<string> _results = new List<string>();
    Results _resultsByStimulus = new Results();
    int _numBlocksAdded = 0;

    List<string> _synonymsForCorrect = new List<string>(new string[] { "hit", "withhold", "go", "right", "correct" });

    void Awake()
    {
        Application.logMessageReceived += HandleException;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleException;
    }

    void Start()
    {
        HTS_Server.SetCurrentScene("Turandot", this);

#if HACKING
        Application.targetFrameRate = 60;
        GameManager.SetSubject("Scratch/_shit");
        string configName = "checklist";
        //DiagnosticsManager.Instance.MakeExtracurricular("Turandot", "Turandot." + configName);
#else
        string configName = GameManager.DataForNextScene;
#endif

        if (string.IsNullOrEmpty(configName))
        {
            _isRemote = HTS_Server.RemoteConnected;
            if (!_isRemote)
            {
                ShowFinishPanel("Nothing to do");
            }
        }

        _engine.ClearScreen();

        _isScripted = (GameObject.Find("TurandotScripter") != null);

        KLib.Expressions.Metrics = GameManager.Metrics;
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
        if (!_isRemote)
        {
            localName = FileLocations.ConfigFile("Turandot", configName);
            _params = KLib.FileIO.XmlDeserialize<Parameters>(localName);
            _paramFile = Path.GetFileName(localName);
            _state = new TurandotState(GameManager.Project, GameManager.Subject, configName);

            ApplyParameters();
            Begin();
        }

    }
    
    private void ApplyParameters()
    {
        string filename = "";

        try
        {
            _params.ApplyDefaultWavFolder(GameManager.Project);

            //try
            {
                _engine.Initialize(_params);
            }
            //catch (Exception ex)
            //{
            //    //HandleException(ex.Message, ex.StackTrace, LogType.Exception);
            //    return;
            //}

            if (_params.screen.ApplyCustomScreenColor)
            {
                _camera.backgroundColor = GameManager.BackgroundColor;
                if (HardwareInterface.LED.IsInitialized)
                {
                    HardwareInterface.LED.SetColorFromString(GameManager.LEDColorString);
                    HTS_Server.SendMessage("ChangedLEDColors", GameManager.LEDColorString);
                }
            }

            _params.Initialize();

            InitDataFile();
            filename = Path.GetFileName(_mainDataFile);
        }
        catch (Exception ex)
        {
            filename = "error";
            HandleException(ex.Message, ex.StackTrace, LogType.Exception);
            _engine.ClearScreen();
        }
        finally
        {
            HTS_Server.SendMessage("Turandot", $"File:{filename}");
        }
    }

    private void Begin()
    {
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
            _titleBar.SetActive(false);
            StartRun();
        }
    }

    IEnumerator ShowInstructions()
    {
        HTS_Server.SendMessage("Turandot", "State:Instructions");

        yield return new WaitForSeconds(1);
        _titleBar.SetActive(false);

        if (_haveException) yield break;

        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = OnInstructionsFinished;
        _instructionPanel.ShowInstructions(_params.instructions);
    }
    private void OnInstructionsFinished()
    {
        _instructionPanel.gameObject.SetActive(false);

        if (_params.flowChart.Count > 0)
        {
            StartRun();
        }
        else
        {
            EndRun(abort: false);
        }
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
        _blockNum = 0;
        _nominalNumBlocks = _params.schedule.numBlocks;
        _numSinceLastBreak = 0;
        _numBlocksAdded = 0;
        _resultsByStimulus.Clear();

        _state.SetMasterSCL(_params.schedule.CreateStimConList());
        string localName = Path.Combine(Application.persistentDataPath, "scldump.json");
        File.WriteAllText(localName, KLib.FileIO.JSONSerializeToString(_state.MasterSCL));

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
    
    public void InitializeProgressBar(int numSteps)
    {
        _progressBarStep = 1f / numSteps;
        _progress = 0;
    }
    
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
                InitializeProgressBar(_params.schedule.numBlocks * _SCL.Count);
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

        _engine.ClearScreen();

        var audioFile = _mainDataFile.Replace(".json", ".audio.json");
        _engine.WriteAudioLogFile(audioFile);

        HTS_Server.SendMessage("Turandot", "Finished:");
        HTS_Server.SendMessage("Turandot", $"ReceiveData:{Path.GetFileName(_mainDataFile)}:{File.ReadAllText(_mainDataFile)}");
        HTS_Server.SendMessage("Turandot", $"ReceiveData:{Path.GetFileName(audioFile)}:{File.ReadAllText(audioFile)}");

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

        if (finished && !_isRemote)
        {
            //DiagnosticsManager.Instance.AdvanceProtocol();

            ShowFinishPanel();
        }
    }

    private void ShowFinishPanel(string message = "")
    {
        _titleBar.SetActive(true);
        _finishText.text = message;
        _finishPanel.SetActive(true);
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
        
        var header = new Turandot.FileHeader();
        header.Initialize(_mainDataFile, _paramFile);
        header.audioSamplingRate = AudioSettings.outputSampleRate;
        AudioSettings.GetDSPBufferSize(out header.audioBufferLength, out header.audioNumBuffers);
        if (_params.screen.ApplyCustomScreenColor)
        {
            header.screenColor = GameManager.ScreenColorString;
            if (HardwareInterface.LED.IsInitialized)
            {
                header.ledColor = GameManager.LEDColorString;
            }
        }

        string json = KLib.FileIO.JSONStringAdd("", "info", KLib.FileIO.JSONSerializeToString(header));
        json = KLib.FileIO.JSONStringAdd(json, "params", KLib.FileIO.JSONSerializeToString(_params));
        json += Environment.NewLine;

        File.WriteAllText(_mainDataFile, json);

        _state.SetDataFile(_mainDataFile);
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A && !_waitingForResponse)
        {
            if (_isRunning) _engine.Abort();
            _isRunning = false;
            _waitingForResponse = true;
            _quitPanel.SetActive(true);
        }
    }

    public void OnQuitConfirmButtonClick()
    {
        HTS_Server.SendMessage("Turandot", "Error:Quit");
        SceneManager.LoadScene("Home");
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _waitingForResponse = false;
        AdvanceSequence();
    }

    void AdvanceSequence()
    {
        _data.NewTrial(_blockNum+1, _SCL[0].trial, _SCL[0].group);
        _data.type = _SCL[0].trialType;

        foreach (FlowElement fe in _params.flowChart)
        {
            fe.Initialize();
        }

        string error = "";
        var stringBuilder = new StringBuilder(100);
        stringBuilder.AppendLine($"Block = {_data.block}");
        stringBuilder.AppendLine($"Trial = {_data.trial}");
        stringBuilder.AppendLine("----------------------");
        foreach (PropValPair pv in _SCL[0].propValPairs)
        {
            stringBuilder.AppendLine($"{pv.property}={pv.value}");
            _data.properties.Add($"{pv.property}={pv.value}");
            error = _params.SetParameter(pv.property, pv.value);

            if (!string.IsNullOrEmpty(error))
            {
                break;
            }
        }

        //if (_data.trial == 2) error = "Kluge to force error";

        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError($"{error}");
        }
        else
        {
            string logPath = _fileStem + "-Block" + _data.block + "-Trial" + _data.trial + ".json";

            HTS_Server.SendMessage("Turandot", $"Trial:{stringBuilder.ToString()}");

#if !KDEBUG
            StartCoroutine(_engine.ExecuteFlowchart(_SCL[0].trialType, _params.flags, logPath));
#else
            StartCoroutine(_engine.SimulateFlowchart(_SCL[0].trialType, _params.flags, logPath, "Go"));
#endif
        }
    }
    
    void OnFlowchartFinished()
    {
        Cursor.visible = true;
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

        File.AppendAllText(_mainDataFile, _data.ToJSONString(_engine.GetEventsAsJSON()));

        HTS_Server.SendMessage("Turandot", "State:Finished");

        if (_engine.FlowchartEndAction != EndAction.None)
        {
            EndRun(_engine.FlowchartEndAction == EndAction.AbortAll);
            return;
        }

        _SCL.RemoveAt(0);

        _progress += _progressBarStep;
        HTS_Server.SendMessage("Turandot", $"Progress:{Mathf.RoundToInt(_progress * 100)}");

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
            else
            {
                StartCoroutine(NextBlock());
            }
        }
    }

      /*
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

    public void HandleException(string condition, string stackTrace, LogType type)
    {
        if (type == LogType.Log || type == LogType.Warning) // || condition.Contains("Capability 'microphone'") || condition.Contains("<RI.Hid>"))
        {
            return;
        }

        try
        {
            _engine.Abort();
            _engine.ClearScreen();
        }
        catch { }

        //ExceptionLog log = new ExceptionLog(condition, stackTrace, type);

        //DataFileManager dfm = new DataFileManager();
        //dfm.StartDataFile(DataFileType.ExceptionLog);
        //dfm.AddToDataJson(log, "Exception");
        //dfm.EndDataFile("Exception-" + System.DateTime.Now.ToString("yyyy-MM-dd_HHmmss"));
        //if (SubjectManager.Instance.UploadData) dfm.UploadDataFile();

        HandleError(condition);
    }

    void HandleError(string error, string stackTrace = "")
    {
        HTS_Server.SendMessage("Turandot", $"Error:{error}");

        _runAborted = true;
        _haveException = true;

        if (!_isRemote)
        {
            ShowFinishPanel("The run was stopped because of an error");
        }
    }

    /*
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
        if (!newScene.Equals("Turandot"))
        {
            SceneManager.LoadScene(newScene);
        }
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        switch (command)
        {
            case "SetParams":
                RpcSetParameters(data);
                break;
            case "StartSynchronizing":
                HardwareInterface.ClockSync.StartSynchronizing(Path.GetFileName(data));
                break;
            case "StopSynchronizing":
                HardwareInterface.ClockSync.StopSynchronizing();
                break;
            case "Begin":
                Begin();
                break;
            case "Abort":
                RpcAbort();
                break;
            //case "SendSyncLog":
            //    var logPath = HardwareInterface.ClockSync.LogFile;
            //    if (!string.IsNullOrEmpty(logPath))
            //    {
            //        HTS_Server.SendMessage("Turandot", $"ReceiveData:{Path.GetFileName(logPath)}:{File.ReadAllText(logPath)}");
            //    }
            //    break;
        }
    }

    public void RpcSetParameters(string xml)
    {
        _params = KLib.FileIO.XmlDeserializeFromString<Parameters>(xml);
        _paramFile = "remote";
        _state = new TurandotState(GameManager.Project, GameManager.Subject, "remote");
        _state.Save();

        ApplyParameters();
    }
    
    public void RpcAbort()
    {
        _isRunning = false;
        if (_instructionPanel.gameObject.activeSelf)
        {
            _instructionPanel.gameObject.SetActive(false);
            HTS_Server.SendMessage("Turandot", "Finished:Run stopped by user");
        }
        else
        {
            _engine.Abort();
            EndRun(abort: true);
        }
    }

}
