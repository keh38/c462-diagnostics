using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    [Serializable]
    public class InputCriterion
    {
        public string control;
        public InputState state;
        public float time_ms;
        public InputOperator op;
        public Comparison comparison = Comparison.EQ;

        [JsonIgnore]
        float trueTime_ms;

        [JsonIgnore]
        bool _lastValue;

        public InputCriterion() { }

        public void Reset()
        {
            _lastValue = false;
            trueTime_ms = 0;
        }

        public bool Update(ButtonData data, float elapsedTime)
        {
            bool result = false;
            switch (state)
            {
                case InputState.High:
                    if (data.value)
                    {
                        trueTime_ms += 1000 * elapsedTime;
                    }
                    else
                    {
                        trueTime_ms = 0;
                    }
                    result = trueTime_ms > time_ms;
                    break;

                case InputState.Low:
                    if (!data.value)
                    {
                        trueTime_ms += 1000 * elapsedTime;
                    }
                    else
                    {
                        trueTime_ms = 0;
                    }
                    result = trueTime_ms > time_ms;
                    break;

                case InputState.Rising:
                    result = data.value && !_lastValue;
                    break;

                case InputState.Falling:
                    result = !data.value && _lastValue;
                    break;

            }

            _lastValue = data.value;
            return result;
        }

    }
}
