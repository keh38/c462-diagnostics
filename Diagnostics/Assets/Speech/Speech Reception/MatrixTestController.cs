using UnityEngine;
using System.Collections.Generic;

public class MatrixTestController : MonoBehaviour
{
/*    public GameObject template;
    public GameObject acceptButton;

    private List<ClosedSetButton> _buttons = new List<ClosedSetButton>();

    private int _xSeparation = 75;
    private int _ySeparation = 30;

    private List<string> _selected = new List<string>();

    private List<string> _names = new List<string> { "Peter", "Kathy", "Lucy", "Alan", "Rachel", "William", "Steven", "Thomas", "Doris", "Nina" };
    private List<string> _verbs = new List<string> { "Got", "Sees", "Brought", "Gives", "Sold", "Prefers", "Has", "Kept", "Ordered", "Wants" };
    private List<string> _numbers = new List<string> { "Three", "Nine", "Seven", "Eight", "Four", "Nineteen", "Two", "Fifteen", "Twelve", "Sixty" };
    private List<string> _adjectives = new List<string> { "Large", "Small", "Old", "Dark", "Heavy", "Green", "Cheap", "Pretty", "Red", "White" };
    private List<string> _nouns = new List<string> { "Desks", "Chairs", "Tables", "Toys", "Spoons", "Windows", "Sofas", "Rings", "Flowers", "Houses" };

    private List<List<string>> _matrix = new List<List<string>>();

    private int _numCategories;
    private bool _isShowing = false;

    private KStringDelegate _onSelectionMade = null;

    public KStringDelegate OnSelectionMade
    {
        set { _onSelectionMade = value; }
    }

    void Awake()
    {
        _matrix.Add(_names);
        _matrix.Add(_verbs);
        _matrix.Add(_numbers);
        _matrix.Add(_adjectives);
        _matrix.Add(_nouns);

        _numCategories = _matrix.Count;
        _selected = new List<string>(_numCategories);
        for (int k = 0; k < _numCategories; k++) _selected.Add(null);
    }

    void Start()
    {
    }

    public void Initialize()
    {
        foreach (ClosedSetButton b in _buttons) GameObject.Destroy(b.gameObject);
        _buttons.Clear();

        int nrow = 10;
        int ncol = 5;

        UIWidget w = GetComponent<UIWidget>();
        UISprite sprite = template.GetComponent<UISprite>();

        int rowWidth = ncol * sprite.width + (ncol - 1) * _xSeparation;
        int height = nrow * sprite.height + (nrow - 1) * _ySeparation;

        int y = height / 2 - sprite.height / 2;
        for (int kr = 0; kr < nrow; kr++)
        {
            int x = -rowWidth / 2 + sprite.width / 2;
            for (int kc = 0; kc < ncol; kc++)
            {
                _buttons.Add(CreateButton(kr, kc, x, y));
                x += (sprite.width + _xSeparation);
            }
            y -= (sprite.height + _ySeparation);
        }
    }

    public void Show()
    {
        foreach (var b in _buttons)
        {
            //b.Enable(true);
            //b.Clear();
        }
        for (int k = 0; k < _selected.Count; k++) _selected[k] = "";

        NGUITools.SetActive(acceptButton, false);
        transform.localPosition = new Vector2(0, 100);

        _isShowing = true;
    }

    public void Hide()
    {
        _isShowing = false;
        transform.localPosition = new Vector2(2500, -1250);
        foreach (var b in _buttons) b.Clear();
    }

    public void OnAcceptClick()
    {
        if (_onSelectionMade != null)
        {
            var sentence = SpeechReception.MatrixTest.WordListToString(_selected);
            _onSelectionMade(sentence);
        }
    }

    private ClosedSetButton CreateButton(int row, int col, int x, int y)
    {
        GameObject obj = GameObject.Instantiate(template);

        obj.transform.parent = template.transform.parent;
        obj.transform.localScale = Vector3.one;
        obj.transform.localPosition = new Vector2(x, y);

        ClosedSetButton b = obj.GetComponent<ClosedSetButton>();
        b.GroupNumber = col + 1;
        b.SetValue(_matrix[col][row]);
        b.OnClickNotifier = OnButtonClick;

        return b;
    }

    private void OnButtonClick(string value)
    {
        if (_isShowing)
        {
            int icat = UIToggle.current.group;
            _selected[icat - 1] = value;

            if (_selected.Find(x => string.IsNullOrEmpty(x)) == null)
            {
                NGUITools.SetActive(acceptButton, true);
            }
        }
    }

    public string Simulate(List<string> words, float SNR)
    {
        var response = new List<string>();

        float snr50 = -10;
        float s50 = 0.2f;

        float ptotal = 1 / (1 + Mathf.Exp(4 * s50 * (snr50 - SNR)));
        float pword = Mathf.Pow(ptotal, 0.2f);

        for (int k=0; k<words.Count; k++)
        {
            if (Random.Range(0f, 1f) < pword)
            {
                response.Add(words[k]);
            }
            else
            {
                response.Add("wrong");
            }
        }

        return SpeechReception.MatrixTest.WordListToString(response);
    }
*/
}
