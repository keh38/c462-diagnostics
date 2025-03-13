using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot.Schedules
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Adaptation
    {
        public int switchAfter = -1;
        public int numBlocks = 1;
        public AdaptSwitchType switchType = AdaptSwitchType.Reversals;
        public AdaptMode mode = AdaptMode.GoNoGo;
        public List<AdaptiveTrack> tracks = new List<AdaptiveTrack>();
        public float cvCriterion = 0.2f;
        public int maxExtraBlocks = 0;
        public bool randomTrackOrder = true;

        bool _isBlockFinished = false;
        int[] _iorder;

        int _currentBlock = 0;
        int _currentTrack = 0;
        int _numAcquired = 0;
        int _numExtra = 0;

        string _log = "";
        List<TrackResult> _results = new List<TrackResult>();


        public Adaptation()
        {
        }

        [JsonIgnore]
        public bool IsBlockFinished
        {
            get { return _isBlockFinished; }
        }

        [JsonIgnore]
        public bool AllBlocksFinished
        {
            get { return _currentBlock >= numBlocks; }
        }

        [JsonIgnore]
        public string Log
        {
            get { return _log; }
        }

        [JsonIgnore]
        public int BlockNumber
        {
            get { return _currentBlock; }
        }

        [JsonIgnore]
        public int TrackNumber
        {
            get { return _currentBlock * numBlocks + _currentTrack; }
        }

        [JsonIgnore]
        public bool UsesCS
        {
            get
            {
                bool usesCS = false;

                foreach (AdaptiveTrack t in tracks)
                {
                    if (t.trackedVarType == TrialType.CSplus || t.trackedVarType == TrialType.CSminus)
                    {
                        usesCS = true;
                        break;
                    }
                }

                return usesCS;
            }
        }

        [JsonIgnore]
        public int MaxNumberOfBlocks
        {
            get { return numBlocks + maxExtraBlocks; }
        }

        [JsonIgnore]
        public List<TrackResult> Results
        {
            get { return _results; }
        }

        public void Initialize()
        {
            _currentBlock = 0;
            _results.Clear();
        }

        public void InitBlock()
        {
            _isBlockFinished = false;

            if (randomTrackOrder)
                _iorder = KLib.KMath.Permute(tracks.Count);
            else
            {
                _iorder = new int[tracks.Count];
                for (int k = 0; k < tracks.Count; k++) _iorder[k] = k;
            }

            _log = "";
            _currentTrack = 0;

            foreach (AdaptiveTrack t in tracks)
            {
                t.Initialize();
                var pv = new List<KLib.Expressions.PropVal>() { new KLib.Expressions.PropVal("AV", t.startVal) };
                foreach (Variable v in t.variables)
                {
                    v.EvaluateExpression(pv);
                }
                foreach (Variable v in t.catches)
                {
                    v.EvaluateExpression();
                }
            }
        }

        public SCLElement InitTrial()
        {
            SCLElement sc = new SCLElement();

            int index = _iorder[_currentTrack];

            sc.group = tracks[index].name;
            sc.block = _currentBlock + 1;
            sc.track = _currentTrack + 1;
            sc.trial = tracks[index].NumTrials + 1;

            int ix = -1;
            int iy = -1;
            foreach (Variable v in tracks[index].variables)
            {
                if (ix < 0 && v.dim == VarDimension.X) ix = UnityEngine.Random.Range(0, v.Length);
                if (iy < 0 && v.dim == VarDimension.Y) iy = UnityEngine.Random.Range(0, v.Length);

                if (v.expression.Contains("AV"))
                {
                    v.EvaluateExpression(new List<KLib.Expressions.PropVal>() { new KLib.Expressions.PropVal("AV", tracks[index].CurrentValue) });
                }

                float value = v.GetValue(ix, iy);
                sc.propValPairs.Add(new PropValPair(v.PropertyName, value));
            }

            if (tracks[index].CurrentTrialType == "test")
            {
                sc.adapt = "test";
                sc.propValPairs.Add(new PropValPair(tracks[index].PropertyName, tracks[index].CurrentValue));
                sc.trialType = tracks[index].trackedVarType;
            }
            else
            {
                sc.adapt = "catch";
                switch (tracks[index].trackedVarType)
                {
                    case TrialType.GoNoGo:
                        sc.trialType = TrialType.GoNoGo;
                        break;
                    case TrialType.CSplus:
                        sc.trialType = TrialType.CSminus;
                        break;
                    case TrialType.CSminus:
                        sc.trialType = TrialType.CSplus;
                        break;
                }
                float[] catchVals = tracks[index].CatchValues;

                for (int k = 0; k < tracks[index].catches.Count; k++)
                {
                    sc.propValPairs.Add(new PropValPair(tracks[index].catches[k].PropertyName, catchVals[k]));
                }
            }

            return sc;
        }

#if KDEBUG
        public string SimulatedResult(TrialType trialType, float threshold)
        {
            int index = _iorder[_currentTrack];
            return tracks[index].SimulatedResult(trialType, threshold);
        }
#endif

        public void Process(string response)
        {
            string pattern = @"outcome=""([a-zA-Z]+)"";";
            Match m = Regex.Match(response, pattern);
            if (m.Success) response = m.Groups[1].Value;

            int index = _iorder[_currentTrack];

            AdaptTrialResult result = tracks[index].ProcessResponse(response);

            if (result.trackFinished && !result.outOfRange)
            {
                tracks[index].ComputeThreshold(result.userStop);

                _results.Add(new TrackResult(tracks[index].name, tracks[index].Threshold));
            }

            Debug.Log(result.log);
            _log += "Trial #" + tracks[index].NumTrials + "\n" + result.log + "\n"; ;

            _isBlockFinished = tracks.Find(t => !t.IsFinished) == null;
            if (!_isBlockFinished)
            {
                tracks[index].SetNextTrial(result.valueChange);

                _isBlockFinished = CheckForTrackSwitch(result);
            }

            if (_isBlockFinished)
            {
                _currentBlock++;
            }
        }

        bool CheckForTrackSwitch(AdaptTrialResult result)
        {
            bool allFinished = false;

            if (switchType == AdaptSwitchType.Reversals && result.reversal) _numAcquired++;
            if (switchType == AdaptSwitchType.Trials) _numAcquired++;

            // There must be at least one track without a threshold

            if ((switchAfter > 0 && _numAcquired >= switchAfter) || result.trackFinished)
            {
                int nextTrack = -1;
                for (int k = _currentTrack + 1; k < tracks.Count; k++)
                {
                    int index = _iorder[k];
                    if (float.IsNaN(tracks[index].FinalValue))
                    {
                        nextTrack = k;
                        break;
                    }
                }

                if (nextTrack < 0)
                {
                    for (int k = 0; k <= _currentTrack; k++)
                    {
                        int index = _iorder[k];
                        if (float.IsNaN(tracks[index].FinalValue))
                        {
                            nextTrack = k;
                            break;
                        }
                    }
                }

                allFinished = nextTrack < 0;
                if (nextTrack >= 0)
                {
                    _log += "\nSwitch track: " + nextTrack + "\n";
                    _currentTrack = nextTrack;
                    _numAcquired = 0;
                }
            }

            return allFinished;
        }

        public bool CheckTrackConsistency()
        {
            if (_numExtra >= maxExtraBlocks) return true;

            bool ok = true;

            foreach (AdaptiveTrack t in tracks)
            {
                float[] thr = _results.FindAll(r => r.name == t.name).Select(s => s.threshold).ToArray();
                float cv = KLib.KMath.CoeffVar(thr);
                Debug.Log("CV = " + cv);

                if (cv > cvCriterion)
                {
                    ok = false;
                    break;
                }
            }

            if (!ok)
            {
                _log += "Failed CV criterion. Adding block." + System.Environment.NewLine;
                numBlocks++;
                _numExtra++;
            }

            return ok;
        }

        public List<TrackResult> ComputeFinalThresholds()
        {
            List<TrackResult> finalResults = new List<TrackResult>();
            foreach (AdaptiveTrack at in tracks)
            {
                float[] thr = _results.FindAll(r => r.name == at.name).Select(s => s.threshold).ToArray();

                float overallThr = float.NaN;
                if (at.computation == AdaptComputation.Mean) overallThr = KLib.KMath.Mean(thr);
                else if (at.computation == AdaptComputation.Median) overallThr = KLib.KMath.Median(thr);
                _log += at.name + " threshold = " + overallThr + System.Environment.NewLine;

                finalResults.Add(new TrackResult(at.name, overallThr));
            }

            return finalResults;
        }
    }
}
