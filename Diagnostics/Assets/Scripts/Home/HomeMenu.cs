using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

using KLib;

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

    private GameObject _activePanel = null;
    private Button _activeButton = null;

    private TMPro.TMP_Text _subjectLabel;

    private void Awake()
    {
        _subjectLabel = subjectMenuButton.GetComponentInChildren<TMPro.TMP_Text>();
    }

    IEnumerator Start()
    {
        versionLabel.text = "V" + Application.version;
        if (!GameManager.Initialized)
        {
            EnableMenu(false);
            message.text = "Starting up...";
            yield return null;

            var logFolder = Path.Combine(Application.persistentDataPath, "Logs");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }

            KLogger.Create(
                Path.Combine(Application.persistentDataPath, "Logs", Application.productName + ".log"),
                retainDays: 14)
                .StartLogging();

            Debug.Log($"Started V{Application.version}");

            subjectPanel.SubjectChangedEvent.AddListener(OnSubjectChanged);
            //yield return StartCoroutine(oneDrivePanel.ConnectToOneDrive());

            message.text = "";
            EnableMenu(true);

            GameManager.Initialized = true;
        }
        else
        {
            //oneDrivePanel.CheckConnectionStatus();
        }

        if (!string.IsNullOrEmpty(GameManager.Subject)) 
        {
            _subjectLabel.text = GameManager.Subject;
        }

        //SelectItem(playMenuButton, playPanel.gameObject);
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

    public void SubjectMenuButtonClick()
    {
        SelectItem(subjectMenuButton, subjectPanel.gameObject);
        subjectPanel.ShowPanel();
    }

    private void OnSubjectChanged(string newSubject)
    {
        _subjectLabel.text = newSubject;
    }

    public void OnConfigButtonClick()
    {
        //SelectItem(configMenuButton, configPanel.gameObject);
    }

    public void QuitMenuClick()
    {
        SelectItem(quitMenuButton, quitPanel);
    }

    public void QuitConfirmClick()
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
