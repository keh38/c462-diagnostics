using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using KLib.Signals.Enumerations;

namespace SpeechReception
{
    [System.Serializable]
    public class TransducerData
    {
        public string name = "Bose";
        public float dB_SPL = float.NaN;
        public float dB_HL = float.NaN;
        public float dB_Vrms = float.NaN;
    }

    [System.Serializable]
    public class References
    {
        public List<TransducerData> Transducers = new List<TransducerData>();

        private string _name;

        public References() { }

        public References(string folder, string name)
        {
            Open(folder, name);
        }

        public References(string referencePath)
        {
            string folder = Path.GetDirectoryName(referencePath);
            _name = Path.GetFileNameWithoutExtension(referencePath);

            References r = KLib.FileIO.XmlDeserialize<References>(referencePath);
            this.Transducers = r.Transducers;
        }

        private void Open(string folder, string name)
        {
            _name = name;
            References r = KLib.FileIO.XmlDeserialize<References>(Path.Combine(folder, name + "_References.xml"));
            this.Transducers = r.Transducers;
        }

        public bool HasReferenceFor(string transducer, string units)
        {
            var data = Transducers.Find(o => o.name == transducer);
            return data != null;
        }

        public float GetReference(string transducer, string units)
        {
            float refVal = float.NaN;

            TransducerData data = Transducers.Find(o => o.name == transducer);
            if (data != null)
            {
                switch (units)
                {
                    case "dBSPL":
                    case "dB_SPL":
                        refVal = data.dB_SPL;
                        break;
                    case "dBHL":
                    case "dB_HL":
                        refVal = data.dB_HL;
                        break;
                    case "dB_Vrms":
                        refVal = data.dB_Vrms;
                        break;
                }
            }

            if (float.IsNaN(refVal)) throw new System.Exception(_name + ": " + units + " reference not found for " + transducer);

            return refVal;
        }

    }

}