using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals.Calibration;
using KLib.Signals.Enumerations;

using KLib.Wave;

namespace KLib.Signals.Waveforms
{
    [System.Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class UserFile : Waveform
    {
        [ProtoMember(1, IsRequired = true)]
        public string fileName;
        [ProtoMember(2, IsRequired = true)]
        public bool oneShot;
        [ProtoMember(3, IsRequired = true)]
        public bool canComputeReference = true;

        private int _curIndex;
        private float _ref_dBV;
        private float _ref_dB;
        private float _maxLevel;
        private float[] _wavData;
        private Dictionary<string, float> _references;

        private bool _playedOnce = false;

        //private WaveFormat _format = null;
        private long _numSamplesPerChannel;
        private List<string> _tags = new List<string>();
        private float _samplingRate = float.NaN;
        private float _duration = float.NaN;
        private int _offset = 0;

        private static string _wavFolder="";

        public UserFile()
        {
            shape = Waveshape.File;
            _shortName = "File";
        }

        [ProtoIgnore]
        [JsonIgnore]
        public List<string> Tags
        {
            get { return _tags; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float SamplingRate
        {
            get { return _samplingRate; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        public float Duration
        {
            get { return _duration; }
        }

        [ProtoIgnore]
        [JsonIgnore]
        [XmlIgnore]
        public static string WavFolder
        {
            get { return _wavFolder; }
            set { _wavFolder = value; }
        }

        public void UpdateMetadata()
        {
            string wfpath = ConstructFilePath(fileName);

            WaveFile wf = new WaveFile();
            wf.Read(wfpath, true);

            _samplingRate = (float) wf.SamplingRate;
            _duration = wf.Duration;

            //Debug.Log(wfpath + ": " + _duration + "s @ " + _samplingRate + " Hz");
        }

        [ProtoIgnore]
        [JsonIgnore]
        public long SamplesPerChannel
        {
            get { return _numSamplesPerChannel; }
        }

        private void ParseTags()
        {
            _tags.Clear();

            if (string.IsNullOrEmpty(fileName)) return;


            string pattern = @"(\-[a-zA-Z]+)";
            Match m = Regex.Match(fileName, pattern);
            while (m.Success)
            {
                _tags.Add(m.Groups[1].Value.Substring(1));
                m = m.NextMatch();
            }
        }

        public override float GetMaxLevel(Level level, float Fs)
        {
            if (!File.Exists(ConstructFilePath(fileName)))
            {
                return float.NaN;
            }
            ReadData(level.Destination);
            ComputeReferences(level, Fs);
            return _maxLevel;
        }

        new public static List<SweepableParam> GetSweepableParams()
        {
            List<SweepableParam> par = new List<SweepableParam>();
            return par;
        }

        override public void ResetSweepables()
        {
            _curIndex = 0;
            _playedOnce = false;
        }

        override public List<string> GetValidParameters()
        {
            ParseTags();
            return _tags;
        }

        override public string SetParameter(string paramName, float value)
        {
            string pattern = @"(\-" + paramName + "[0-9]+)";
            Match m = Regex.Match(fileName, pattern);
            while (m.Success)
            {
                fileName = fileName.Replace(m.Groups[1].Value, "-" + paramName + value.ToString());
                m = m.NextMatch();
            }
            return "";
        }

        override public Action<float> ParamSetter(string paramName)
        {
            Action<float> setter = null;
            return setter;
        }

        override public void Initialize(float Fs, int N, Channel channel)
        {
            base.Initialize(Fs, N, channel);

            ReadData(channel.level.Destination);
            ComputeReferences(channel.level, samplingRate_Hz);
            _curIndex = 0;
            _offset = 0;

            _playedOnce = false;
            if (oneShot)
            {
                OffsetBy(channel.gate.Delay_ms);
            }
        }

        public void OffsetBy(float delay_ms)
        {
            _offset = Mathf.RoundToInt(1e-3f * delay_ms * samplingRate_Hz);
        }

        public void ReadData(string destination)
        {
            string wfpath = ConstructFilePath(fileName);
            if (!File.Exists(wfpath))
                throw new System.Exception("File not found: " + wfpath);

            WaveFile wf = new WaveFile();
            wf.Read(wfpath);

            _samplingRate = (float)wf.SamplingRate;
            _duration = wf.Duration;

//            Debug.Log(wfpath + ": " + _duration + "s @ " + _samplingRate + " Hz");

            if (wf.NumChannels == 1 || destination.ToLower().Contains("left"))
            {
                _wavData = wf.GetChannel(0);
            }
            else
            {
                _wavData = wf.GetChannel(1);
            }

            _references = wf.References;
        }

        string ConstructFilePath(string filename)
        {
            string newPath = filename;
            if (!string.IsNullOrEmpty(_wavFolder))
            {
                string fn = Path.GetFileName(filename);
                newPath = Path.Combine(_wavFolder, fn);
            }
            return newPath;
        }

        private void ComputeReferences(Level level, float Fs)
        {
            var externalRefs = UserFileReferences.Read(ConstructFilePath(fileName));

            _ref_dBV = externalRefs.GetReference("dB_Vrms");

            if (!float.IsNaN(_ref_dBV))
            { }
            else if (_references != null && _references.ContainsKey("ref_dBV"))
                _ref_dBV = _references["ref_dBV"];
            else
                _ref_dBV = KMath.RMS_dB(_wavData);

            _ref_dB = _ref_dBV;
            _maxLevel = _ref_dBV;

            bool haveSPLref = false;
            if (level.Units == LevelUnits.dB_SPL)
            {
                var extVal = externalRefs.GetReference(level.Units.ToString(), level.Transducer, level.Destination);
                if (!float.IsNaN(extVal))
                {
                    _ref_dB = extVal;
                    _maxLevel = _ref_dB;
                    haveSPLref = true;
                }
                else if (_references != null)
                {
                    string key = level.Transducer + ":SPL";

                    if (!_references.ContainsKey(key)) key = "refSPL";

                    if (_references.ContainsKey(key))
                    {
                        _ref_dB = _references[key];
                        _maxLevel = _ref_dB;
                        haveSPLref = true;
                        //Debug.Log(key + ": " + _ref_dB);
                    }
                }

                if (!haveSPLref && !canComputeReference)
                    throw new ApplicationException(".wav file does not contain a reference level for '" + level.Transducer + "'.");
            }

            if ((level.Units == LevelUnits.dB_SPL && !haveSPLref) || level.Units == LevelUnits.dB_HL || level.Units == LevelUnits.dB_SL)
            {
                if (level.Cal == null)
                    throw new ApplicationException("Null calibration for " + level.Transducer + ".");

                float deltaRef = _ref_dBV - KMath.RMS_dB(_wavData);

                _ref_dB = level.Cal.GetReference(_wavData, Fs) + deltaRef;
                _maxLevel = level.Cal.GetMax(_wavData, Fs) + deltaRef;
            }
        }

        override public References Create(float[] data)
        {
            if (!(oneShot && _playedOnce))
            {
                for (int k = 0; k < Npts; k++)
                {
                    if (_curIndex >= _offset)
                    {
                        data[k] = _wavData[_curIndex - _offset];
                    }
                    if (++_curIndex == _wavData.Length + _offset)
                    {
                        _curIndex = 0;
                        _playedOnce = true;
                        if (oneShot) break;
                    }
                }
            }
            return new References(_ref_dB, _maxLevel);
        }
    }
}
