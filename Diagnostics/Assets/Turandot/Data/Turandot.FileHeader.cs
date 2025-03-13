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
        public string masterID;
        public string programName;
        public string version;
        public string date;
        public string filePath="";
        public string parameterFile="";
        public string note;
        public int audioSamplingRate = -1;
        public int audioBufferLength = -1;
        public int audioNumBuffers = -1;

        public FileHeader()
        {
        }

        public void Initialize(string filePath, string paramFile)
        {
            //masterID = SubjectManager.Instance.MasterID.ToString();
            //programName = VersionInfo.AppName;
            //version = VersionInfo.SemanticVersion;
            //date = DateTime.Now.ToString();
            //note = SubjectManager.Instance.Note;
            //this.filePath = filePath;
            //this.parameterFile = paramFile;
        }

    }

}