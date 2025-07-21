using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnimateSlider : MonoBehaviour 
{
    private Slider _slider;

    private RectTransform _rectTransform;

    bool _isMoving = false;
    bool _isSliding = false;
    float _speed;
    float _curX;
    float _targX;
    float _dir;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
        _rectTransform = _slider.GetComponent<RectTransform>();
    }

    public bool IsMoving
    {
        get { return _isMoving;}
    }
    public bool IsSliding
    {
        get { return _isSliding;}
    }

    public void MoveTo(float x, float speed)
    {
        _curX = transform.localPosition.x;
        _targX = x;
        this._speed = speed;
        _dir = Mathf.Sign(_targX - _curX);
        _isMoving = true;
    }

	void Update() 
    {
        if (!_isMoving)
            return;

        _curX = _rectTransform.anchoredPosition.x;

        float dx = _speed * Time.deltaTime;
        if (Mathf.Sign(_targX - (_curX + _dir*dx)) != _dir)
        {
            dx = Mathf.Abs(_targX - _curX);
            _isMoving = false;
        }

        _rectTransform.anchoredPosition += new Vector2(_dir*dx, 0);
	}

    public void ChangeValue(Slider slider, float targVal, float speed)
    {
        _isSliding = true;
        StartCoroutine(AnimateValueChange(slider, targVal, speed));
    }

    IEnumerator AnimateValueChange(Slider slider, float targVal, float speed)
    {
        bool dirChange = false;
        float dir = 2 * (float) Random.Range((int)0, (int)2) - 1;

        while (!dirChange)
        {
            float v = slider.value + dir * speed * Time.deltaTime;
            if (v <=0 || v >= 1)
            {
                dirChange = true;
                dir = -dir;
            }

            slider.value = Mathf.Clamp(v, 0, 1);

            yield return null;
        }

        while (_isSliding)
        {
            float dv = speed * Time.deltaTime;
            if (Mathf.Sign(targVal - (slider.value + dir*dv)) != dir)
            {
                dv = Mathf.Abs(targVal - slider.value);
                _isSliding = false;
            }
            slider.value += dir*dv;

            yield return null;
        }
    }

}
