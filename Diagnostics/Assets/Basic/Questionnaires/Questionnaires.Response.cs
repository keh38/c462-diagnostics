using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using ProtoBuf;

using ExtensionMethods;

namespace Questionnaires
{
    /// <summary>
    /// Represents response to a questionnaire item.
    /// </summary>
    [System.Serializable]
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class Response
    {
        public string question;
        public List<int> selectionNumbers;
        public List<string> selectionValues;

        public Response() { }

        public Response(string question)
        {
            this.question = question;

            selectionNumbers = new List<int>();
            selectionValues = new List<string>();
        }


        /// <summary>
        /// Gets a value indicating whether this <see cref="Response"/> is answered.
        /// </summary>
        /// <value><c>true</c> if answered; otherwise, <c>false</c>.</value>
        [JsonIgnore]
        [ProtoIgnore]
        public bool Answered
        {
            get { return this.selectionNumbers.Count > 0; }
        }
    }
}