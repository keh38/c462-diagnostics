using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TurandotPanel : MonoBehaviour
{
    [SerializeField] private ListBoxControl _listBox;
    [SerializeField] private MessageBox _messageBox;

    void Start()
    {
        _messageBox.Hide();
        _listBox.Items.Clear();
    }

    public void OnPlayButtonClick()
    {
        SceneManager.LoadScene("Turandot Interactive");
    }
}
