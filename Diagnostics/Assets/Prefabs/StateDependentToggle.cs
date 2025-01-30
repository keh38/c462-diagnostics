using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateDependentToggle : MonoBehaviour
{
    public Image onImage;
    public Image offImage;
    public TMPro.TMP_Text label;

    public string onText;
    public string offText;

    public void OnStateToggle(bool pressed)
    {
        onImage.enabled = pressed;
        offImage.enabled = !pressed;

        label.text = pressed ? onText : offText;
    }

}
