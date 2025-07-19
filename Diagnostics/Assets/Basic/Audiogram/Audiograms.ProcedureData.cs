using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_METRO && !UNITY_EDITOR
using LegacySystem.IO;
#else
using System.IO;
#endif

using KLib;
using KLib.Signals;
using KLib.Signals.Waveforms;

using Newtonsoft.Json;
using ProtoBuf;

namespace Audiograms
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class ProcedureData
    {
        public AudiogramData audiogramData = new AudiogramData();
        public List<TrackData> tracks = new List<TrackData>();
    }
}