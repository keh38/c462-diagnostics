using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HardwarePanel : MonoBehaviour
{
    [SerializeField] private MessageBox _messageBox;

    void Start()
    {
        _messageBox.Show("There were one or more errors initializing the hardware", MessageBox.IconShape.Error);
    }

}
