using UnityEngine;
using System.Collections.Generic;

//using Vectrosity;

using Turandot.Screen;

public class TurandotGrapher : MonoBehaviour
{
    // TURANDOT FIX
    public GameObject background;
    public MeshRenderer paper;
    public Material material;
    public GameObject line;
    public GameObject stimulus;
    public GameObject marker;
    public Camera grapherCamera;

    //private VectorLine _trailLine = null;

    private int _maxPts = 16383;
    private int _curIndex;

    private bool _isRunning = false;
    private bool _isVisible = false;

    private float _xOffset;
    private float _xPosition;
    private float _xMax;
    private float _xWrap;

    private float _yPosition = 0f;
    private float _yMin = 0f;
    private float _yMax = 1f;

    private float _tlastAudio = 0;

    private GrapherLayout _layout;
    private Turandot.Inputs.GrapherAction _action = null;
    private Turandot.Inputs.GrapherLog _log = null;

    private bool _isMoving;
    private Vector2 _lastMousePosition;

    private Rect _backgroundRect;
    private List<GameObject> _markers = new List<GameObject>();
    private KLib.Signals.SignalManager _sigMan;

    private Turandot.ButtonData _buttonData = new Turandot.ButtonData();

    public Turandot.ButtonData ButtonData { get { return _buttonData; } }

    void Update()
    {
        if (_isRunning)
        {
            UpdateYPosition();

            float delta = _layout.speed * Time.deltaTime;

            if (_layout.stylusPositionFixed)
            {
                //                _xOffset += delta;
                _xOffset = _sigMan.ElapsedTime * _layout.speed;
                line.transform.localPosition = new Vector2(-_xOffset, line.transform.localPosition.y);
                stimulus.transform.localPosition = new Vector2(-_xOffset, 0);
                paper.material.mainTextureOffset = new Vector2(_xOffset, 0);
            }
            else
            {
                delta = _layout.speed * (_sigMan.ElapsedTime - _tlastAudio);
                _tlastAudio = _sigMan.ElapsedTime;
                _xPosition += delta;
                if (_xPosition >= _xMax) // wrap
                {
                    _xPosition -= _xWrap;
                    _xOffset += _xWrap;
                    line.transform.localPosition = new Vector2(line.transform.localPosition.x - _xWrap, line.transform.localPosition.y);
                    paper.material.mainTextureOffset = new Vector2(_xOffset, 0);
                    stimulus.transform.localPosition = new Vector2(-_xOffset, 0);
                }
            }

            transform.position = new Vector3(_xPosition, _yPosition, 2f);

            //if (_curIndex < _maxPts)
            //{
            //    _trailLine.points3[_curIndex++] = new Vector3(_xPosition + _xOffset, _yPosition, 0);
            //    _trailLine.drawStart = 0;
            //    _trailLine.drawEnd = _curIndex - 1;
            //}

            //_trailLine.Draw3D();

            _log.Add(Time.timeSinceLevelLoad, _sigMan.ElapsedTime, (_yPosition - _yMin) / (_yMax - _yMin), _isMoving ? 1 : 0);
        }
        else if (_isVisible)
        {
            UpdateButtonValue();
        }

    }

    void UpdateButtonValue()
    {
#if UNITY_METRO && !UNITY_EDITOR
        Vector2 thisMousePosition = (Input.touchCount > 0) ? Input.touches[0].position : Vector2.zero;
        _buttonData.value = Input.touchCount > 0 && IsTouchValid(thisMousePosition, true);
#else
        Vector2 thisMousePosition = Input.mousePosition;
        _buttonData.value = Input.GetMouseButton(0) && IsTouchValid(thisMousePosition, true);
#endif
    }

    void UpdateYPosition()
    {
#if UNITY_METRO && !UNITY_EDITOR
        Vector2 thisMousePosition = (Input.touchCount > 0) ? Input.touches[0].position : Vector2.zero;
        bool touchValid = Input.touchCount > 0 && IsTouchValid(thisMousePosition, _layout.mustContactStylus);
//        bool touchStarted = Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && IsTouchValid(thisMousePosition, true) && !_isMoving;
        bool touchStarted = touchValid && !_isMoving;
        _buttonData.value = Input.touchCount > 0 && IsTouchValid(thisMousePosition, true);
#else
        Vector2 thisMousePosition = Input.mousePosition;

        bool touchValid = Input.GetMouseButton(0) && IsTouchValid(thisMousePosition, _layout.mustContactStylus);
        //        bool touchStarted = Input.GetMouseButtonDown(0) && IsTouchValid(thisMousePosition, true) && !_isMoving;
        bool touchStarted = touchValid && !_isMoving;
        _buttonData.value = Input.GetMouseButton(0) && IsTouchValid(thisMousePosition, true);
#endif

        if (!_isMoving && !touchStarted) return;

        if (!touchValid)
        {
            _isMoving = false;
            return;
        }

        if (touchStarted)
        {
            _isMoving = true;
            _lastMousePosition = thisMousePosition;
        }

        // Has the mouse moved since the last call?
        Vector2 mousePosDelta = thisMousePosition - _lastMousePosition;

        if (mousePosDelta.sqrMagnitude > 0)
        {
            Vector3 currentMovementOffset = TouchPosToWorld(thisMousePosition) - TouchPosToWorld(_lastMousePosition);
            _yPosition += currentMovementOffset.y;
            _yPosition = Mathf.Clamp(_yPosition, _yMin, _yMax);
        }

        _lastMousePosition = thisMousePosition;
    }

