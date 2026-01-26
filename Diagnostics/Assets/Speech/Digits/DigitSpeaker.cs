using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using KLib.Signals;
using KLib.Signals.Calibration;

public class DigitSpeaker : MonoBehaviour 
{
    public enum SpeakerID {F01, M01, M02};
    public SpeakerID speakerID;

    public float IDI_ms = 680; // interval between digits
    public float atten_dB = 0;
    public float itd_us = 0;

    private List<AudioClip> _digitClips = new List<AudioClip>();
    private int[] _digits;
    private bool _isSpeaking;
    private float _tZero = 1e-3f;

    void Start() 
    {
        _isSpeaking = false;
        StartCoroutine(GetClips());
	}

    public int[] SpokenDigits
    {
        get { return _digits; }
        set { _digits = (int[])value.Clone(); }
    }

    public int SpokenDigit
    {
        get { return _digits[0]; }
        set { _digits = new int[] { value }; }
    }

    public bool IsSpeaking
    {
        get { return _isSpeaking;}
    }

    IEnumerator GetClips()
    {
        DigitsWavManifest manifest = KLib.FileIO.XmlDeserializeFromTextAsset<DigitsWavManifest>("Digit Manifests/DigitsWavManifest_" + speakerID.ToString());

        foreach (string fn in manifest.wavfiles)
        {
            WWW www = new WWW("file:///" + Path.Combine(FileLocations.BasicResourcesFolder, "Digits", speakerID.ToString(), fn));
            while (!www.isDone)
                yield return null;
            
            _digitClips.Add(ZeroPadClip(www.GetAudioClip()));
        }
        //Debug.Log(ComputeMaxSPL());
    }

    public static float GetMaxLevel(string transducer, string units)
    {
        SpeechReception.References r = new SpeechReception.References(Path.Combine(FileLocations.BasicResourcesFolder, "Digits"), "Digits");
        return r.GetReference(transducer, units);
    }

    public float ComputeMaxSPL()
    {
        CalibrationData cal = CalibrationFactory.Load(LevelUnits.dB_SPL, GameManager.Transducer, "Diotic");

        float sum = 0;
        foreach (AudioClip a in _digitClips)
        {
            float[] data = new float[a.samples];
            a.GetData(data, 0);
            sum +=cal.GetMax(data, AudioSettings.outputSampleRate);
        }

        return sum / _digitClips.Count;
    }

    public void Clear()
    {
        StopAllCoroutines();
        _isSpeaking = false;
    }

    public void Randomize(List<List<int>> availableDigits)
    {
        _digits = new int[availableDigits.Count];
        for (int k=0; k<_digits.Length; k++)
        {
            int idx = Random.Range(0, availableDigits[k].Count);

            _digits[k] = availableDigits[k][idx];
            availableDigits[k].RemoveAt(idx);
        }
    }

    public void SpeakAll()
    {
        GetComponent<AudioSource>().volume = Mathf.Pow(10, atten_dB / 20);
        StartCoroutine(DoSpeakAll());
    }
    
    IEnumerator DoSpeakAll()
    {
        _isSpeaking = true;
        
        foreach (int digit in _digits)
        {
            GetComponent<AudioSource>().clip = _digitClips[digit];
            GetComponent<AudioSource>().Play();
        
            yield return new WaitForSeconds(Mathf.Max(GetComponent<AudioSource>().clip.length, IDI_ms / 1000));
        }
        
        _isSpeaking = false;
    }

    public void SpeakDigit(int digitNum)
    {
        GetComponent<AudioSource>().volume = Mathf.Pow(10, atten_dB / 20);
        StartCoroutine(DoSpeakDigit(digitNum));
    }

    public void Speak()
    {
        SpeakDigit(0);
    }

    IEnumerator DoSpeakDigit(int digitNum)
    {
        _isSpeaking = true;

        GetComponent<AudioSource>().clip = ApplyITDToClip(_digitClips[_digits[digitNum]], itd_us);
        GetComponent<AudioSource>().Play();
        
        yield return new WaitForSeconds(Mathf.Max(GetComponent<AudioSource>().clip.length, IDI_ms/1000));
        
        _isSpeaking = false;
    }

    public AudioClip ZeroPadClip(AudioClip clip)
    {
        int numPad = Mathf.RoundToInt(2 * _tZero * clip.frequency);
        AudioClip paddedClip = AudioClip.Create("padded", clip.samples + 2 * numPad, 1, clip.frequency, false);

        float[] y = new float[clip.samples];
        clip.GetData(y, 0);

        paddedClip.SetData(y, numPad);

        return paddedClip;
    }

    private AudioClip ApplyITDToClip(AudioClip clip, float ITD)
    {
        int nshift = Mathf.RoundToInt(0.5f * ITD * 1e-6f * clip.frequency);
        int ncenter = Mathf.RoundToInt(_tZero * clip.frequency);

        float[] yclip = new float[clip.samples];
        clip.GetData(yclip, 0);

        float[] yitd = new float[2 * clip.samples];

        int index = 0;
        int iright = ncenter + nshift;
        int ileft = ncenter - nshift;

//        Debug.Log("ITD = " + ITD + "; nshift = " + nshift + "; iright = " + iright + "; ileft = " + ileft);

        for (int k=0; k<clip.samples - 2*ncenter + Mathf.Abs(nshift); k++)
        {
            yitd[index++] = yclip[ileft++];
            yitd[index++] = yclip[iright++];
        }

        AudioClip newClip = AudioClip.Create("ITD digit", clip.samples, 2, clip.frequency, false);
        newClip.SetData(yitd, 0);

        return newClip;
    }


}
