using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using KLib;
using KLib.MSGraph;

public class HomeMenu : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private TMPro.TMP_Text _versionLabel;
    [SerializeField] private TMPro.TMP_Text _message;

    [SerializeField] private Button _subjectMenuButton;
    [SerializeField] private Button _turandotMenuButton;
    [SerializeField] private Button _pupilMenuButton;
    [SerializeField] private Button _configMenuButton;
    [SerializeField] private Button _quitMenuButton;

    [SerializeField] private PlayPanel _playPanel;
    [SerializeField] private SubjectPanel _subjectPanel;
    [SerializeField] private TurandotPanel _turandotPanel;
    [SerializeField] private PupilPanel _pupilPanel;
    [SerializeField] private GameObject _quitPanel;

    [SerializeField] private Image _oneDriveIcon;
    [SerializeField] private Image _networkIcon;

    private Color _networkActiveColor = new Color(0f, 0.7f, 0f);

    private GameObject _activePanel = null;
    private Button _activeButton = null;

    private TMPro.TMP_Text _subjectLabel;

    private void Awake()
    {
        _subjectLabel = _subjectMenuButton.GetComponentInChildren<TMPro.TMP_Text>();
    }

    IEnumerator Start()
    {
        Application.runInBackground = true;

        _versionLabel.text = "V" + Application.version;
        yield return null;

        if (!GameManager.Initialized)
        {
            EnableMenu(false);
            //_message.text = "Starting up...";
            yield return null;

            KLogger.Create(
                Path.Combine(Application.persistentDataPath, "Logs", Application.productName + ".log"),
                retainDays: 14)
                .StartLogging();

            Debug.Log($"Started V{Application.version}");
            
            _subjectPanel.SubjectChangedEvent.AddListener(OnSubjectChanged);

            _message.text = "";
            EnableMenu(true);

            HardwareInterface.Initialize();

            HTS_Server.StartServer();
            GameManager.Initialized = true;
        }

        HTS_Server.SetCurrentScene("Home", this);

        ConnectToCloud();

        if (!string.IsNullOrEmpty(GameManager.Subject))
        {
            _subjectLabel.text = GameManager.Subject;
        }

        _subjectMenuButton.Select();
        OnSubjectMenuButtonClick();

        _networkIcon.color = HTS_Server.RemoteConnected ? _networkActiveColor : Color.gray;

        StartCoroutine(InitializeHardware());
    }

    private IEnumerator InitializeHardware()
    {
        yield return new WaitForSeconds(1);

        if (!HardwareInterface.IsReady && !HardwareInterface.ErrorAcknowledged)
        {
            SceneManager.LoadScene("Admin Tools");
        }
    }

    private async void ConnectToCloud()
    {
        MSGraphClient.RestartPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        if (!MSGraphClient.IsReady)
        {
            await Task.Run(() => MSGraphClient.Initialize("Diagnostics"));
        }

        if (_oneDriveIcon != null)
        {
            _oneDriveIcon.color = OneDrivePanel.GetStatusColor(MSGraphClient.GetConnectionStatus());
        }
    }

    void EnableMenu(bool enabled)
    {
        _subjectMenuButton.interactable = enabled;
        _configMenuButton.interactable = enabled;
    }

    public void PlayMenuButtonClick()
    {
        //SelectItem(playMenuButton, playPanel.gameObject);
    }

    public void OnSubjectMenuButtonClick()
    {
        StartCoroutine(SelectSubjectPanel());
        //StartCoroutine(SelectItem(_subjectMenuButton, _subjectPanel.gameObject));
        //_subjectPanel.ShowPanel();
    }
    private IEnumerator SelectSubjectPanel()
    {
        yield return StartCoroutine(SelectItem(_subjectMenuButton, _subjectPanel.gameObject));
        _subjectPanel.ShowPanel();
    }

    public void OnTurandotButtonClick()
    {
        StartCoroutine(SelectTurandotPanel());
    }
    private IEnumerator SelectTurandotPanel()
    {
        yield return StartCoroutine(SelectItem(_turandotMenuButton, _turandotPanel.gameObject));
        _turandotPanel.ShowPanel();
    }

    public void OnPupilButtonClick()
    {
        StartCoroutine(SelectPupilPanel());
    }
    private IEnumerator SelectPupilPanel()
    {
        yield return StartCoroutine(SelectItem(_pupilMenuButton, _pupilPanel.gameObject));
        //_turandotPanel.ShowPanel();
    }

    private void OnSubjectChanged(string newSubject)
    {
        _subjectLabel.text = newSubject;
    }

    public void OnAdminButtonClick()
    {
        SceneManager.LoadScene("Admin Tools");
    }

    public void OnQuitMenuClick()
    {
        StartCoroutine(SelectItem(_quitMenuButton, _quitPanel));
    }

    public void OnQuitConfirmClick()
    {
        Debug.Log("Quitting");
        HardwareInterface.CleanUp();
        KLogger.Log.StopLogging();
#if UNITY_STANDALONE_WIN
        //        Application.Quit();
        // https://answers.unity.com/questions/467030/unity-builds-crash-when-i-exit-1.html
        if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }

    private IEnumerator SelectItem(Button button, GameObject panel)
    {
        ColorBlock cb;

        if (_activePanel != null)
        {
            _activePanel.SetActive(false);
            cb = _activeButton.colors;
            cb.normalColor = new Color(cb.normalColor.r, cb.normalColor.g, cb.normalColor.b, 0);
            cb.selectedColor = cb.normalColor;
            _activeButton.colors = cb;
        }

        _activePanel = panel;
        _activeButton = button;

        _activePanel.SetActive(true);
        cb = _activeButton.colors;
        cb.normalColor = new Color(cb.normalColor.r, cb.normalColor.g, cb.normalColor.b, 1);
        cb.selectedColor = cb.normalColor;
        _activeButton.colors = cb;

        yield return null;
    }

    void IRemoteControllable.ProcessRPC(string command, string data="")
    {
        switch (command)
        {
            case "Connect":
                _networkIcon.color = _networkActiveColor;
                break;

            case "Disconnect":
                _networkIcon.color = Color.gray;
                break;

            case "SubjectChanged":
                if (_activePanel == _subjectPanel.gameObject)
                {
                    _subjectPanel.ShowPanel();
                }
                break;

            case "SubjectMetadataChanged":
                if (_activePanel == _subjectPanel.gameObject)
                {
                    _subjectPanel.ShowPanel();
                }
                break;
        }
    }
    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }
}
