using UnityEngine;
using System.Collections;

public class SpeechReceptionBluetoothKeepAlive : MonoBehaviour
{
    bool _isRunning = false;
    float _multiplier = 0;
    System.Random _rn;

    public void KeepAlive(float atten)
    {
        _multiplier = Mathf.Pow(10, -Mathf.Abs(atten) / 20);
        _isRunning = true;
        _rn = new System.Random();
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (_isRunning)
        {
            int idx = 0;
            for (int k=0; k<Mathf.RoundToInt(data.Length / 2f); k++)
            {
                data[idx++] = _multiplier * (float) _rn.NextDouble();
                data[idx] = data[idx - 1];
                idx++;
            }
        }
    }
}
