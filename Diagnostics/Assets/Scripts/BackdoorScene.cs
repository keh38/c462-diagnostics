using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackdoorScene : MonoBehaviour
{
    void Start()
    {
        GameObject.Find("Version").GetComponent<TMPro.TMP_Text>().text = "V" + VersionInfo.Version;
    }

    void Update()
    {
        
    }

    public void QuitButton_Pressed()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }

}
