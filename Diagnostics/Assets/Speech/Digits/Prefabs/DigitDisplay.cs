using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class DigitDisplay : MonoBehaviour
{
    [SerializeField] private Keypad _keypad;
    [SerializeField] private SingleDigitDisplay[] _digitDisplays;
    [SerializeField] private Button _lockInButton;

    private int[] _digits;

    private int _numDigits;
    private int _curTypeDigit;

    private bool _isActive;
    private bool _anyPressed;

    public delegate void DigitDisplayCallback();
    public DigitDisplayCallback OnEnter { get; set; }
    public DigitDisplayCallback OnFirstPress { get; set; }

    void Start()
    {
        _numDigits = _digitDisplays.Length;
        _digits = new int[_numDigits];

        _keypad.SetCallback(OnKeypadPress);
        _keypad.BackspaceButton.ButtonCallback = OnBackspacePress;
        _keypad.EnterButton.ButtonCallback = OnEnterPress;

        if (_lockInButton == null)
        {
            _lockInButton = _keypad.EnterButton.Button;
        }

        Clear();
    }

    public bool IsActive { get; private set; }

    public int[] Value
    {
        get { return (int[])_digits.Clone(); }
    }

    public void SetButtonStates(KeypadButtonState state, params int[] buttons)
    {
        foreach (int i in buttons)
        {
            if (i >= 0 && i < 10)
            {
                _keypad[i].State = state;
            }
        }
    }

    public void SetButtonStates(KeypadButtonState state, params string[] buttonNames)
    {
        foreach (string b in buttonNames)
        {
            _keypad[b].State = state;
        }
    }

    public void Hide()
    {
        _isActive = false;
        _keypad.Hide();
        //gameObject.SetActive(false);
    }

    public void Show()
    {
        _anyPressed = false;
        _isActive = true;
        _lockInButton.enabled = true;
        _keypad.Show();
        gameObject.SetActive(true);

        Clear();
    }

    public void Disable()
    {
        _isActive = false;
        _lockInButton.enabled = false;
        _digitDisplays[_curTypeDigit].SetFocus(false);
    }

    public void Hide_lockInButton()
    {
        _lockInButton.gameObject.SetActive(false);
    }

    public void Clear()
    {
        for (int k = 0; k < _numDigits; k++)
        {
            _digits[k] = -1;
            _digitDisplays[k].Clear();
        }

        _curTypeDigit = 0;
        _digitDisplays[_curTypeDigit].SetFocus(true);
        _lockInButton.gameObject.SetActive(false);
    }

    private void OnKeypadPress(int num)
    {
        if (!_isActive)
        {
            return;
        }

        OnFirstPress?.Invoke();

        _digitDisplays[_curTypeDigit].SetValue(num);
        _digitDisplays[_curTypeDigit].SetFocus(false);

        if (++_curTypeDigit == _numDigits)
        {
            _curTypeDigit = 0;
        }

        _digitDisplays[_curTypeDigit].SetFocus(true);
        UpdateDigits();
    }

    private void OnBackspacePress(int num)
    {
        if (!_isActive)
        {
            return;
        }

        if (_digitDisplays[_curTypeDigit].Value < 0)
        {
            _digitDisplays[_curTypeDigit].SetFocus(false);
            if (--_curTypeDigit < 0)
            {
                _curTypeDigit = _numDigits - 1;
            }
        }

        _digitDisplays[_curTypeDigit].SetValue(-1);
        _digitDisplays[_curTypeDigit].SetFocus(true);

        UpdateDigits();
    }

    private void OnEnterPress(int num)
    {
        if (_isActive)
        {
            OnEnter?.Invoke();
        }
    }

    void Update()
    {
        if (_isActive && Input.anyKeyDown)
        {
            int digit = -1;
            for (int k=0; k<10; k++)
            {
                if ((Input.GetKeyDown(KeyCode.Alpha0+k) || Input.GetKeyDown(KeyCode.Keypad0+k)) && _keypad[k].State == KeypadButtonState.Enabled)
                {
                    digit = k;
                    break;
                }
            }
            
            if (digit > -1)
            {
                OnKeypadPress(digit);
            }
            else if (Input.GetKeyDown(KeyCode.Backspace) && _keypad.BackspaceButton.State == KeypadButtonState.Enabled)
            {
                OnBackspacePress(-1);
            }
        }
    }

    private void UpdateDigits()
    {
        bool allSet = true;

        for (int k=0; k<_numDigits; k++)
        {
            _digits[k] = _digitDisplays[k].Value;
            allSet &= _digits[k]>-1;
        }
        _lockInButton.gameObject.SetActive(allSet);
    }


    public IEnumerator Feedback(int[] correctAnswer)
    {
        _digitDisplays[_curTypeDigit].SetFocus(false);
        yield return new WaitForSeconds(0.3f);

        for (int k=0; k<_numDigits; k++)
        {
            _digitDisplays[k].ShowValue(correctAnswer[k], correctAnswer[k]==_digits[k] ? Color.green : Color.red);
            yield return new WaitForSeconds(0.3f);
        }
    }
}
