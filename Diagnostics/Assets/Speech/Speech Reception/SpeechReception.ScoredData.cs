using UnityEngine;
using System;
using System.Collections;
#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

using Newtonsoft.Json;
using ProtoBuf;

namespace SpeechReception
{
    [System.Serializable]
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    public class ScoredData
    {
        [ProtoMember(1, IsRequired = true)]
        public string date;
        [ProtoMember(2, IsRequired = true)]
        public int QuickSINA;
        [ProtoMember(3, IsRequired = true)]
        public int QuickSINB;
        [ProtoMember(4, IsRequired = true)]
        public int QuickSINC;
        [ProtoMember(5, IsRequired = true)]
        public int QuickSIND;
        [ProtoMember(6, IsRequired = true)]
        public int QuickSINTotal;
        [ProtoMember(7, IsRequired = true)]
        public int stratum;
        [ProtoMember(8, IsRequired = true)]
        public int groupNum;
        [ProtoMember(9, IsRequired = true)]
        public float avgSNR;

        public ScoredData()
        {
            QuickSINA = -1;
            QuickSINB = -1;
            QuickSINC = -1;
            QuickSIND = -1;
            stratum = -1;
            groupNum = -1;
        }

        [JsonIgnore]
        [ProtoIgnore]
        public bool IsValid
        {
            get
            {
                return (
                  ValidQuickSIN(QuickSINA) &&
                  ValidQuickSIN(QuickSINB) &&
                  ValidQuickSIN(QuickSINC));
                    }
        }

        public bool SetQuickSINA(int value)
        {
            QuickSINA = value;
            return ValidQuickSIN(value);
        }
        public bool ValidQuickSIN(int value)
        {
            return value >= 0; // && QuickSIN <= 30;
        }
        public bool SetQuickSINB(int value)
        {
            QuickSINB = value;
            return ValidQuickSIN(value);
        }
        public bool SetQuickSINC(int value)
        {
            QuickSINC = value;
            return ValidQuickSIN(value);
        }
        public bool SetQuickSIND(int value)
        {
            QuickSIND = value;
            return true;
        }

        public float ComputeAvgSNR()
        {
            float sum = QuickSINA + QuickSINB + QuickSINC;
            float n = 3;

            if (QuickSIND >= 0)
            {
                sum += QuickSIND;
                ++n;
            }

            avgSNR = 27.5f - sum / n;
            return avgSNR;
        }

    }
}