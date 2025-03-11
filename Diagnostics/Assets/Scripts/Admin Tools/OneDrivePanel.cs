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

    public static Color GetStatusColor(MSGraphClient.ConnectionStatus status)
    {
        if (status == MSGraphClient.ConnectionStatus.Ready)
        {
            return new Color(9f / 255f, 74f / 255f, 178f / 255f);
        }
        else if ((status & MSGraphClient.ConnectionStatus.HaveInterface) == 0)
        {
        }
        else if ((status & MSGraphClient.ConnectionStatus.HaveAccessToken) == 0)
        {
        }
        else if ((status == MSGraphClient.ConnectionStatus.Error || (status & MSGraphClient.ConnectionStatus.HaveFolderAccess) == 0))
        {
            return Color.red;
        }

        return new Color(0.47f, 0.47f, 0.47f);
    }

    public IEnumerator ConnectToOneDrive()
    {
        MSGraphClient.RestartPath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        yield return null;

        if (!MSGraphClient.IsReady)
        {
            MSGraphClient.Initialize("Diagnostics");
        }

        var status = MSGraphClient.GetConnectionStatus(out string details);
        cloud.color = GetStatusColor(status);
        messageBox.Show("Connecting...");

        if ((status & MSGraphClient.ConnectionStatus.HaveAccessToken) > 0)
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

        if ((status & MSGraphClient.ConnectionStatus.Error) > 0)
        { 
            Debug.Log("OneDrive: " + details);
            if (details.StartsWith("No connection"))
            {
                signInOutButton.interactable = false;
                details = $"-{details}{Environment.NewLine}-Rebooting is the easiest option";
            }
            messageBox.ShowMarkdown(details, MessageBox.IconShape.Error);
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
