using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;


namespace SpeechReception
{
    [System.Serializable]
    public class Sequence
    {
        public enum Order { Default, Sequential, FullRandom, BlockRandom}

        public Order order = Order.Sequential;
        public int repeatsPerBlock = 1;
        public int numBlocks = 1;
        public int choose = -1;

        private int _itemsPerBlock;

        [XmlIgnore]
        public int ItemsPerBlock
        {
            get { return _itemsPerBlock; }
            set { _itemsPerBlock = value; }
        }

    }
}
