using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ExtensionMethods;
using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;
using SpeechReception;
using System.IO;

public class SpeechMasker : MonoBehaviour 
{
    private List<AudioSource> _speakers = new List<AudioSource>();
    private List<string> _speechFiles = new List<string>(new string[]
    {
        "CHF10_60.wav",
        "CHM01_60.wav",
        "CHM02_60.wav",
        "CHF06_60.wav",
        "CHM11_60.wav",
        "NWF02_60.wav",
        "NWF03_60.wav",
        "NWF08_60.wav",
        "NWF09_60.wav",
        "NWM07_60.wav"
    });

    private string _source = "";
    private int _numBabblers = 1;
    private float _maxLevel;

    public IEnumerator Initialize(Masker masker, float level, string transducer, LevelUnits units, TestEar testEar)
    {
        _source = masker.Source;

        _maxLevel = GetMaxLevel(masker.Source, transducer, units.ToString());

        Debug.Log(_source + " max = " + _maxLevel + " " + units.ToString());

        if (masker.Source == "Homebrew")
        {
            yield return StartCoroutine(InitializeHomebrew(masker.NumBabblers, masker.BabbleSeed, testEar));
        }
        else
        {
            yield return StartCoroutine(InitializeWavFile(masker.Source, testEar));
        }
        SetLevel(level);
    }

    private IEnumerator InitializeWavFile(string source, TestEar testEar)
    {
        string clipName = "";

        if (source == "ASU")
        {
            clipName = "4T_Babble.wav";
        }
        else if (source == "BSC")
        {
            clipName = "4Tnewbabble_cut_ch1.wav";
        }
        else 
        {
            clipName = source + ".wav";
        }


        if (_speakers.Count == 0)
        {
            _speakers.Add(gameObject.AddComponent<AudioSource>() as AudioSource);

            _speakers[0].bypassEffects = true;
            _speakers[0].bypassListenerEffects = true;
            _speakers[0].bypassReverbZones = true;
            _speakers[0].loop = true;
            _speakers[0].spatialBlend = 0;
        }
        for (int k=1; k<_speakers.Count; k++)
        {
            GameObject.Destroy(_speakers[k]);
        }
        _speakers.RemoveRange(1, _speakers.Count - 1);

        Debug.Log(clipName);

        WWW www = new WWW("file:///" + Path.Combine(FileLocations.SpeechWavFolder, "Maskers", clipName));
        while (!www.isDone)
            yield return null;

        if (!string.IsNullOrEmpty(www.error))
            throw new System.Exception(www.error);

        _speakers[0].enabled = false;
        _speakers[0].clip = www.GetAudioClip();

        _speakers[0].panStereo = testEar.ToBalance();
    }

    private IEnumerator InitializeHomebrew(int numBabblers, int seed, TestEar testEar)
    {
        _numBabblers = numBabblers;

        if (seed > 0)
        {
            KLib.KMath.Seed = seed;
        }

        int[] randomOrder = KLib.KMath.Permute(_speechFiles.Count);

        for (int k = 0; k < numBabblers; k++)
        {
            if (k == _speakers.Count)
                _speakers.Add(gameObject.AddComponent<AudioSource>() as AudioSource);

            _speakers[k].bypassEffects = true;
            _speakers[k].bypassListenerEffects = true;
            _speakers[k].bypassReverbZones = true;
            _speakers[k].loop = true;
            _speakers[k].spatialBlend = 0;
            _speakers[k].panStereo = testEar.ToBalance();

            _speakers[k].enabled = false;

            WWW www = new WWW("file:///" + Path.Combine(FileLocations.SpeechWavFolder, "Maskers", _speechFiles[randomOrder[k]]));
            while (!www.isDone)
                yield return null;

            _speakers[k].clip = www.GetAudioClip();
            _speakers[k].time = Random.Range(0, 60f);
        }
    }

    private float GetMaxLevel(string source, string transducer, string units)
    {
        if (units == "dBatten") return 0;

        SpeechReception.References r = new SpeechReception.References(Path.Combine(FileLocations.SpeechWavFolder, "Maskers"), source);
        return r.GetReference(transducer, units);
    }

    public void SetLevel(float level)
    {
        if (_source == "Homebrew")
        {
            SetHomebrewLevel(level);
        }
        else
        {
            SetWavFileLevel(level);
        }
    }

    private void SetHomebrewLevel(float level)
    {
        float atten = level - _maxLevel - 20f * Mathf.Log10(Mathf.Sqrt(_numBabblers));
        Debug.Log(atten);
        float volume = Mathf.Pow(10f, atten / 20f);

        foreach (AudioSource a in _speakers) a.volume = volume;
    }

    private void SetWavFileLevel(float level)
    {
        float atten = level - _maxLevel;
        float volume = Mathf.Pow(10f, atten / 20f);

        Debug.Log("MASKER atten = " + atten + " dB");

        _speakers[0].volume = volume;
    }

    public void Play()
    {
        foreach (AudioSource a in _speakers) a.enabled = true;
        foreach (AudioSource a in _speakers) a.UnPause();
    }

    public void Stop()
    {
        foreach (AudioSource a in _speakers) a.Pause();
    }

    public bool IsPlaying
    {
        get { return _speakers[0].isPlaying; }
    }

}
