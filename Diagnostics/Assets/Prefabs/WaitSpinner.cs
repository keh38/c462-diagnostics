using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitSpinner : MonoBehaviour
{
    public Image image;
    public float spinRate = 180f;

    private float _angle = 0;
    private bool _isActive = false;

    public bool IsActive
    {
        set
        {
            image.enabled = value;
            _isActive = value;
        }
        get
        {
            return _isActive;
        }
    }

    void Update()
    {
        if (_isActive)
        {
            transform.Rotate(new Vector3(0, 0, spinRate * Time.deltaTime));
        }
    }

}
