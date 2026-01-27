using UnityEngine;
using System;
using System.Collections;

#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

using ExtensionMethods;
using KLib;
using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;
using SpeechReception;
//using SRI.Messages;

public class SpeechReceptionInterface : MonoBehaviour
{
/*    private UILabel _message;

    private AudioSource _audioPlay;
    private SpeechMasker _masker;
    private UILabel _prompt;
    private GameObject _ear;
    private GameObject _banner;
    private GameObject _topFill;
    private GameObject _bottomFill;
    private ClosedSetController _closedSetController;

    private string _transducer;

    private Test _speechTest;
    private ListProperties _speechList;

    private float _level;
    private float _currentSNR;
    private bool _continuousMasker;
    private int _curItem;
    private int _numItems;
    private int _lastPlayedItem;

    private UserCustomizations _customizations;

    private bool _ignoreEvents = false;
    private VolumeManager _volumeManager;

    private float _maxSpeechLevel;
    private float _maxMaskerLevel;
    private float _minSNR;

    private SpeechPupilConfiguration _pupilSettings = new SpeechPupilConfiguration();
    private SRIFixationPoint _fixationPoint;

    private bool _waitingForResponse = false;

    private TestState _state = new TestState();
    private string _speechFilenameFormat;

    private Exception _lastException = null;

    private const int _remotePort = 4951;

    public string RemoteIPAddress { set; get; }

    public int _qnum;
    public bool _useClosedSet;
    private float _volumeAtten;

    private bool _quitTest;
    private bool _tapRestarts = false;
    private bool _addressShowing = false;

    void Awake()
    {
        _message = GameObject.Find("Message").GetComponent<UILabel>();
        _audioPlay = GetComponent<AudioSource>();
        _masker = GameObject.Find("Masker").GetComponent<SpeechMasker>();
        _prompt = GameObject.Find("Prompt").GetComponent<UILabel>();
        _ear = GameObject.Find("Ear Sprite");
        _banner = GameObject.Find("Banner");
        _topFill = GameObject.Find("Filler/Top");
        _bottomFill = GameObject.Find("Filler/Bottom");
        _closedSetController = GameObject.Find("Closed Set").GetComponent<ClosedSetController>();

        _fixationPoint = GameObject.Find("Fixation Point").GetComponent<SRIFixationPoint>();
    }

    void Start()
    {
        _message.text = "";
        NGUITools.SetActive(_prompt.gameObject, false);
        NGUITools.SetActive(_ear, false);

        _closedSetController.OnSelectionMade = OnClosedSetResponse;

#if CONFIG_HACK
        SubjectManager.Instance.ChangeSubject("CISpeech", "_Test");
#endif
        _customizations = UserCustomizations.Initialize(DataFileLocations.SubjectCustomSpeechPath, SubjectManager.CurrentSubject);

        _transducer = SubjectManager.Instance.Transducer;

        if (_customizations.keepBluetoothAlive)
        {
            GameObject.Find("Bluetooth Keep Alive").GetComponent<SpeechReceptionBluetoothKeepAlive>().KeepAlive(_customizations.keepAliveAtten);
        }

        _volumeManager = new VolumeManager();

        SRITCP.Instance.StartServer(this);
    }

    void OnGUI()
    {
        Event e = Event.current;
        if (e.control && !_waitingForResponse && e.keyCode == KeyCode.A)
        {
            _tapRestarts = false;
            _message.text = "Press ENTER or tap screen to quit.";
            _waitingForResponse = true;
        }
    }

    void Update()
    {
        if (_waitingForResponse && (Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)))
        {
            if (_tapRestarts)
            {
                Application.LoadLevel("SpeechReceptionInterface");
            }
            else
            {
                Return();
            }
        }
        else if (Input.GetKeyDown(KeyCode.I) && (Input.GetKey(KeyCode.LeftControl)))
        {
            if (_addressShowing)
            {
                _message.text = "";
            }
            else
            {
                _message.text = "listening on " + SRITCP.Instance.Host;
            }
            _addressShowing = !_addressShowing;
        }
    }

    public void ShowMessage(string s)
    {
        _message.text = s;
    }

    private void ApplyPupilSettings()
    {
        Color bgc = _pupilSettings.active ? KLib.Unity.ColorFromARGB(_pupilSettings.backgroundColor) : new Color(206f / 255f, 206f / 255f, 206f / 255f);
        transform.GetComponent<Camera>().backgroundColor = bgc;

        NGUITools.SetActive(_banner, !_pupilSettings.active);
        NGUITools.SetActive(_topFill, !_pupilSettings.active);
        NGUITools.SetActive(_bottomFill, !_pupilSettings.active);

        IPC.Instance.Use = _pupilSettings.active;

        if (_pupilSettings.active)
        {
            _fixationPoint.Initialize(KLib.Unity.ColorFromARGB(_pupilSettings.fixationColor), _pupilSettings.fixationSize);
        }
    }

    public SubjectInfo GetSubjectInfo()
    {
        var info = new SubjectInfo(SubjectManager.CurrentSubject);
        return info;
    }

    public SRI.Messages.Resources EnumerateResources()
    {
        string configFolder = DataFileLocations.LocalResourceFolder(SubjectManager.Instance.Project, "Config Files");
        if (!Directory.Exists(configFolder)) return null;

        var result = new SRI.Messages.Resources();

        string[] listFiles = Directory.GetFiles(configFolder, "CustomizeSpeech.*.xml");
        if (listFiles != null && listFiles.Length > 0)
        {
            foreach (string f in listFiles)
            {
                string[] parts = f.Split('.');
                if (parts.Length == 3)
                {
                    var test = FileIO.XmlDeserialize<SpeechReception.Test>(f);
                    result.tests.Add(parts[1]);
                }
            }
        }

        result.maskers.Add("Auditech4T");

        return result;
    }

    public bool SetTestLevels(float level, float[] snrs)
    {
        bool success = true;

        try
        {
            _level = level;
            _state.level = level;
            _state.testSNRs = snrs;
            _customizations.Set(_speechList.TestType, _level, snrs);
            FileIO.XmlSerialize(_customizations, DataFileLocations.SubjectCustomSpeechPath);
        }
        catch (Exception ex)
        {
            _lastException = ex;
            success = false;
            Debug.Log(ex.Message);
            Debug.Log(ex.StackTrace);
        }

        return success;
    }

    public bool SetPupilSettings(SpeechPupilConfiguration settings)
    {
        _pupilSettings = settings;
        ApplyPupilSettings();
        if (IPC.Instance.Use)
        {
            StartCoroutine(PingTabletInterface());
        }
        return true;
    }

    private IEnumerator PingTabletInterface()
    {
        yield return null;

        var success = IPC.Instance.Ping();
        if (!success)
        {
            SendMessageAndBytesToPC("error", SRI.Messages.Message.ToProtoBuf(new ErrorDescription("Tablet failed to ping Tablet Interface")));
        }
    }

    public void Return()
    {
        SRITCP.Instance.StopServer();
        if (_pupilSettings.active)
        {
            IPC.Instance.Use = false;
        }

        Application.LoadLevel("Backdoor Scene");
    }

    public TestState InitializeTest(string name)
    {
        bool success = true;

        try
        {
            var configPath = DataFileLocations.ConfigFile("CustomizeSpeech", name);

            _speechTest = FileIO.XmlDeserialize<Test>(configPath);
            _speechList = _speechTest.Lists[0];
            _speechFilenameFormat = _speechList.file;

            _maxSpeechLevel = Mathf.FloorToInt(_speechTest.GetReference(_transducer, SpeechReception.LevelUnits.dBSPL));

            _state.testName = name;
            _state.testType = _speechTest.TestType;
            _state.numLists = _speechTest.NumLists;
            _state.maxSpeechLevel = _maxSpeechLevel;
            _state.isClosed = _speechList.closedSet != null && _speechList.closedSet.active;

            var c = _customizations.Get(_speechTest.TestType);
            if (c != null)
            {
                _level = c.level;
                _state.testSNRs = c.snr;
                _state.level = c.level;
                _speechList.level = _level;
            }
            else
            {
                _state.testSNRs = null;
            }

            success = InitializeList(1, true) != null;

            if (success) InitializeMasker();
        }
        catch (Exception ex)
        {
            _lastException = ex;
            success = false;
            Debug.Log(ex.Message);
            Debug.Log(ex.StackTrace);
        }

        return success ? _state : null;
    }

    public TestState InitializeRandomList(bool updateLevel)
    {
        int listIndex = -1;
        int listNumber = _speechTest.SelectRandomList(out listIndex);
        var state = InitializeList(listNumber, updateLevel);
        _speechList.listIndex = listIndex;

        return state;
    }

    public TestState InitializeList(int listNumber, bool updateLevel)
    {
        bool success = true;

        try
        {
            _speechList.file = String.Format(_speechFilenameFormat, listNumber);

            _speechList.Initialize(_state.testType, Laterality.Unspecified, "");
            _speechList.SetSequence();

            _state.items.Clear();
            foreach (var s in _speechList.sentences)
            {
                _state.items.Add(s.whole);
            }

            if (updateLevel)
            {
                _level = _speechList.level;
                _currentSNR = 0;
            }

            _curItem = 1;
            _lastPlayedItem = 0;
            _numItems = _speechList.sentences.Count;
            _continuousMasker = false;

            _state.listNum = listNumber;
            _state.itemNum = _curItem;
            _state.level = _level;
            _state.snr = _currentSNR;
        }
        catch (Exception ex)
        {
            _lastException = ex;
            success = false;
            Debug.Log(ex.Message);
            Debug.Log(ex.StackTrace);
        }

        return success ? _state : null;
    }
    
    public void SetLevels(float level, float snr)
    {
        _level = level;
        _currentSNR = snr;

        _state.level = _level;
        _state.snr = snr;

        UpdateSNR();

        if (_continuousMasker) _masker.SetLevel(_level - _currentSNR);
    }

    public void SetMaskerState(bool continuous)
    {
        _continuousMasker = continuous;
        _state.continuousMasker = continuous;

        if (continuous)
        {
            _volumeManager.SetMasterVolume(1.0f, VolumeManager.VolumeUnit.Scalar);
            _masker.SetLevel(_level - _currentSNR);
            _masker.Play();
        }
        else
        {
            _masker.Stop();
        }
    }

    private void InitializeMasker()
    {
        _state.masker = _speechList.masker.Source;

        SpeechReception.References r = new SpeechReception.References(FileIO.CombinePaths(DataFileLocations.SpeechWavFolder, "Maskers"), _speechList.masker.Source);
        _maxMaskerLevel = Mathf.Floor(r.GetReference(_transducer, "dBSPL"));

        _state.maxMaskerLevel = _maxMaskerLevel;

        UpdateSNR();

        StartCoroutine(_masker.Initialize(
            _speechList.masker,
            _level - _currentSNR,
            _transducer,
            _speechList.units,
            _speechList.laterality));
    }
    
    private void UpdateSNR()
    {
        if (!_speechList.masker.Source.Equals("None"))
        {
            _minSNR = _level - _maxMaskerLevel;
            _currentSNR = Mathf.Max(_currentSNR, _minSNR);

            _state.maxMaskerLevel = _maxMaskerLevel;
            _state.minSNR = _minSNR;
            _state.snr = _currentSNR;
        }
    }
    
    public void PlayItem(int number)
    {
        ApplyLevel(_level, _speechList.units, _speechList.laterality);
        _curItem = number;
        _state.itemNum = _curItem;
        StartCoroutine(PlayToken());
    }

    private IEnumerator PlayToken()
    {
        string wavfile = _speechList.sentences[_curItem - 1].wavfile;

        Debug.Log(_curItem + ": " + wavfile);
        Debug.Log("Level =" + _level + "; SNR = " + _currentSNR);

        WWW www = new WWW("file:///" + FileIO.CombinePaths(DataFileLocations.SpeechWavFolder, _speechList.TestType, wavfile));
        //WWW www = new WWW("file:///" + System.IO.Path.Combine(DataFileLocations.SpeechWavFolder, wavfile));
        while (!www.isDone)
            yield return null;

        if (!_speechList.masker.Source.ToLower().Equals("none") && !_continuousMasker)
        {
            _masker.SetLevel(_level - _currentSNR);
            _masker.Play();
        }

        _audioPlay.clip = www.GetAudioClip(true, false, AudioType.WAV);

        _audioPlay.Play();
        yield return new WaitForSeconds(_audioPlay.clip.length);

        if (!_speechList.masker.Source.ToLower().Equals("none") && !_continuousMasker)
            _masker.Stop();

        SendMessageToPC("play finished");
    }

    private float ApplyLevel(float level, SpeechReception.LevelUnits units, Laterality laterality)
    {
        float dB_wavfile_fullscale = float.NaN;

        float ref_dB = _speechTest.GetReference(_transducer, units);

        dB_wavfile_fullscale = ref_dB;
        Debug.Log("dB_wavfile_fullscale" + " = " + dB_wavfile_fullscale);

        float atten = level - dB_wavfile_fullscale;
        Debug.Log("atten" + " = " + atten);
        atten = Mathf.Min(0f, atten);

        _audioPlay.volume = Mathf.Pow(10, atten / 20);
        _audioPlay.panStereo = laterality.ToBalance();

        _audioPlay.volume = Mathf.Pow(10, atten / 20);
        atten = 0;

        _volumeManager.SetMasterVolume(atten, VolumeManager.VolumeUnit.Decibel);

        return atten;
    }

    private void SendMessageToPC(string message)
    {
        var client = new Sockets.KTcpClient();
        client.ConnectTCPServer(RemoteIPAddress, _remotePort);
        client.WriteBinary(message);
        client.CloseTCPServer();
    }

    private void SendMessageAndBytesToPC(string message, byte[] data)
    {
        var client = new Sockets.KTcpClient();
        client.ConnectTCPServer(RemoteIPAddress, _remotePort);
        client.WriteBinary(message);
        client.WriteBinary(data);
        client.CloseTCPServer();
    }

    public void StartTest(int startFrom)
    {
        StartCoroutine(StartTestAsync(startFrom));
    }

    private IEnumerator StartTestAsync(int startFrom)
    {
        if (IPC.Instance.Use)
        {
            var success = IPC.Instance.Ping();
            if (!success)
            {
                SendMessageAndBytesToPC("error", SRI.Messages.Message.ToProtoBuf(new ErrorDescription("Tablet failed to ping Tablet Interface")));
                yield break;
            }
        }

        _quitTest = false;

        _useClosedSet = _speechList.closedSet != null && _speechList.closedSet.active;
        if (_useClosedSet)
        {
            _closedSetController.Initialize(_speechList.closedSet, _speechList.GetClosedSetResponses());
        }

        _speechList.level = _level;
        _speechList.SNRs = new float[] { _currentSNR };
        _speechList.SetSequence();

        //_volumeAtten = SetLevel(_speechList.level, _speechList.units, _speechList.laterality);
        ApplyLevel(_level, _speechList.units, _speechList.laterality);

        if (_pupilSettings.active)
        {
            //_message.text = "initializing...";
            //yield return null;

            _speechTest.MinDelay_s = _pupilSettings.stimDelay_s;
            _speechTest.MaxDelay_s = _pupilSettings.stimDelay_s;
            _speechTest.SentenceDuration_s = _pupilSettings.sentenceDuration_s;
            _speechTest.MaskerTail_s = _pupilSettings.maskerTail_s;

            //_message.text = "";
            //yield return null;
        }

        if (IPC.Instance.Use)
        {
            IPC.Instance.StartRecording(SubjectManager.CurrentSubject + "-" + _speechList.file);
            IPC.Instance.SendCommand("State", "0");

            yield return new WaitForSeconds(1f);
        }

        _qnum = startFrom - 1;
        if (_speechList.UseMasker)
        {
            yield return StartCoroutine(_masker.Initialize(
                _speechList.masker,
                _speechList.level - _speechList.sentences[_qnum].SNR,
                _transducer,
                _speechList.units,
                _speechList.laterality));
        }

        DoNextTrial();
    }

    public void DoNextTrial()
    {
        var item = new CurrentItem(_speechList.sentences[_qnum].whole, _speechList.sentences[_qnum].words);
        item.num = _qnum;
        item.total = _speechList.sentences.Count;
        SendMessageAndBytesToPC("item", SRI.Messages.Message.ToProtoBuf(item));

        StartCoroutine(DoSentence());
    }

    public void EndTest()
    {
        _fixationPoint.Hide();
        _quitTest = true;
        if (IPC.Instance.Use) IPC.Instance.StopRecording();
    }

    private float SetLevel(float level, SpeechReception.LevelUnits units, Laterality laterality)
    {
        float dB_wavfile_fullscale = float.NaN;

        float ref_dB = _speechTest.GetReference(_transducer, units);
        //float ref_dB = _speechTest.GetReference(units);

        dB_wavfile_fullscale = ref_dB;
        Debug.Log("dB_wavfile_fullscale" + " = " + dB_wavfile_fullscale);

        float atten = level - dB_wavfile_fullscale;
        Debug.Log("atten" + " = " + atten);
//        atten = Math.Min(0f, atten);
        atten = 0;

        _volumeManager.SetMasterVolume(atten, VolumeManager.VolumeUnit.Decibel);

        return atten;
    }

    private IEnumerator DoSentence()
    {
        if (_pupilSettings.active)
        {
            _fixationPoint.ShowCross();
        }
        else
        {
            NGUITools.SetActive(_ear, true);
            NGUITools.SetActive(_prompt.gameObject, true);
            _prompt.text = "Listen";
        }

        string wavfile = _speechList.sentences[_qnum].wavfile;

        Debug.Log(_qnum + ": " + wavfile);

        WWW www = new WWW("file:///" + FileIO.CombinePaths(DataFileLocations.SpeechWavFolder, _speechTest.TestType, wavfile));
        //WWW www = new WWW("file:///" + System.IO.Path.Combine(DataFileLocations.SpeechWavFolder, wavfile));
        while (!www.isDone)
            yield return null;

        _audioPlay.clip = www.GetAudioClip(false, false, AudioType.WAV);

        if (IPC.Instance.Use) IPC.Instance.SendCommand("Sentence", _qnum.ToString());

        if (_pupilSettings.active)
        {
            yield return new WaitForSeconds(_pupilSettings.pupilPreBaseline_s);
        }

        if (_speechList.UseMasker)
        {
            Debug.Log("SNR = " + _speechList.sentences[_qnum].SNR);
            _masker.SetLevel(_speechList.level - _speechList.sentences[_qnum].SNR);
            if (IPC.Instance.Use) IPC.Instance.SendCommand("MaskerOn", _qnum.ToString());
            _masker.Play();
        }

        float frontPad = _speechTest.SentenceDuration_s > 0 ? _speechTest.SentenceDuration_s - _audioPlay.clip.length : 0;

        if (_speechTest.MaxDelay_s >= 0 || frontPad > 0)
        {
            float delay_s = _speechTest.MaxDelay_s > 0 ? Expressions.UniformRandomNumber(_speechTest.MinDelay_s, _speechTest.MaxDelay_s) : 0;
            yield return new WaitForSeconds(delay_s + frontPad);
        }

        // Sentence
        _volumeManager.SetMasterVolume(_volumeAtten, VolumeManager.VolumeUnit.Decibel);

        if (IPC.Instance.Use) IPC.Instance.SendCommand("SentenceStart", _qnum.ToString());
        _audioPlay.Play();
        yield return new WaitForSeconds(_audioPlay.clip.length);
        if (IPC.Instance.Use) IPC.Instance.SendCommand("SentenceEnd", _qnum.ToString());


        // Post-sentence baseline
        if (_speechList.UseMasker)
        {
            if (_speechTest.MaskerTail_s > 0)
            {
                yield return new WaitForSeconds(_speechTest.MaskerTail_s);
                _masker.Stop();
                if (IPC.Instance.Use) IPC.Instance.SendCommand("MaskerOff", _qnum.ToString());
            }
            else
            {
                _masker.Stop();
            }
        }

        if (_pupilSettings.active)
        {
            yield return new WaitForSeconds(_pupilSettings.pupilPostBaseline_s);
            _fixationPoint.ShowCircle();
        }
        else
        {
            NGUITools.SetActive(_ear, false);
            _prompt.text = "What did you hear?";
        }

        if (IPC.Instance.Use) IPC.Instance.SendCommand("Response", _qnum.ToString());
        if (_useClosedSet)
        {
            _closedSetController.Show();
        }
        else
        {
            ResponseAcquired("", false);
        }
    }

    private void OnClosedSetResponse(string value)
    {
        bool volumeChanged = _volumeManager.GetMasterVolume(VolumeManager.VolumeUnit.Decibel) != _volumeAtten;
        Debug.Log("Response = " + value);

        _prompt.text = "";
        _closedSetController.Hide();

        ResponseAcquired(value, value.Equals(_speechList.sentences[_qnum].whole));
    }

    public void ResponseAcquired(string response, bool correct)
    {
        _prompt.text = "";
        _qnum++;

        bool finished = _qnum >= _speechList.sentences.Count || _quitTest;
        var userResponse = new UserResponse(response, correct, finished);

        SendMessageAndBytesToPC("response", SRI.Messages.Message.ToProtoBuf(userResponse));

        // Figure out where to go next
        if (finished)
        {
            if (_speechList.listIndex >= 0)
            {
                string historyFile = FileIO.CombinePaths(DataFileLocations.SubjectMetaFolder, _speechList.TestType + "_History.xml");
                if (File.Exists(historyFile))
                {
                    var history = FileIO.XmlDeserialize<ListHistory>(historyFile);
                    history.LastCompleted = _speechList.listIndex;
                    FileIO.XmlSerialize(history, historyFile);
                }
            }
        }
    }

    public void Ping()
    {
        _message.text = "";
    }

    public void Disconnect()
    {
        _message.text = "listening...";
    }
*/
}
