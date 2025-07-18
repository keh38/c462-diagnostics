using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Protocols;

public class ProcotolController : MonoBehaviour, IRemoteControllable
{
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private InstructionPanel _instructionPanel;
    [SerializeField] private TMPro.TMP_Text _outline;
    [SerializeField] private Text _finishText;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private GameObject _quitPanel;

    private Protocol _protocol;
    private ProtocolHistory _history;

    private bool _isRemote;
    private int _nextTestIndex;
    private bool _advanceAfterInstructions = false;

    private bool _waitingForResponse = false;

    private void Start()
    {
        Cursor.visible = true;
        HTS_Server.SetCurrentScene("Protocol", this);

        _title.text = "";

        if (string.IsNullOrEmpty(GameManager.DataForNextScene))
        {
            _isRemote = HTS_Server.RemoteConnected;
            if (!_isRemote)
            {
                ShowFinishPanel("Nothing to do");
            }
        }     
    }

    void Update()
    {
        if (!_waitingForResponse) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            _waitingForResponse = false;
            _outline.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(_protocol.Tests[_nextTestIndex].Instructions))
            {
                ShowInstructions(
                    _protocol.Tests[_nextTestIndex].Instructions, 
                    fontSize: _protocol.Appearance.InstructionFontSize,
                    autoAdvance: true);
            }
            else
            {
                HTS_Server.SendMessage("Protocol", "Advance");
            }
        }
    }

    void OnGUI()
    {
        if (!_waitingForResponse) return;

        Event e = Event.current;
        if (e.control && e.keyCode == KeyCode.A)
        {
            _waitingForResponse = false;
            _outline.gameObject.SetActive(false);
            _quitPanel.SetActive(true);
        }
    }

    public void OnQuitConfirmButtonClick()
    {
        HTS_Server.SendMessage("Protocol", "Quit");
        SceneManager.LoadScene("Home");
    }

    public void OnQuitCancelButtonClick()
    {
        _quitPanel.SetActive(false);
        _outline.gameObject.SetActive(true);
        _waitingForResponse = true;
    }

    private void ShowInstructions(string instructions, int fontSize, bool autoAdvance)
    {
        _advanceAfterInstructions = autoAdvance;
        _instructionPanel.gameObject.SetActive(true);
        _instructionPanel.InstructionsFinished = OnInstructionsFinished;
        _instructionPanel.ShowInstructions(new Turandot.Instructions() { Text = instructions, FontSize = fontSize });
    }
    private void OnInstructionsFinished()
    {
        _instructionPanel.gameObject.SetActive(false);
        if (_advanceAfterInstructions)
        {
            HTS_Server.SendMessage("Protocol", "Advance");
        }
        else
        {
            StartCoroutine(AnimateOutline());
        }
    }

    private IEnumerator AnimateOutline()
    {
        yield return new WaitForSeconds(0.5f);
        for (int k=0; k<_history.Data.Count; k++)
        {
            if (!_protocol.Tests[k].HideOutline)
            {
                DrawOutline(k + 1, -1);
                yield return new WaitForSeconds(1f);
            }
        }
        DrawOutline(_history.Data.Count, _nextTestIndex);

        HTS_Server.SendMessage("Protocol", "Waiting");
        _waitingForResponse = true;
    }

    private void DrawOutline(int numLines, int selected)
    {
        string text = "";
        for (int k = 0; k < numLines; k++)
        {
            if (!_protocol.Tests[k].HideOutline)
            { 
                string line = _history.Data[k].Title;
                if (k == selected)
                {
                    line = $"<color=#00aa00><b>{line}</b></color>";
                }
                else if (!string.IsNullOrEmpty(_history.Data[k].Date))
                {
                    line = $"<color=#888888><i>{line}</i></color>";
                }
                else
                {
                }
                text += $"{line}\n";
            }
        }

        if (selected > -1)
        {
            text += "\n\n<i>Click to continue</i>";
        }

        _outline.text = text;
        _outline.gameObject.SetActive(true);
    }

    private void ShowFinishPanel(string message = "")
    {
        _finishText.text = message;
        _finishPanel.SetActive(true);
    }

    public void OnFinishButtonClick()
    {
        Return();
    }

    private void Return()
    {
        SceneManager.LoadScene("Home");
    }

    private void RpcSetProtocol(string data)
    {
        _protocol = KLib.FileIO.XmlDeserializeFromString<Protocol>(data);
        _title.text = _protocol.Title;
        _outline.fontSize = _protocol.Appearance.ListFontSize;
    }

    private void RpcSetHistory(string data)
    {
        _history = KLib.FileIO.XmlDeserializeFromString<ProtocolHistory>(data);
    }

    private void RpcBegin(int testIndex)
    {
        _nextTestIndex = testIndex;
        if (testIndex == 0 && !string.IsNullOrEmpty(_protocol.Introduction))
        {
            HTS_Server.SendMessage("Protocol", "Instructions");
            ShowInstructions(
                _protocol.Introduction, 
                fontSize: _protocol.Appearance.InstructionFontSize,
                autoAdvance: false);
        }
        else
        {
            StartCoroutine(AnimateOutline());
        }
    }

    private void RpcFinish()
    {
        DrawOutline(_history.Data.Count, -1);
    }

    void IRemoteControllable.ProcessRPC(string command, string data)
    {
        var parts = data.Split(':');

        switch (command)
        {
            case "SetProtocol":
                RpcSetProtocol(data);
                break;
            case "SetHistory":
                RpcSetHistory(data);
                break;
            case "Begin":
                RpcBegin(int.Parse(data));
                break;
            case "Finish":
                RpcFinish();
                break;
        }
    }

    void IRemoteControllable.ChangeScene(string newScene)
    {
        SceneManager.LoadScene(newScene);
    }
}
