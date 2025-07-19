using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class QuestionBox : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _text;

    public delegate void ResponseDelegate(bool yes);
    public ResponseDelegate Response;
    private void OnResponse(bool yes)
    {
        Response?.Invoke(yes);
    }

    public void PoseQuestion(string question, ResponseDelegate response)
    {
        Response = response;
        _text.text = question;
    }

    public void OnYesClick()
    {
        OnResponse(true);
    }

    public void OnNoClick()
    {
        OnResponse(false);
    }

}
