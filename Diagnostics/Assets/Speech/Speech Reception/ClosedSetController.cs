using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClosedSetController : MonoBehaviour
{
/*    public GameObject template;
    public GameObject acceptButton;
    public UILabel FBlabel;
    public UISprite FBsprite;

    private List<ClosedSetButton> _buttons = new List<ClosedSetButton>();

    private GameObject _repeatButton;
    private GameObject _continueButton;

    private int _xSeparation = 50;
    private int _ySeparation = 50;

    private SpeechReception.ClosedSet _options = new SpeechReception.ClosedSet();
    private List<string> _values;

    private string _selected = "";

    private KStringDelegate _onSelectionMade = null;

    private bool _shuffle = false;
    private SpeechReception.ClosedSet.FeedbackType _feedback = SpeechReception.ClosedSet.FeedbackType.None;
    private int[] _feedbackValues = null;

    private bool _applyActive = false;

    public KStringDelegate OnSelectionMade
    {
        set { _onSelectionMade = value; }
    }

    void Awake()
    {
        _repeatButton = GameObject.Find("Closed Set/Repeat Button");
        _continueButton = GameObject.Find("Closed Set/Continue Button");
    }

    void Start()
    {
        NGUITools.SetActive(_repeatButton, false);
        NGUITools.SetActive(_continueButton, false);
    }

    public void Initialize(SpeechReception.ClosedSet closedSet, List<string> responses)
    {
        Debug.Log(closedSet.feedback);
        _feedback = closedSet.feedback;
        _shuffle = closedSet.shuffle;
        _values = responses;

        if (_feedback == SpeechReception.ClosedSet.FeedbackType.Investigator) _feedbackValues = new int[4];

        NGUITools.SetActive(FBlabel.gameObject, false);

        foreach (ClosedSetButton b in _buttons) GameObject.Destroy(b.gameObject);
        _buttons.Clear();

        int nresp = responses.Count;
        int nrow = closedSet.numRows;
        int ncol = 0;

        if (nrow > 0)
        {
            ncol = Mathf.CeilToInt((float)nresp / nrow);
        }
        else
        {
            ncol = Mathf.CeilToInt(Mathf.Sqrt(nresp));
            nrow = Mathf.CeilToInt((float)nresp / ncol);
        }

        UIWidget w = GetComponent<UIWidget>();
        UISprite sprite = template.GetComponent<UISprite>();

        int height = nrow * sprite.height + (nrow - 1) * _ySeparation;
        int y = height / 2 - sprite.height/2;
        int nremaining = nresp;
        for (int k=0; k< nrow; k++)
        {
            int nthisrow = Mathf.Min(ncol, nremaining);
            int rowWidth = nthisrow * sprite.width + (nthisrow - 1) * _xSeparation;
            int x = -rowWidth / 2 + sprite.width/2;

            for (int j=0; j<nthisrow; j++)
            {
                _buttons.Add(CreateButton(x, y));
                x += (sprite.width + _xSeparation);
            }

            y -= (sprite.height + _ySeparation);
            nremaining -= nthisrow;
        }

        _values.Sort();
        for (int k = 0; k < _buttons.Count; k++) _buttons[k].SetValue(_values[k]);
    }

    public void Show()
    {
        NGUITools.SetActive(_repeatButton, false);
        NGUITools.SetActive(_continueButton, false);

        if (_shuffle) ShuffleValues();

        foreach (var b in _buttons)
        {
            b.Enable(true);
            b.Clear();
        }
        NGUITools.SetActive(acceptButton, false);
        _applyActive = true;
        transform.localPosition = new Vector2(0, 50);
    }

    public void Hide()
    {
        NGUITools.SetActive(FBlabel.gameObject, false);
        transform.localPosition = new Vector2(0, -1000);
        foreach (var b in _buttons) b.Clear();
    }

    public void OnButtonToggle()
    {
        NGUITools.SetActive(acceptButton, true);
    }

    public void OnAcceptClick()
    {
        if (_applyActive)
        {
            _applyActive = false;
            if (_onSelectionMade != null) _onSelectionMade(_selected);
        }
    }

    private ClosedSetButton CreateButton(int x, int y)
    {
        GameObject obj = GameObject.Instantiate(template);

        obj.transform.parent = template.transform.parent;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = new Vector2(x, y);

        ClosedSetButton b = obj.GetComponent<ClosedSetButton>();
        b.OnClickNotifier = OnButtonClick;

        return b;
    }

    private void ShuffleValues()
    {
        int[] iorder = KLib.KMath.Permute(_values.Count);
        for (int k = 0; k < _buttons.Count; k++) _buttons[k].SetValue(_values[iorder[k]]);
    }

    private void OnButtonClick(string value)
    {
        _selected = value;
    }

    public void ShowFeedback(bool correct, string actual)
    {
        if (_feedback == SpeechReception.ClosedSet.FeedbackType.Subject)
        {
            FBsprite.spriteName = "Icon_" + (correct ? "Right" : "Wrong");
            FBlabel.text = (correct ? "Correct" : "Correct answer was '" + actual + "'");
            NGUITools.SetActive(FBlabel.gameObject, true);

            if (!correct)
            {
                foreach (var b in _buttons) b.Enable(false);
                NGUITools.SetActive(_repeatButton, true);
                NGUITools.SetActive(_continueButton, true);
                NGUITools.SetActive(acceptButton, false);
            }
        }

    }
*/
}
