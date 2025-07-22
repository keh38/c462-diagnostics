using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


using Questionnaires;
using Microsoft.Graph;
using UnityEngine.Events;

public class QuestionnaireChecklist : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _label;
    [SerializeField] private GameObject _checklistItemPrefab;
    [SerializeField] private int _spacing = 50;

    private ToggleGroup _toggleGroup;

    private List<ChecklistItem> _items = new List<ChecklistItem>();

    public UnityAction<bool> SelectionChanged;

    private void Awake()
    {
        _toggleGroup = GetComponent<ToggleGroup>();
    }

    public List<int> GetSelectionNumbers()
    {
        var selection = new List<int>();

        for (int k=0; k<_items.Count; k++)        
        {
            if (_items[k].gameObject.activeSelf && _items[k].Value)
            {
                selection.Add(k);
            }
        }
        return selection;
    }

    public List<string> GetSelectionValues()
    {
        var selection = new List<string>();

        for (int k = 0; k < _items.Count; k++)
        {
            if (_items[k].gameObject.activeSelf && _items[k].Value)
            {
                selection.Add(_items[k].Name);
            }
        }
        return selection;
    }

    public void LayoutChecklist(Question question, List<int> selected, int fontSize)
    {
        _label.fontSize = fontSize;
        _label.text = question.Prompt;

        var labelRT = _label.GetComponent<RectTransform>();
        float yoffset = -2 * _spacing;

        var myRect = GetComponent<RectTransform>();

        float width = 0;

        for (int k=0; k<question.Options.Count; k++)
        {
            if (k >= _items.Count)
            {
                var gobj = GameObject.Instantiate(_checklistItemPrefab, myRect);
                var ci = gobj.GetComponent<ChecklistItem>();
                ci.Toggled = OnItemToggled;
                _items.Add(ci);
            }

            _items[k].gameObject.SetActive(true);

            _items[k].SetLabel(question.Options[k], fontSize);
            _items[k].SetGroup(question.AllowMultipleSelections ? null: _toggleGroup);
            _items[k].Value = selected.Contains(k);

            float itemWidth = _items[k].GetWidth();

            var rt = _items[k].gameObject.GetComponent<RectTransform>();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, yoffset);

            yoffset -= _items[k].GetHeight() + _spacing;
            width = Mathf.Max(width, itemWidth);
        }

        for (int k=question.Options.Count; k<_items.Count; k++)
        {
            _items[k].gameObject.SetActive(false);
        }

        myRect.sizeDelta = new Vector2(width, -yoffset);
    }

    private void OnItemToggled(string name, bool isPressed)
    {
        SelectionChanged?.Invoke(_items.Find(x => x.Value) != null);
    }



}
