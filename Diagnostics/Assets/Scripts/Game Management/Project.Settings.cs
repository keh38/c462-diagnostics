using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Project
{ 
    [Serializable]
    public class Settings
    {
        public string DefaultTransducer { set; get; } = "HD280";
        public List<string> ValidTransducers { set; get; } = null;
    }
}
