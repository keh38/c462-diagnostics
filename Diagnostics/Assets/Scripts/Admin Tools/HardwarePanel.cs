using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HardwarePanel : MonoBehaviour
{
    [SerializeField] private MessageBox _messageBox;
    [SerializeField] private GameObject _syncStartButton;

    private float _initialPollInterval;

    void Start()
    {
        _messageBox.gameObject.SetActive(false);
    }

    public void ShowHardwareErrorMessage()
    {
        _messageBox.ShowMarkdown("There were one or more errors initializing the hardware:\n" + HardwareInterface.ErrorMessage, MessageBox.IconShape.Error);
    }

    public void OnSyncStartButtonClick()
    {
        _initialPollInterval = HardwareInterface.ClockSync.pollInterval_s;
        HardwareInterface.ClockSync.pollInterval_s = 2.5f;
        HardwareInterface.ClockSync.StartSynchronizing();
        _syncStartButton.SetActive(false);

        InvokeRepeating("UpdateSyncStatus", 1, 5);
    }

    public void OnSyncStopButtonClick()
    {
        HardwareInterface.ClockSync.pollInterval_s = _initialPollInterval;
        HardwareInterface.ClockSync.StopSynchronizing();
        _syncStartButton.SetActive(false);
    }

    private void UpdateSyncStatus()
    {
        _messageBox.ShowMarkdown(
            "Sync status:\n" +
            $"- channel index = {HardwareInterface.ClockSync.ChannelIndex}\n" +
            $"- pulses generated = {HardwareInterface.ClockSync.PulsesGenerated}\n" +
            $"- pulses detected = {HardwareInterface.ClockSync.PulsesDetected}\n");
    }
}
