using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

using OrderedPropertyGrid;

namespace SpeechReception
{
    public enum Order { Sequential, FullRandom, BlockRandom }

    [TypeConverter(typeof(SequenceTypeConverter))]
    public class Sequence
    {
        [Browsable(false)]
        public Order Order { get; set; }
        private bool ShouldSerializeOrder() { return false; }

        [PropertyOrder(1)]
        public int RepeatsPerBlock { get; set; }
        private bool ShouldSerializeRepeatsPerBlock() { return false; }

        [PropertyOrder(2)]
        public int NumBlocks { get; set; }
        private bool ShouldSerializeNumBlocks() { return false; }

        public int choose = -1;

        private int _itemsPerBlock;

        public Sequence()
        {
            Order = Order.Sequential;
            RepeatsPerBlock = 1;
            NumBlocks = 1;
        }

        [XmlIgnore]
        [Browsable(false)]
        public int ItemsPerBlock
        {
            get { return _itemsPerBlock; }
            set { _itemsPerBlock = value; }
        }

    }
}
