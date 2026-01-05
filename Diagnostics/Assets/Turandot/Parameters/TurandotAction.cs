using System;
using System.ComponentModel;

using Newtonsoft.Json;

using KLib.Signals;

namespace Turandot
{
    /// <summary>
    /// Represents an action with state, channel, property, value, and operation.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class TurandotAction
    {
        [Category("Action")]
        public string State { get; set; }
        private bool ShouldSerializeState() { return false; }

        [Category("Action")]
        public string Channel { get; set; }
        private bool ShouldSerializeChannel() { return false; }

        [Category("Action")]
        public string Property { get; set; }
        private bool ShouldSerializeProperty() { return false; }

        [Category("Action")]
        public float Value { get; set; }
        private bool ShouldSerializeValue() { return false; }

        [Category("Action")]
        public ActionOperation Operation { get; set; }
        private bool ShouldSerializeOperation() { return false; }

        public TurandotAction() { }

        public void ApplyAction(SignalManager sigMan)
        {
            if (sigMan == null)
            {
                throw new ArgumentNullException(nameof(sigMan), "SignalManager cannot be null.");
            }
            float currentValue = sigMan.GetParameter(Channel, Property);
            switch (Operation)
            {
                case ActionOperation.Add:
                    sigMan.SetParameter(Channel, Property, currentValue + Value);
                    break;
                case ActionOperation.Subtract:
                    sigMan.SetParameter(Channel, Property, currentValue - Value);
                    break;
                case ActionOperation.Set:
                    sigMan.SetParameter(Channel, Property, Value);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported ActionOperation.");
            }
        }

    }
}
