using Microsoft.Graph;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Xml.Serialization;
using UnityEngine;

namespace Questionnaires
{
    //[XmlInclude(typeof(AgeQuestion))]
    //[XmlInclude(typeof(MedsQuestion))]
    //[XmlInclude(typeof(Slider))]
    public class Question
    {
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Prompt { get; set; }
        private bool ShouldSerializePrompt() { return false; }

        [DisplayName("Select multiple")]
        [Description("If true, subject can select multiple options")]
        public bool AllowMultipleSelections { get; set; }
        private bool ShouldSerializeAllowMultipleSelections() { return false; }

        public BindingList<string> Options { get; set; }
        private bool ShouldSerializeOptions() { return false; }

        //public QuestionType type;
        //public string prompt;
        //public bool single = true;
        //public List<string> choices = new List<string>();
        //public List<Wheel> wheel = new List<Wheel>();
        //public ConditionalSkip conditionalSkip = new ConditionalSkip();
        //public int onBackSkipN = 0;
        //public string category;
        //public Exclusion exclusion = null;
        //public int showTextIfAnswerIs = 0;

        public Question()
        {
            Prompt = "";
            Options = new BindingList<string>();
        }
    }

}