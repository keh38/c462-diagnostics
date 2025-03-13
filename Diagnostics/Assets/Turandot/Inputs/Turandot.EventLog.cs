using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class EventLog
    {
        public string[] controlNames;
        public string[] eventNames;
        public float[] t;
        public int[][] controls;
        public int[][] events;

        [JsonIgnore]
        private int _numControls;
        [JsonIgnore]
        private int _numEvents;
        [JsonIgnore]
        private int _index;
        [JsonIgnore]
        private int _lengthIncrement;

        public EventLog() : this(10000)
        {
        }

        public EventLog(int lengthIncrement)
        {
            _lengthIncrement = lengthIncrement;
            _numControls = 0;
        }

        public void Initialize(string[] controlNames, string[] eventNames)
        {
            t = new float[_lengthIncrement];

            _numControls = controlNames.Length;
            this.controlNames = new string[_numControls];
            for (int k = 0; k < _numControls; k++)
            {
                this.controlNames[k] = controlNames[k];
            }

            _numEvents = eventNames.Length;
            this.eventNames = new string[_numEvents];
            for (int k = 0; k < _numEvents; k++)
            {
                this.eventNames[k] = eventNames[k];
            }

            Clear();

        }

        public void Clear()
        {
            t = new float[_lengthIncrement];

            controls = new int[_numControls][];
            for (int k = 0; k < _numControls; k++)
            {
                this.controlNames[k] = controlNames[k];
                controls[k] = new int[_lengthIncrement];
            }

            events = new int[_numEvents][];
            for (int k = 0; k < _numEvents; k++)
            {
                this.eventNames[k] = eventNames[k];
                events[k] = new int[_lengthIncrement];
            }

            _index = 0;

        }

        public int Length()
        {
            return _index;
        }

        public void Add(float t, int[] controlValues, int[] eventValues)
        {
            if (_index == this.t.Length)
            {
                int newLen = this.t.Length + _lengthIncrement;
                System.Array.Resize(ref this.t, newLen);
                for (int k = 0; k < _numControls; k++)
                {
                    System.Array.Resize(ref this.controls[k], newLen);
                }
                for (int k = 0; k < _numEvents; k++)
                {
                    System.Array.Resize(ref this.events[k], newLen);
                }

            }
            this.t[_index] = t;
            for (int k = 0; k < _numControls; k++)
            {
                controls[k][_index] = controlValues[k];
            }
            for (int k = 0; k < _numEvents; k++)
            {
                events[k][_index] = eventValues[k];
            }


            ++_index;
        }

        public void Trim()
        {
            System.Array.Resize(ref this.t, _index);
            for (int k = 0; k < _numControls; k++)
            {
                System.Array.Resize(ref this.controls[k], _index);
            }
            for (int k = 0; k < _numEvents; k++)
            {
                System.Array.Resize(ref this.events[k], _index);
            }
        }

    }

}