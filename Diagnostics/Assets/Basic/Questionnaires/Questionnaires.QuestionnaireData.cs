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
            Debug.Log("hello");
            Name = questionnaire.Title;
            Debug.Log("it's me");
            foreach (Question q in questionnaire.Questions)
            {
                Debug.Log($"add a question: {q.Prompt}");
                responses.Add(new Response(q.Prompt));
            }
        }
    }

}