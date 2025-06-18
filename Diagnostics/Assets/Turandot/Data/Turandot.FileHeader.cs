using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

using Newtonsoft.Json;
using ProtoBuf;

namespace Turandot
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    [JsonObject(MemberSerialization.OptOut)]
    public class FileHeader
    {
        public string programName;
        public string version;
        public string date;
        public string filePath="";
        public string parameterFile="";
        public string note;
        public int audioSamplingRate = -1;
        public int audioBufferLength = -1;
        public int audioNumBuffers = -1;
        public string screenColor = "";
        public string ledColor = "";

        public FileHeader()
        {
        }

        public void Initialize(string filePath, string paramFile)
        {
            programName = UnityEngine.Application.productName;
            version = UnityEngine.Application.version;
            date = DateTime.Now.ToString();
            //note = GameManager.Note;
            this.filePath = filePath;
            this.parameterFile = paramFile;
        }

    }

}