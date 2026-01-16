using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using KLib;
using KLib.Signals;
using System.Linq;

using LDL;
using LDL.Haptics;

public class LDLHapticsSliderPanel : MonoBehaviour 
{
    [SerializeField] private Button _lockInButton;
    [SerializeField] private TMPro.TMP_Text _prompt;
    
    private List<LDLHapticsLevelSlider> _sliders;
    
    private float[] _xPositions;

    private TextSizeAnimator _textSizeAnimator;

    public UnityAction LockInPressed;

    void Awake()
    {
        _sliders = new List<LDLHapticsLevelSlider>(GetComponentsInChildren<LDLHapticsLevelSlider>());
        _textSizeAnimator = _prompt.gameObject.GetComponent<TextSizeAnimator>();
    }

    void Start() { }

    public void Initialize(LDLMeasurementSettings settings, Channel ch, Channel haptic)
    {
        _lockInButton.gameObject.SetActive(false);
        _prompt.gameObject.SetActive(false);

        _prompt.text = settings.Prompt;
        _prompt.fontSize = settings.PromptFontSize;

        _xPositions = new float[_sliders.Count];

        for (int k=0; k< _sliders.Count; k++) 
        {
            _sliders[k].InitializeStimulusGeneration(ch.Clone(), settings.Bandwidth, settings.MinLevel, settings.ModDepth_pct, haptic.Clone());
            _sliders[k].SliderMoved += OnSliderMoved;

            var rt = _sliders[k].gameObject.GetComponent<RectTransform>();
            _xPositions[k] = rt.anchoredPosition.x;
        }
    }

    public void ResetSliders(List<HapticsTestCondition> conditions)
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

    public List<HapticSliderSettings> GetSliderSettings()
    {
        return _sliders.FindAll(x => x.gameObject.activeSelf).Select(x => x.Settings).ToList();
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
