using System;
using System.ComponentModel;

using Newtonsoft.Json;
using OrderedPropertyGrid;

using KLib.Signals;
using KLib.TypeConverters;

namespace Turandot
{
    /// <summary>
    /// Represents an action with state, channel, property, value, and operation.
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    [TypeConverter(typeof(SortableTypeConverter))]
    public class TurandotAction
    {
        [Category("Action")]
        [PropertyOrder(0)]
        public string State { get; set; }
        private bool ShouldSerializeState() { return false; }

        [Category("Action")]
        [PropertyOrder(1)]
        public string Channel { get; set; }
        private bool ShouldSerializeChannel() { return false; }

        [Category("Action")]
        [PropertyOrder(2)]
        public string Property { get; set; }
        private bool ShouldSerializeProperty() { return false; }

        [Category("Action")]
        [PropertyOrder(3)]
        public ActionOperation Operation { get; set; }
        private bool ShouldSerializeOperation() { return false; }

        [Category("Action")]
        [PropertyOrder(4)]
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
