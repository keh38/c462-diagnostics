using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using KLib.MSGraph;

public class UpdatePanel : MonoBehaviour
{
    public TMPro.TMP_Text statusLabel;
    public Button updateButton;
    public WaitSpinner appWaitSpinner;
    public MessageBox messageBox;

    private string _updateURL = "";

    void Start()
    {
        
    }

    public void CheckForUpdates()
    {
        messageBox.Hide();
        if (MSGraphClient.IsConnected)
        {
            StartCoroutine(CheckForUpdatesAsync());
        }
        else
        {
            appWaitSpinner.IsActive = false;
            updateButton.interactable = false;

            statusLabel.text = "Connect to OneDrive to check for updates";
        }
    }

    private IEnumerator CheckForUpdatesAsync()
    {
        appWaitSpinner.IsActive = true;
        updateButton.interactable = false;

        statusLabel.text = "Checking for updates...";
        Debug.Log("Checking for updates");

        yield return new WaitForSeconds(1);
        string remoteFolder = GameManager.Project + "/Admin/Update";

        _updateURL = "";
        var maxVersion = KLib.VersionInfo.FromString(Application.version);

        try
        {
            bool updatesAvailable = false;

            foreach (var driveItem in MSGraphClient.GetFiles(remoteFolder))
            {
                if (driveItem.name.StartsWith(Application.productName))
                {
                    Match m = Regex.Match(driveItem.name, @"_([0-9\-]+).exe");

                    if (m.Success)
                    {
                        updatesAvailable = true;

                        var remoteVersion = KLib.VersionInfo.FromString(m.Groups[1].Value);
                        if (KLib.VersionInfo.Compare(remoteVersion, maxVersion) > 0)
                        {
                            maxVersion = remoteVersion;
                            _updateURL = driveItem.url;
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(_updateURL))
            {
                statusLabel.text = $"Version {maxVersion.ToString()} is ready to install";
                updateButton.interactable = true;
            }
            else if (!updatesAvailable)
            {
                statusLabel.text = $"No updates available";
            }
            else
            {
                statusLabel.text = "All set -- you have the latest version of the app";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            statusLabel.text = $"Error: {ex.Message}";
        }

        appWaitSpinner.IsActive = false;
    }

    public void UpdateButtonClick()
    {
        StartCoroutine(UpdateAsync());
    }

    private IEnumerator UpdateAsync()
    {
        updateButton.interactable = false;
        yield return null;

        string command = "install;" + _updateURL;
#if !UNITY_EDITOR
            command += ";" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
#endif
        var result = MSGraphClient.SendMessageToInterface(command);
        if (result.Equals("OK"))
        {
            messageBox.Show("The app will close and then restart after updating. See if Windows is asking for permission.");
            yield return new WaitForSeconds(3);
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        else
        {
            messageBox.Show(MSGraphClient.LastError, MessageBox.IconShape.Error);
        }
    }
}
