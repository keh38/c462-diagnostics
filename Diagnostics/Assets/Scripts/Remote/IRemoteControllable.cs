using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRemoteControllable
{
    void ProcessRPC(string command, string data="");
    void ChangeScene(string newScene);
}