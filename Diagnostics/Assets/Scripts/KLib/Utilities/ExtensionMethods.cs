using UnityEngine;
using System.Collections.Generic;

using KLib.Signals.Enumerations;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static void MakePiecewiseLinear(this AnimationCurve ac)
        {
            float inTangent = float.NaN;
            float outTangent = float.NaN;

            for (int k=0; k<ac.keys.Length; k++)
            {
                Keyframe key = ac[k];

                // backward
                if (k > 0)
                {
                    float dx = ac[k].time - ac[k - 1].time;
                    float dy = ac[k].value - ac[k - 1].value;
                    inTangent = dy / dx;
                }
                // forward
                if (k < ac.keys.Length-1)
                {
                    float dx = ac[k+1].time - ac[k].time;
                    float dy = ac[k+1].value - ac[k].value;
                    outTangent = dy / dx;
                }

                key.inTangent = inTangent;
                key.outTangent = outTangent;
                ac.MoveKey(k, key);

            }
        }

        public static bool IsOneOf<T>(this T e, params T[] x) //where T : struct, System.IConvertible
        {
#if UNITY_EDITOR
            if (!e.GetType().IsEnum)
                throw new System.Exception("Input is not an enum.");
#endif

            return (new List<T>(x).Contains(e));
        }

        public static float ToBalance(this SpeechReception.TestEar testEar)
        {
            float balance = 0f;
            switch (testEar)
            {
                case SpeechReception.TestEar.Left:
                    balance = -1f;
                    break;
                case SpeechReception.TestEar.Right:
                    balance = 1f;
                    break;
                case SpeechReception.TestEar.Binaural:
                case SpeechReception.TestEar.SubjectDefault:
                    balance = 0f;
                    break;
            }
            return balance;
        }

        //public static Laterality ToLaterality(this Audiograms.Ear ear)
        //{
        //    return ear == Audiograms.Ear.Left ? Laterality.Left : Laterality.Right;
        //}

        //public static Audiograms.Ear ToEar(this Laterality laterality)
        //{
        //    return laterality == Laterality.Left ? Audiograms.Ear.Left : Audiograms.Ear.Right;
        //}

        //public static float ToBalance(this Laterality laterality)
        //{
        //    float b = 0;
        //    switch (laterality)
        //    {
        //        case Laterality.Left:
        //            b = -1;
        //            break;
        //        case Laterality.Right:
        //            b = 1;
        //            break;
        //        default:
        //            b = 0;
        //            break;
        //    }
        //    return b;
        //}

        public static string ToHex(this string s)
        {
            char[] carr = s.ToCharArray();
            string h = "";
            foreach (char c in carr)
            {
                h += ((byte)c).ToString("X2");
            }
            return h;
        }

        public static string FromHex(this string s)
        {
            char[] carr = new char[s.Length / 2];
            for (int k = 0; k < carr.Length; k++)
            {
                carr[k] = (char)System.Convert.ToByte(s.Substring(2 * k, 2), 16);
            }
            return new string(carr);
        }

        public static T GetRandom<T>(this List<T> list)
        {
            int rn = Random.Range((int)0, (int)list.Count);
            return list[rn];
        }

        public static string GetRandom(this List<string> list)
        {
            int rn = Random.Range((int)0, (int)list.Count);
            return list[rn];
        }
        public static int GetRandom(this List<int> list)
        {
            int rn = Random.Range((int)0, (int)list.Count);
            return list[rn];
        }
        public static float GetRandom(this float[] list)
        {
            int rn = Random.Range((int)0, (int)list.Length);
            return list[rn];
        }
        public static int GetRandom(this int[] list)
        {
            int rn = Random.Range((int)0, (int)list.Length);
            return list[rn];
        }

        public static string PopRandom(this List<string> list)
        {
            int rn = Random.Range((int)0, (int)list.Count);
            string value = list[rn];
            list.RemoveAt(rn);
            return value;
        }
        public static int PopRandom(this List<int> list)
        {
            int rn = Random.Range((int)0, (int)list.Count);
            int value = list[rn];
            list.RemoveAt(rn);
            return value;
        }

        public static Vector3 FindClosestTo(this List<Vector3> list, Vector3 vtest)
        {
            Vector3 closest = new Vector3();
            float minDist = float.PositiveInfinity;

            foreach (Vector3 v in list)
            {
                float d = (vtest - v).sqrMagnitude;
                if (d < minDist)
                {
                    minDist = d;
                    closest = v;
                }
            }
            return closest;
        }

        public static float XYMagnitude(this Vector3 v)
        {
            return Mathf.Sqrt(v.x * v.x + v.y * v.y);
        }

        public static List<string> Unique(this List<string> list)
        {
            List<string> u = new List<string>();
            foreach(string s in list)
            {
                if (u.FindIndex(o => o == s) < 0) u.Add(s);
            }
            return u;
        }

    }
}
