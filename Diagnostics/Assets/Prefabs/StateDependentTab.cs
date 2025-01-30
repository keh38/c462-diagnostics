using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class StateDependentTab : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _panel;

    private Color _offColor = new Color(0.5607f, 0.6431f, 0.6980f);

    public void OnToggleValueChanged(bool pressed)
    {
        var color = pressed ? Color.white : _offColor;

        _label.color = color;
        _image.color = color;

        //        if (Application.isPlaying)
        _panel.transform.localScale = pressed ? Vector3.one : Vector3.zero;
    }
}
