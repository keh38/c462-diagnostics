using System;

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
        public string State { get; set; }
        private bool ShouldSerializeState() { return false; }

        public string Channel { get; set; }
        private bool ShouldSerializeChannel() { return false; }

        public string Property { get; set; }
        private bool ShouldSerializeProperty() { return false; }

        public ActionOperation Operation { get; set; }
        private bool ShouldSerializeOperation() { return false; }

        public float Value { get; set; }
        private bool ShouldSerializeValue() { return false; }

        public TurandotAction()
        {
            State = "";
            Channel = "";
            Property = "";
            Value = 0;
            Operation = ActionOperation.Set;
        }

        /// <summary>
        /// Constructs a new TurandotAction by duplicating the values from another object.
        /// If the input is a TurandotAction, copies all property values.
        /// </summary>
        /// <param name="obj">Object to duplicate values from.</param>
        public TurandotAction(object obj)
            : this()
        {
            if (obj is TurandotAction other)
            {
                State = other.State;
                Channel = other.Channel;
                Property = other.Property;
                Operation = other.Operation;
                Value = other.Value;
            }
            // Optionally, handle other types or throw if not supported
        }

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
