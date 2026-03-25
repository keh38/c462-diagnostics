using KLib;
using KLibU;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _versionLabel;

    IEnumerator Start()
    {
        string nextScene = "Home";

#if !UNITY_EDITOR
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--lobby")
            {
                nextScene = "Lobby";
            }
        }

        if (HardwareInterface.LockWindowInCenter)
        {
            yield return null;
            WindowManager.InitializeWindow(Screen.width, Screen.height);
        }

#else
        //nextScene = "Lobby";
#endif

        Application.runInBackground = true;

#if UNITY_EDITOR
        Application.targetFrameRate = 60;
#endif

        _versionLabel.text = "V" + Application.version;
        KLogger.Create(
            Path.Combine(Application.persistentDataPath, "Logs", Application.productName + ".log"),
            retainDays: 14)
            .StartLogging();

        Debug.Log($"Started V{Application.version}");

        HTS_Server.StartServer();
        HTS_Server.SetCurrentScene("Loading", null);

        yield return StartCoroutine(InitializeHardware());
        GameManager.Initialized = true;

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(nextScene);
    }

    private IEnumerator InitializeHardware()
    {
        yield break;
        //yield return new WaitForSeconds(1);

        //if (!HardwareInterface.IsReady)
        //{
        //    SceneManager.LoadScene("Admin Tools");
        //}
    }



}
