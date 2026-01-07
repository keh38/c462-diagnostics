using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class Keypad : MonoBehaviour 
{
    private List<KeypadButton> _buttons;

    private List<int> _disabled;

    public KeypadButton BackspaceButton { get; private set; }
    public KeypadButton EnterButton { get; private set; }

    void Awake()
    {
        _buttons = new List<KeypadButton>(GetComponentsInChildren<KeypadButton>());

        BackspaceButton = _buttons.Find(b => b.name == "Backspace");
        EnterButton = _buttons.Find(b => b.name == "Enter");
    }

    public void SetCallback(KeypadButtonCallback callback)
    {
        foreach (KeypadButton b in _buttons)
        {
            if (b.IsNumeric) b.ButtonCallback = callback;
        }
    }

    public KeypadButton this[string value]
    {
        get { return _buttons.Find(b => b.name == value); }
    }

    public KeypadButton this[int value]
    {
        get { return _buttons.Find(b => b.name == value.ToString()); }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        //foreach (KeypadButton kb in _buttons)
        //{
        //    kb.Show();
        //}
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        //foreach (KeypadButton kb in _buttons)
        //{
        //    kb.Hide();
        //}
    }
}
