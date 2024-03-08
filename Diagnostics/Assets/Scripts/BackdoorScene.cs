using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackdoorScene : MonoBehaviour
{
    void Start()
    {
        GameObject.Find("Version").GetComponent<TMPro.TMP_Text>().text = "V" + VersionInfo.Version;

        FillSubjectDropdown();
    }

    void FillSubjectDropdown()
    {
        var obj = GameObject.Find("Subject Dropdown").GetComponent<TMPro.TMP_Dropdown>();
        obj.options.Clear();
        obj.options.Add(new TMPro.TMP_Dropdown.OptionData("get bent/ass"));
        obj.options.Add(new TMPro.TMP_Dropdown.OptionData("get bent/hole"));
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
