using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcotolController : MonoBehaviour, IRemoteControllable
{

    void IRemoteControllable.ProcessRPC(string command, string data)
    {

    }

    void IRemoteControllable.ChangeScene(string newScene)
    {

    }
}
