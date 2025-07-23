using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
//using UnityEngine.UIElements;

using KLib;
using LDL;
using KLib.Signals;
using System.Linq;

public class LDLSliderPanel : MonoBehaviour 
{
    [SerializeField] private Button _lockInButton;
    [SerializeField] private TMPro.TMP_Text _prompt;
    
    private List<LDLLevelSlider> _sliders;
    
    private float[] _xPositions;

    private TextSizeAnimator _textSizeAnimator;

    public UnityAction LockInPressed;

    void Awake()
    {
        _sliders = new List<LDLLevelSlider>(GetComponentsInChildren<LDLLevelSlider>());
        _textSizeAnimator = _prompt.gameObject.GetComponent<TextSizeAnimator>();
    }

	void Start () 
    {
	}

    public void Initialize(LDLMeasurementSettings settings, Channel ch)
    {
        _lockInButton.gameObject.SetActive(false);
        _prompt.gameObject.SetActive(false);

        _prompt.text = settings.Prompt;
        _prompt.fontSize = settings.PromptFontSize;

        _xPositions = new float[_sliders.Count];

        for (int k=0; k< _sliders.Count; k++) 
        {
            _sliders[k].InitializeStimulusGeneration(ch.Clone(), settings.MinLevel, settings.ModDepth_pct);
            _sliders[k].SliderMoved += OnSliderMoved;

            var rt = _sliders[k].gameObject.GetComponent<RectTransform>();
            _xPositions[k] = rt.anchoredPosition.x;
        }
    }

    public void ResetSliders(List<TestCondition> conditions)
    {
        for (int k=0; k<conditions.Count; k++)
        {
            _sliders[k].gameObject.SetActive(true);
            _sliders[k].ResetSlider(conditions[k]);
        }

        for (int k = conditions.Count; k < _sliders.Count; k++)
        {
            _sliders[k].gameObject.SetActive(false);
        }

        _prompt.gameObject.SetActive(true);
        _textSizeAnimator.Animate();
    }

    public void OnLockInButtonPressed()
    {
        _prompt.gameObject.SetActive(false);
        _lockInButton.interactable = false;

        foreach (var s in _sliders)
        {
            s.Lock(true);
        }

        LockInPressed?.Invoke();
    }

    public void HideLockInButton()
    {
        _lockInButton.gameObject.SetActive(false);
    }

    public List<SliderSettings> GetSliderSettings()
    {
        return _sliders.Select(x => x.Settings).ToList();
    }

    public int NumSliders
    {
        get { return _sliders.Count; }
    }

    private void OnSliderMoved()
    {
        if (!_lockInButton.gameObject.activeSelf)
        {
            var allSlidersMoved = _sliders.Find(x => !x.HasMoved) == null;
            if (allSlidersMoved)
            {
                _lockInButton.gameObject.SetActive(true);
                _lockInButton.interactable = true;
            }
        }
    }

    public IEnumerator ShuffleSliderPositions()
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var s in _sliders)
        {
            s.DefaultState();
        }

        float speed = 1000;

        int numShuffles = 3;

        for (int ks = 0; ks < numShuffles; ks++)
        {
            _xPositions = KMath.Permute(_xPositions);
            for (int k = 0; k < _sliders.Count; k++)
            {
                _sliders[k].Mover.MoveTo(_xPositions[k], speed);
            }

            while (true)
            {
                var anyMoving = _sliders.Find(x => x.Mover.IsMoving) != null;
                if (!anyMoving)
                {
                    break;
                }

                yield return null;
            }
        }
    }

    public void SimulateMove()
    {
        foreach (var s in _sliders)
        {
            if (s.gameObject.activeSelf)
            {
                s.SimulateMove();
            }
        }

    }
}
