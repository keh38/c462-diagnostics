using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Protocols
{
    public class Protocol
    {
        public string Title { get; set; }
        public Appearance Appearance { get; set; }
        public string Introduction { get; set; }
        public bool FullAuto {  get; set; }

        public List<ProtocolEntry> Tests { get; set; }

        public Protocol()
        {
            Appearance = new Appearance();
            FullAuto = false;
        }

    }

}