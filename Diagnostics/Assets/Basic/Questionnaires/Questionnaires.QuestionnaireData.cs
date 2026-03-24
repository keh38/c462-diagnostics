using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Questionnaires
{
    public class QuestionnaireData
    {
        public string Name;
        public int Num;
        public List<Response> responses = new List<Response>();

        public QuestionnaireData() { }

        public QuestionnaireData(Questionnaire questionnaire)
        {
            Name = questionnaire.Title;
            foreach (Question q in questionnaire.Questions)
            {
                responses.Add(new Response(q.Prompt));
            }
        }
    }

}