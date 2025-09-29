using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Turandot;
using Turandot.Cues;
using Turandot.Screen;

namespace Turandot.Scripts
{
    public class TurandotCueController : MonoBehaviour
    {
        [SerializeField] private GameObject _fixationPrefab;
        [SerializeField] private GameObject _imagePrefab;
        [SerializeField] private GameObject _messagePrefab;
        [SerializeField] private GameObject _textBoxPrefab;
        [SerializeField] private GameObject _videoPrefab;

        private List<string> _used = new List<string>();
        private List<Flag> _flags = null;

        private List<TurandotCue> _controls;
        private List<Cue> _cues;

        public void Initialize(List<CueLayout> cues)
        {
           _controls = new List<TurandotCue>();

            var canvasRT = GameObject.Find("Canvas").GetComponent<RectTransform>();
            foreach (var layout in cues)
            {
                if (layout is FixationPointLayout)
                {
                    var gobj = GameObject.Instantiate(_fixationPrefab, canvasRT);
                    var c = gobj.GetComponent<TurandotFixationPoint>();
                    c.Initialize(layout as FixationPointLayout);
                    _controls.Add(c);
                    gobj.SetActive(false);
                }
                else if (layout is ImageLayout)
                {
                    var gobj = GameObject.Instantiate(_imagePrefab, canvasRT);
                    var c = gobj.GetComponent<TurandotImage>();
                    c.Initialize(layout as ImageLayout);
                    _controls.Add(c);
                    gobj.SetActive(false);
                }
                else if (layout is MessageLayout)
                {
                    var gobj = GameObject.Instantiate(_messagePrefab, canvasRT);
                    var c = gobj.GetComponent<TurandotCueMessage>();
                    c.Initialize(layout as MessageLayout);
                    _controls.Add(c);
                    gobj.SetActive(false);
                }
                else if (layout is VideoLayout)
                {
                    var gobj = GameObject.Instantiate(_videoPrefab, canvasRT);
                    var c = gobj.GetComponent<TurandotVideo>();
                    c.Initialize(layout as VideoLayout);
                    _controls.Add(c);
                    gobj.SetActive(false);
                }
            }
        }

        public void ClearScreen()
        {
            if (_controls == null) return;

            foreach (var c in _controls) c.gameObject.SetActive(false);
        }

       public void ClearLog()
        {
            foreach (var control in _controls)
            {
                control.ClearLog();
            }
        }

        public void SetFlags(List<Flag> flags)
        {
            _flags = flags;
        }

        public void Activate(List<Cue> cues)
        {
            _cues = cues;

            foreach (Cue c in cues)
            {
                var target = _controls.Find(x => x.Name.Equals(c.Target));
                target?.Activate(c);
            }
        }

        public void Deactivate()
        {
            foreach (Cue c in _cues)
            {
                var target = _controls.Find(x => x.Name.Equals(c.Target));
                target?.Deactivate();
            }
            }

        public string LogJSONString
        {
            get
            {
                string json = "";
                foreach (var control in _controls)
                {
                    json = KLib.FileIO.JSONStringAdd(json, control.Name, control.LogJSONString);
                }

                return json;
            }
        }

    }
}