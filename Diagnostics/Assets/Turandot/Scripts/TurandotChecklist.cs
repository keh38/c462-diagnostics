using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


using Turandot.Screen;
using Turandot.Inputs;

namespace Turandot.Scripts
{
    public class TurandotChecklist : TurandotInput
    {
        [SerializeField] private TMPro.TMP_Text _label;
        [SerializeField] private GameObject _checklistItemPrefab;
        [SerializeField] private GameObject _button;
        [SerializeField] private int _spacing = 50;

        private ToggleGroup _toggleGroup;

        private ChecklistLayout _layout;
        private List<ChecklistItem> _items = new List<ChecklistItem>();

        private List<string> _selectedItems = new List<string>();

        public override string Name { get { return _layout.Name; } }
        public ButtonData ButtonData { get; private set; }

        private string _result;
        public override string Result { get { return _result; } }

        public void Initialize(ChecklistLayout layout)
        {
            _toggleGroup = GetComponent<ToggleGroup>();
            _layout = layout;

            LayoutControl();
            ButtonData = new ButtonData() { name = layout.Name };
        }

        public void LayoutControl()
        {
            _label.fontSize = _layout.FontSize;
            _label.text = _layout.Label;

            _button.GetComponentInChildren<TMPro.TMP_Text>().fontSize = _layout.FontSize;

            var labelRT = _label.GetComponent<RectTransform>();
            float yoffset = - _layout.PromptSpacing;

            var myRect = GetComponent<RectTransform>();

            float width = 0;

            _items.Clear();
            foreach (var item in _layout.Items)
            {
                var gobj = GameObject.Instantiate(_checklistItemPrefab, myRect);
                var ci = gobj.GetComponent<ChecklistItem>();
                ci.SetLabel(item, _layout.FontSize);
                if (!_layout.AllowMultiple)
                {
                    ci.SetGroup(_toggleGroup);
                }
                ci.Toggled = OnItemToggled;
                float itemWidth = ci.GetWidth();

                _items.Add(ci);

                var rt = gobj.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, yoffset);

                yoffset -= ci.GetHeight() + _spacing;
                width = Mathf.Max(width, itemWidth);
            }
            myRect.sizeDelta = new Vector2(width+500, -yoffset);
            myRect.anchorMin = new Vector2(_layout.X, _layout.Y);
            myRect.anchorMax = new Vector2(_layout.X, _layout.Y);
        }

        private void OnItemToggled(string name, bool isPressed)
        {
            if (isPressed)
            {
                _selectedItems.Add(name);
            }
            else
            {
                _selectedItems.Remove(name);
            }

            if (!_layout.AllowMultiple)
            {
                _toggleGroup.allowSwitchOff = _layout.AllowNone;
            }

            if (_layout.AutoAdvance && !_layout.AllowMultiple)
            {
                _result = name;
                OnButtonClick();
                //ButtonData.value = true;
            }
            else
            {
                _button.SetActive(_selectedItems.Count > 0);
            }
        }
        public void OnButtonClick()
        {
            ButtonData.value = true;

            _result = $"{Name}=\"";
            for (int k=0; k<_selectedItems.Count; k++)
            {
                _result += $"{_selectedItems[k]}";
                if (k < _selectedItems.Count - 1)
                {
                    _result += ",";
                }
            }
            _result += "\";";
        }

        public override void Activate(Inputs.Input input, TurandotAudio audio)
        {
            _result = $"{Name}=\"\"";

            _selectedItems.Clear();

            _toggleGroup.allowSwitchOff = true;
            _toggleGroup.SetAllTogglesOff();

            _button.SetActive(false);
            ButtonData.value = false;

            base.Activate(input, audio);
        }

        public override void Deactivate()
        {
            foreach (var i in _items) i.Clear();
            base.Deactivate();
        }

    }
}