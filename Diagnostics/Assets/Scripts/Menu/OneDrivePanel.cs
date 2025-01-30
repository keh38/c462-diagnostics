using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using KLib.MSGraph;

public class OneDrivePanel : MonoBehaviour
{
    public Image cloud;

    public MessageBox messageBox;

    public Button signInOutButton;
    public Button showODIButton;

    public TMPro.TMP_Text signInLabel;

    private bool _isSignedIn = false;

    public bool IsConnected { get; private set; }

    public IEnumerator ConnectToOneDrive()
    { 
        MSGraphClient.RestartPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        yield return null;

        cloud.color = new Color(0.47f, 0.47f, 0.47f);
        messageBox.Show("Connecting...");

        var success = MSGraphClient.Initialize("Training");

        if (MSGraphClient.IsConnected)
        {
            string user = MSGraphClient.GetUser();
            if (string.IsNullOrEmpty(user)) user = "???";
            messageBox.Show("Signed in as: " + user);
            signInLabel.text = "Sign out";
            _isSignedIn = true;
        }
        else
        {
            signInLabel.text = "Sign in";
            _isSignedIn = false;
        }
        signInOutButton.interactable = true;

        if (success)
        {
            cloud.color = new Color(9f / 255f, 74f / 255f, 178f / 255f);
        }
        else
        {
            var errorMsg = MSGraphClient.GetInitializationStatus();
            if (MSGraphClient.IsConnected)
            {
                cloud.color = Color.red;
            }

            Debug.Log("OneDrive: " + errorMsg);
            if (errorMsg.StartsWith("No connection"))
            {
                signInOutButton.interactable = false;
                //errorMsg += $"{Environment.NewLine}{Environment.NewLine}Rebooting is the easiest option";
                errorMsg = $"-{errorMsg}{Environment.NewLine}-Rebooting is the easiest option";
            }
            messageBox.ShowMarkdown(errorMsg, MessageBox.IconShape.Error);
        }

        IsConnected = success;
    }

    public void CheckConnectionStatus()
    {
        if (MSGraphClient.IsConnected)
        {
            cloud.color = new Color(9f / 255f, 74f / 255f, 178f / 255f);
        }
    }

    public void SignInOutButtonClick()
    {
        signInOutButton.interactable = false;

        if (_isSignedIn)
        {
            StartCoroutine(SignOutAsync());
        }
        else
        {
            StartCoroutine(SignInAsync());
        }
    }

    public void ShowODIButtonClick()
    {
        showODIButton.interactable = false;
        StartCoroutine(ShowODIAsync());
    }

    private IEnumerator ShowODIAsync()
    {
        showODIButton.interactable = false;
        yield return new WaitForSeconds(1);

        var result = MSGraphClient.SendMessageToInterface("Show");
        if (!result.Equals("OK"))
        {
            messageBox.Show(MSGraphClient.LastError, MessageBox.IconShape.Error);
        }

        showODIButton.interactable = true;
    }

    private IEnumerator SignInAsync()
    {
//        Debug.Log(AppDomain.CurrentDomain.BaseDirectory);

        if (!MSGraphClient.SignInUser())
        {
            Debug.Log("OneDrive: error signing in");
            messageBox.Show("Error signing in", MessageBox.IconShape.Error);
        }
        else
        {
            Debug.Log("OneDrive: signing in");
            messageBox.Show("This app will close and restart after you log into OneDrive.");
            yield return new WaitForSeconds(3);
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        //        Application.Quit();
        // https://answers.unity.com/questions/467030/unity-builds-crash-when-i-exit-1.html
        if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
#endif
        }
    }

    private IEnumerator SignOutAsync()
    {
        messageBox.Show("Signing out...");
        if (!MSGraphClient.SignOutUser())
        {
            messageBox.Show("Error logging off", MessageBox.IconShape.Error);
            Debug.Log("OneDrive: Error logging off");
        }
        else
        {
            yield return StartCoroutine(WaitForSignOut());
        }
        yield return StartCoroutine(ConnectToOneDrive());
    }

    private IEnumerator WaitForSignOut()
    {
        int maxTries = 5;
        int ntries = 0;
        while (ntries < maxTries)
        {
            yield return new WaitForSeconds(1);
            if (!MSGraphClient.AcquireAccessToken())
            {
                yield break;
            }
            ++ntries;
        }

        messageBox.Show("Timed out logging off");
        Debug.Log("OneDrive: Timed out logging off");
    }
}
