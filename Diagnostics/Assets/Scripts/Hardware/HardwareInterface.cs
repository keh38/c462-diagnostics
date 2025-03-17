using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardwareInterface : MonoBehaviour
{
    private KLib.AdapterMap _adapterMap;

    #region SINGLETON CREATION
    // Singleton
    private static HardwareInterface _instance;
    private static HardwareInterface instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gobj = GameObject.Find("HardwareInterface");
                if (gobj != null)
                {
                    _instance = gobj.GetComponent<HardwareInterface>();
                }
                else
                {
                    _instance = new GameObject("HardwareInterface").AddComponent<HardwareInterface>();
                }
                DontDestroyOnLoad(_instance);
                _instance._Init();
            }
            return _instance;
        }
    }
    #endregion

    #region PUBLIC STATIC ACCESSORS
    public static KLib.AdapterMap AdapterMap { get { return instance._adapterMap; } }
    #endregion

    #region PRIVATE METHODS

    private bool _Init()
    {
        _adapterMap = KLib.AdapterMap.DefaultStereoMap();


        return true;
    }
    #endregion
}
