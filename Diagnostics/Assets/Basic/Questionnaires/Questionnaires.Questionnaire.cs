using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Xml.Serialization;
using Turandot.Screen;
using UnityEngine;

using BasicMeasurements;

namespace Questionnaires
{
    [System.Serializable]
    public class Questionnaire : BasicMeasurementConfiguration
    {
        [Category("Appearance")]
        public string Title { get; set; }
        private bool ShouldSerializeTitle() { return false; }

        [Category("Appearance")]
        [DisplayName("Question font size")]
        public int FontSize { get; set; }
        private bool ShouldSerializeFontSize() { return false; }

        [Category("Content")]
        [Editor(typeof(QuestionCollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public QuestionCollection Questions { get; set; }
        private bool ShouldSerializeQuestions {  get; set; }

        //public bool saveLocalCopy = false;
        //public bool useOSK = false;
        //public string linkTo = "";
        //public List<string> instructions = new List<string>();
        //public List<Question> questions = new List<Question>();

        public Questionnaire()
        {
            Title = "Questionnaire";
            FontSize = 60;
            Questions = new QuestionCollection();
        }
    }

    public class QuestionCollection : CollectionBase
    {
        public Question this[int index]
        {
            get { return (Question)List[index]; }
        }
        public void Add(Question question)
        {
            List.Add(question);
        }
        public void Remove(Question question)
        {
            List.Remove(question);
        }
    }

    public class QuestionCollectionEditor : CollectionEditor
    {
        public QuestionCollectionEditor(Type type) : base(type) { }

        protected override string GetDisplayText(object value)
        {
            Question item = new Question();
            item = (Question)value;

            return base.GetDisplayText(item.Prompt);
        }
    }


}