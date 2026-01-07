using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public delegate void KeypadButtonCallback(int num);

public class KeypadButton : MonoBehaviour 
{
    public KeypadButtonCallback ButtonCallback { get; set; }

    private int _myNum;
    private KeypadButtonState _buttonState = KeypadButtonState.Enabled;

    public KeypadButtonState State
    {
        get { return _buttonState; }
        set { _buttonState = value; UpdateState(); }
    }

    public bool IsNumeric
    {
        get { return _myNum > -1 && _myNum < 10; }
    }

    public Button Button { get; private set; }

    void Awake()
    {
        Button = gameObject.GetComponent<Button>();
    }

    void Start () 
    {
        var myLabel = gameObject.GetComponentInChildren<TMPro.TMP_Text>();
        if (myLabel.text == "Del")
        {
            _myNum = 10;
        }
        else if (myLabel.text == "Enter")
        {
            _myNum = 13;
        }
        else
        {
            _myNum = int.Parse(myLabel.text.Substring(0, 1));
        }
	}

    public void OnClick()
    {
        if (_buttonState == KeypadButtonState.Enabled)
        {
            ButtonCallback?.Invoke(_myNum);
        }
    }

    private void UpdateState()
    {
        gameObject.SetActive(_buttonState != KeypadButtonState.Hidden);
        switch (_buttonState)
        {
            case KeypadButtonState.Enabled:
                Enable();
                break;
            case KeypadButtonState.Disabled:
                Disable(false);
                break;
            case KeypadButtonState.DisabledAndGrayed:
                Disable(true);
                break;
        }
    }

    private void Disable(bool gray)
    {
        Button.interactable = gray;
        Button.enabled = false;
    }

    private void Enable()
    {
        Button.interactable = true;
        Button.enabled = true;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        UpdateState();
    }
}
