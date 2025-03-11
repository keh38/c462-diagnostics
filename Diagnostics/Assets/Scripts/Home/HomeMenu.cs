using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using KLib;
using KLib.MSGraph;

public class HomeMenu : MonoBehaviour
{
    public TMPro.TMP_Text versionLabel;
    public TMPro.TMP_Text message;

    public PlayPanel playPanel;
    public SubjectPanel subjectPanel;
    public GameObject quitPanel;

    public Button subjectMenuButton;
    public Button configMenuButton;
    public Button quitMenuButton;

    [SerializeField] private Image _oneDriveIcon;
    [SerializeField] private Image _networkIcon;

    private GameObject _activePanel = null;
    private Button _activeButton = null;

    private TMPro.TMP_Text _subjectLabel;

    private void Awake()
    {
        _subjectLabel = subjectMenuButton.GetComponentInChildren<TMPro.TMP_Text>();
    }

    IEnumerator Start()
    {
        Application.runInBackground = true;

        versionLabel.text = "V" + Application.version;
        yield return null;

        if (!GameManager.Initialized)
        {
            EnableMenu(false);
            message.text = "Starting up...";
            yield return null;

            KLogger.Create(
                Path.Combine(Application.persistentDataPath, "Logs", Application.productName + ".log"),
                retainDays: 14)
                .StartLogging();

            Debug.Log($"Started V{Application.version}");

            subjectPanel.SubjectChangedEvent.AddListener(OnSubjectChanged);

            message.text = "";
            EnableMenu(true);

            HTS_Server.StartServer();

            GameManager.Initialized = true;
        }

        ConnectToCloud();

        if (!string.IsNullOrEmpty(GameManager.Subject)) 
        {
            _subjectLabel.text = GameManager.Subject;
        }

        subjectMenuButton.Select();
        OnSubjectMenuButtonClick();

        _networkIcon.color = HTS_Server.RemoteConnected ? Color.green : Color.gray;
    }

    private async void ConnectToCloud()
    {
        MSGraphClient.RestartPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        if (!MSGraphClient.IsReady)
        {
            await Task.Run(() => MSGraphClient.Initialize("Diagnostics"));
        }

        _oneDriveIcon.color = OneDrivePanel.GetStatusColor(MSGraphClient.GetConnectionStatus());
    }

    void EnableMenu(bool enabled)
    {
        subjectMenuButton.interactable = enabled;
        configMenuButton.interactable = enabled;
    }

    public void PlayMenuButtonClick()
    {
        //SelectItem(playMenuButton, playPanel.gameObject);
    }

    public void OnSubjectMenuButtonClick()
    {
        SelectItem(subjectMenuButton, subjectPanel.gameObject);
        subjectPanel.ShowPanel();
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
        SelectItem(quitMenuButton, quitPanel);
    }

    public void OnQuitConfirmClick()
    {
        Debug.Log("Quitting");
        KLib.KLogger.Log.StopLogging();
#if UNITY_STANDALONE_WIN
        //        Application.Quit();
        // https://answers.unity.com/questions/467030/unity-builds-crash-when-i-exit-1.html
        if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
    }

    private void SelectItem(Button button, GameObject panel)
    {
        if (_activePanel != null)
        {
            _activePanel.SetActive(false);
        }

        _activePanel = panel;
        _activeButton = button;

        _activePanel.SetActive(true);
    }
}