    public void Initialize(GrapherLayout layout)
    {
        _buttonData = new Turandot.ButtonData();
        _buttonData.name = "Grapher";

        _layout = layout;

        transform.localScale = new Vector2(_layout.stylusW, layout.stylusW);
        paper.transform.localScale = new Vector3(_layout.graphW, _layout.graphH, 1f);

        var wback = _layout.graphW + 0.1f;
        var hback = _layout.graphH + 0.1f;
        background.transform.localScale = new Vector3(wback, hback, 1f);
        _backgroundRect = new Rect(background.transform.position.x - wback / 2, _layout.graphY - hback / 2, wback, hback);
        
        line.transform.localPosition = new Vector2(0, -_layout.graphY);

        float w = _layout.graphW / 3.6f;
        grapherCamera.rect = new Rect((1- w)/2, 0, w, 1);

        paper.material.mainTextureScale = new Vector2(_layout.graphW, _layout.graphH);

        _xMax = _layout.graphW / 2 - _layout.stylusW;
        _xWrap = 0.95f * _layout.graphW;

        _yMin = _layout.graphY - paper.transform.localScale.y / 2;
        _yMax = _layout.graphY + paper.transform.localScale.y / 2;
    }

    public void Activate(Turandot.Inputs.Input action, KLib.Signals.SignalManager sigMan, float timeOut)
    {
        _action = action as Turandot.Inputs.GrapherAction;
        if (!_action.BeginVisible)
        {
            transform.parent.transform.position = new Vector2(0, 2f);
            _isVisible = false;
            return;
        }

        _tlastAudio = 0f;
        transform.parent.transform.position = new Vector2(_layout.graphX, _layout.graphY);
        _isVisible = true;
        grapherCamera.enabled = true;

        if (_action.reset)
        {
            _sigMan = sigMan;
            InitializeStimulusMarkers(sigMan, timeOut);
            Reset();
        }

        //_trailLine.active = true;
        _isMoving = false;
        _isRunning = _action.enabled;
    }

    public void Deactivate()
    {
        transform.parent.transform.position = new Vector2(0, _action!=null && _action.EndVisible ? _layout.graphY : 2f);
        _isRunning = false;
        _isVisible = _action != null && _action.EndVisible;

        //if (_action != null && !_action.endVisible && _trailLine != null)
        //{
        //    _trailLine.active = false;
        //    grapherCamera.enabled = false;
        //    foreach (var o in _markers) GameObject.Destroy(o);
        //}
    }

    private void InitializeStimulusMarkers(KLib.Signals.SignalManager sigMan, float timeOut)
    {
        foreach (var o in _markers) GameObject.Destroy(o);
        _markers.Clear();

        if (!_action.markStimuli || sigMan == null)
        {
            marker.SetActive(false);
            return;
        }

        marker.SetActive(true);
        stimulus.transform.localPosition = Vector3.zero;

        var g = sigMan.channels[0].gate;

        float stimDur = g.Duration_ms / 1000;
        float isi = Mathf.Min(g.Period_ms / 1000, timeOut);
        float w = stimDur * _layout.speed;
        int n = Mathf.Max(Mathf.FloorToInt(timeOut / isi), 1);
        float dx = isi * _layout.speed;

        for (int k=0; k< n; k++)
        {
            GameObject m = GameObject.Instantiate(marker);
            m.transform.localScale = new Vector2(w, 1.0f);
            m.transform.localPosition = new Vector3(_layout.stylusX + w/2 + k * dx, _layout.graphY, 0.75f);
            m.transform.parent = stimulus.transform;
            _markers.Add(m);
        }

        marker.SetActive(false);
    }

    public string LogJSONString
    {
        get
        {
#if KDEBUG
            return "";
#endif
            _log.Trim();
            return KLib.FileIO.JSONSerializeToString(_log);
        }
    }

    public string Result
    {
        get
        {
            string t = "";
            string y = "";

            for (int k=0; k<_log.Length; k++)
            {
                t += _log.t[k].ToString("F3") + ",";
                y += _log.y[k].ToString("F3") + ",";
            }

            return "{\"t\":[" + t + "],\"y\":[" + y + "]}";
        }
    }

    private void Reset()
    {
        /*
        _log = new Turandot.Inputs.GrapherLog();
        _log.Clear();

        _xOffset = 0;
        _xPosition = _layout.stylusX;
        _yPosition = _yMin;
        transform.position = new Vector3(_xPosition, _yPosition, 2f);

        VectorLine.Destroy(ref _trailLine);

        VectorLine.SetCamera3D(grapherCamera);
        VectorLine.canvas3D.gameObject.layer = 13;
        VectorLine.canvas3D.transform.position = new Vector3(0, 0, 2f);

        _trailLine = new VectorLine("trail", new Vector3[_maxPts], material, 3f, LineType.Continuous, Joins.Fill);
        _trailLine.drawTransform = line.transform;

        _curIndex = 0;

        _trailLine.SetColor(KLib.Unity.ColorFromARGB(_layout.inkColor));
        */
    }

    private bool IsTouchValid(Vector2 touchPos, bool stylus)
    {
        Vector3 touchWorld = TouchPosToWorld(touchPos);
        touchWorld.z = transform.position.z;
        bool touchStylus = (touchWorld - transform.position).magnitude < 5 * _layout.stylusW;

        bool touchBackground = _backgroundRect.Contains(new Vector2(touchWorld.x, touchWorld.y));

        return touchStylus || (!stylus && touchBackground);
    }

    private Vector3 TouchPosToWorld(Vector2 touchPos)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 0f));
    }

}
