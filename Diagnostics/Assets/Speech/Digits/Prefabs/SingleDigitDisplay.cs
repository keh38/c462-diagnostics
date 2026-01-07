using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class SingleDigitDisplay : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private Image _background;

    public Color defColor;
    public Color focusColor;

    private float defAlpha;
    private int _value;

	void Start () 
    {
        _value = -1;
        _label.text = "-";
	}
	
    public int Value
    {
        get { return _value;}
    }

    public void Clear()
    {
        SetValue(-1);
        SetFocus(false);
    }

    public void SetValue(int value)
    {
        _label.color = Color.white;

        this._value = value;
        if (value >= 0 && value <= 9)
        {
            _label.text = value.ToString();
        }
        else
        {
            _label.text = "-";
        }
    }

    public void ShowValue(int value, Color color)
    {
        _label.text = value.ToString();
        _label.color = color;
    }

    public void SetFocus(bool focus)
    {
        _background.color = focus ? focusColor : defColor;
        //_background.alpha = defColor.a;
        //background.alpha = defAlpha;
    }
}
