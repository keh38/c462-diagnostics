using UnityEngine;
using UnityEngine.UI;

public class RecordButton : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private Image _image;
    [SerializeField] private Button _button;

    private Color _imageColor;
    private Color _textColor;

    void Awake()
    {
        _imageColor = _image.color;
        _textColor = _label.color;
    }

    public void SetLabel(string text)
    {
        _label.text = text;
    }

    public void SetInteractable(bool interactable)
    {
        _button.interactable = interactable;

        if (interactable)
        {
            _image.color = _imageColor;
            _label.color = _textColor;
        }
        else
        {
            _image.color = Color.gray;
            _label.color = Color.gray;
        }
    }   

}
