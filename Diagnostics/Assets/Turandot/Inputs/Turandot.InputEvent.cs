using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class InputEvent
    {
        public string name;
        public List<InputCriterion> criteria = new List<InputCriterion>();

        private List<bool> _subResults = new List<bool>();

        [JsonIgnore]
        bool _lastvalue;

        public InputEvent()
        {
        }

        public InputEvent(string name)
        {
            this.name = name;
        }

        public void Reset()
        {
            foreach (InputCriterion c in criteria)
            {
                c.Reset();
            }
        }

        public void ClearRisingFalling()
        {
            if (criteria.Find(o => o.state == InputState.Rising || o.state == InputState.Falling) != null) _lastvalue = false;
        }

        public bool Value
        {
            get { return _lastvalue; }
        }

        public bool Update(List<ButtonData> data, List<Flag> flags, List<ScalarData> scalars, float elapsedTime_s)
        {
            if (name.Contains("AFC"))
            {
                return UpdateAFC(data, (int) flags.Find(o => o.name == "TestInterval").value);
            }

            _subResults.Clear();

            bool value = true;
            InputOperator nextOp = InputOperator.AND;

            foreach (InputCriterion c in criteria)
            {
                bool result = false;
                ButtonData bd = data.Find(d => d.name == c.control);
                var sd = scalars.Find(x => x.name == c.control);
                if (bd != null)
                {
                    result = c.Update(bd, elapsedTime_s);
                }
                else if (sd != null)
                {
                    result = sd.hasChanged && MakeComparison(sd.value, c.comparison, c.time_ms);
                }
                else
                {
                    Flag flag = flags.Find(f => f.name == c.control);
                    if (flag != null)
                    {
                        result = MakeComparison(flag.value, c.comparison, c.time_ms);
                    }
                }

                switch (nextOp)
                {
                    case InputOperator.None:
                    case InputOperator.AND:
                        value &= result;
                        break;
                    case InputOperator.OR:
                        _subResults.Add(value);
                        value = result;
                        break;
                }

                nextOp = c.op;

            }
            _subResults.Add(value);

            value = false;
            foreach (bool v in _subResults) value |= v;

            bool isChanged = value && !_lastvalue;
            _lastvalue = value;

            return isChanged; 
        }

        private bool MakeComparison(float a, Comparison comparison, float b)
        {
            bool result = false;
            switch (comparison)
            {
                case Comparison.LE:
                    result = a <= b;
                    break;
                case Comparison.LT:
                    result = a < b;
                    break;
                case Comparison.EQ:
                    result = a == b;
                    break;
                case Comparison.NE:
                    result = a != b;
                    break;
                case Comparison.GT:
                    result = a > b;
                    break;
                case Comparison.GE:
                    result = a >= b;
                    break;
            }
            return result;
        }

        private bool UpdateAFC(List<ButtonData> data, int testInterval)
        {
            bool value = false;

            foreach (ButtonData d in data.FindAll(o => o.name.StartsWith("Button")))
            {
                if (d.value)
                {
                    int buttonNum = int.Parse(d.name.Substring(6));
                    if (name == "AFC Correct") value = buttonNum == testInterval;
                    else value = buttonNum != testInterval;
                }
            }

            bool isChanged = value && !_lastvalue;
            _lastvalue = value;

            return isChanged;
        }

    }
}
