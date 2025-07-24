using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Bekesy
{
    [ProtoContract(ImplicitFields=ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class Data
    {
        public Audiograms.AudiogramData audiogram = null;
        public List<TrackData> tracks;

        public Data()
        {
            audiogram = new Audiograms.AudiogramData();
            tracks = new List<TrackData>();
        }

        public TrackData GetNext()
        {
            return tracks.Find(x => !x.completed);
        }

        [ProtoIgnore]
        [JsonIgnore]
        public int NumCompleted { get { return tracks.FindAll(x  => x.completed).Count; } }

        [ProtoIgnore]
        [JsonIgnore]
        public int PercentCompleted { get { return Mathf.RoundToInt(100f * NumCompleted / tracks.Count); } }
    }
}