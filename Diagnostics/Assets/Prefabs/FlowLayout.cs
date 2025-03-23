using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlowLayout : MonoBehaviour
{
    [SerializeField] private float _spacing = 50;

    private float _xoffset = 0;
    private float _yoffset = 0;
    private float _relWidth = 0.49f;
    private int _column = 0;

    public void Clear()
    {
        int nchild = gameObject.transform.childCount;
        for (int k = nchild - 1; k >= 0; k--) GameObject.Destroy(gameObject.transform.GetChild(k).gameObject);
        _xoffset = 0;
        _yoffset = 0;
    }

    public void Add(GameObject gobj)
    {
        var myRT = GetComponent<RectTransform>();


        var rt = gobj.GetComponent<RectTransform>();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);

        if (myRT.rect.height + _yoffset - rt.rect.height < 0)
        {
            _column++;
            if (_column > 1)
            {
                Debug.LogError("so out of room");
            }
            _xoffset += 0.51f;
            _yoffset = 0;
        }

        rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, _yoffset);
        rt.anchorMin = new Vector2(_xoffset, rt.anchorMin.y);
        rt.anchorMax = new Vector2(_xoffset + _relWidth, rt.anchorMax.y);
        _yoffset -= rt.rect.height + _spacing;
    }
}
