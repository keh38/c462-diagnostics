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
        [SerializeField] private Text _label;
        [SerializeField] private GameObject _checklistItemPrefab;
        [SerializeField] private int _spacing = 50;

        private ChecklistLayout _layout;
        private List<ChecklistItem> _items = new List<ChecklistItem>();

        public override string Name { get { return _layout.Name; } }
        public ButtonData ButtonData { get; private set; }

        private string _result;
        public override string Result { get { return _result; } }

        public void Initialize(ChecklistLayout layout)
        {
            _layout = layout;
            LayoutControl();
            ButtonData = new ButtonData() { name = layout.Name };
        }


        public void LayoutControl()
        {
            _label.text = _layout.Label;
            var labelRT = _label.GetComponent<RectTransform>();
            float yoffset = -labelRT.rect.height - _spacing;

            var myRect = GetComponent<RectTransform>();

            float width = 0;

            _items.Clear();
            foreach (var item in _layout.Items)
            {
                var gobj = GameObject.Instantiate(_checklistItemPrefab, myRect);
                var ci = gobj.GetComponent<ChecklistItem>();
                ci.SetLabel(item);
                ci.Toggled = OnItemToggled;
                float itemWidth = ci.GetWidth();
                _items.Add(ci);

                var rt = gobj.GetComponent<RectTransform>();
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, yoffset);
                rt.anchorMin = new Vector2(0, rt.anchorMin.y);
                rt.anchorMax = new Vector2(0, rt.anchorMax.y);

                yoffset -= rt.rect.height + _spacing;
                width = Mathf.Max(width, itemWidth);
            }
            myRect.sizeDelta = new Vector2(width, -yoffset);
        }

        private void OnItemToggled(string name, bool isPressed)
        {
            _result = name;
            ButtonData.value = true;
        }

        public override void Activate(Inputs.Input input)
        {
            _result = "";
            ButtonData.value = false;
            base.Activate(input);
        }

        public override void Deactivate()
        {
            foreach (var i in _items) i.Clear();
            base.Deactivate();
        }

    }
}