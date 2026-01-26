using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Digits
{
    [JsonObject(MemberSerialization.OptOut)]
    public class Trial
    {
        public int[] F01;
        public int[] M01;
        public int[] M02;
        public int[] Response;

        public int NumCorrect()
        {
            int n = 0;
            for (int k = 0; k < Response.Length; k++)
            {
                if (Response[k] == M02[k])
                {
                    ++n;
                }
            }

            return n;
        }
    }
}
