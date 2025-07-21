using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using KLib;
using LDL;
using KLib.Signals;
using UnityEngine.UIElements;

public class LDLSliderPanel : MonoBehaviour 
{
    [SerializeField] private GameObject _lockInButton;
    [SerializeField] private TMPro.TMP_Text _prompt;
    
    private List<LDLLevelSlider> _sliders;
    
    private bool _allSlidersMoved = false;
    private float[] _xPositions;

    void Awake()
    {
        _sliders = new List<LDLLevelSlider>(GetComponentsInChildren<LDLLevelSlider>());
    }

	void Start () 
    {
        //foreach (ParamSlider s in sliders)
        //{
        //    s.NotifyOnSliderMove = OnSliderMoved;
        //    s.DefaultState();
        //}
	}

    public void Initialize(LDLMeasurementSettings settings, Channel ch)
    {
        _lockInButton.SetActive(false);

        _prompt.text = settings.Prompt;
        _prompt.fontSize = settings.PromptFontSize;

        _xPositions = new float[_sliders.Count];

        for (int k=0; k< _sliders.Count; k++) 
        {
            _sliders[k].InitializeStimulusGeneration(ch.Clone());

            var rt = _sliders[k].gameObject.GetComponent<RectTransform>();
            _xPositions[k] = rt.anchoredPosition.x;
        }
    }

    public void HideLockInButton()
    {
        _lockInButton.SetActive(false);
    }

    public List<SliderSettings> GetSettings()
    {
        List<SliderSettings> lss = new List<SliderSettings>();
        //foreach (ParamSlider s in sliders)
        //{
        //    if (s.IsVisible)
        //        lss.Add(s.Settings);
        //}

        return lss;
    }

    public int NumSliders
    {
        get { return 0; }
        //get { return sliders.Count; }
    }

    //public ParamSlider this[int index]
    //{
    //    get { return sliders[index]; }
    //}

    public void SimulateMove(int sliderNum)
    {
        //if (sliders[sliderNum].IsVisible)
        //    sliders[sliderNum].SimulateMove();
    }

    public void HideSlider(int sliderNum)
    {
        //if (sliderNum >= 0 && sliderNum<sliders.Count)
        //    sliders[sliderNum].Show(false);
    }

    public void ResetFirstMove()
    {
        //foreach (ParamSlider s in sliders)
        //{
        //    s.ResetFirstMove();
        //}
    }

    private void OnSliderMoved()
    {
        //if (!_allSlidersMoved)
        //{
        //    _allSlidersMoved = true;
        //    foreach (ParamSlider s in sliders)
        //    {
        //        _allSlidersMoved &= s.HasMoved;
        //    }

        //    //if (_allSlidersMoved && lockinEnableCallback != null)
        //    //    lockinEnableCallback();
        //}
    }

    public void LockSliders(bool isLocked)
    {
        //_allSlidersMoved = false;
        //foreach (ParamSlider s in sliders)
        //    s.Lock(isLocked);
    }

    public IEnumerator ShuffleSliderPositions()
    {
        yield return new WaitForSeconds(0.5f);

        float speed = 1000;

        int numShuffles = 3;

        for (int ks = 0; ks < numShuffles; ks++)
        {
            _xPositions = KMath.Permute(_xPositions);
            for (int k = 0; k < _sliders.Count; k++)
            {
                _sliders[k].Mover.MoveTo(_xPositions[k], speed);
            }

            //while (true)
            //{
            //    bool anyMoving = false;
            //    foreach (ParamSlider s in sliders)
            //    {
            //        if (s.Mover.IsMoving)
            //        {
            //            anyMoving = true;
            //            break;
            //        }
            //    }
            //    if (!anyMoving)
            //        break;

            //    yield return null;
            //}
        }
    }
}
