using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HardwarePanel : MonoBehaviour
{
    [SerializeField] private MessageBox _messageBox;

    void Start()
    {
        //_messageBox.Show("There were one or more errors initializing the hardware", MessageBox.IconShape.Error);
    }

    public void OnSyncStartButtonClick()
    {
        HardwareInterface.ClockSync.StartSynchronizing();
    }

    public void OnSyncStopButtonClick()
    {
        HardwareInterface.ClockSync.StopSynchronizing();
    }

}
