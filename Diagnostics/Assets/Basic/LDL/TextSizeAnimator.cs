using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TextSizeAnimator : MonoBehaviour
{
    [SerializeField] private float _scale;
    [SerializeField] private float _duration;
    [SerializeField] private AnimationCurve _curve;

    private TMPro.TMP_Text _text;
    private RectTransform _rectTransform;

    private bool _enabled = false;
    private float _startTime = 0;

    private void Awake()
    {
        _text = GetComponent<TMPro.TMP_Text>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Animate()
    {
        _startTime = Time.time;
        _enabled = true;
    }

    private void Update()
    {
        if (_enabled)
        {
            var relTime = (Time.time - _startTime) / _duration;
            var scale = Mathf.Lerp(1, _scale, _curve.Evaluate(relTime));
            if (relTime >= 1)
            {
                scale = 1;
                enabled = false;
            }

            _rectTransform.localScale = scale * Vector3.one;
        }
        
    }


}
