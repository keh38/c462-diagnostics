using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

using KLib.Signals;

namespace Bekesy
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class TrackData
    {
        public Laterality ear;
        public float Freq_Hz;
        public bool completed;
        public TrackLog log;

        public TrackData() { }

        public TrackData(Laterality ear, float Freq_Hz)
        {
            this.ear = ear;
            this.Freq_Hz = Freq_Hz;

            this.completed = false;
            this.log = new TrackLog();
        }

        public void Complete()
        {
            completed = true;
            log.Trim();
        }

        public float ComputeThreshold()
        {
            float sum = 0;
            int n = 0;
            for (int k = 0; k < log.Length; k++)
            {
                if (log.reversal[k] > 0)
                {
                    sum += log.level[k];
                    n++;
                }
            }

            float threshold = float.NaN;
            if (n > 0)
            {
                threshold = sum / n;
            }

            return threshold;
        }
    }
}