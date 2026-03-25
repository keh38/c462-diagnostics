using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;


namespace SpeechReception
{
    public enum Order { Sequential, FullRandom, BlockRandom }

    public class Sequence
    {
        public Order Order { get; set; }
        private bool ShouldSerializeOrder() { return false; }

        public int RepeatsPerBlock { get; set; }
        private bool ShouldSerializeRepeatsPerBlock() { return false; }

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
        public int ItemsPerBlock
        {
            get { return _itemsPerBlock; }
            set { _itemsPerBlock = value; }
        }

    }
}
