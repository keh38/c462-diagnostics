using System;
using System.ComponentModel;
using UnityEngine;

namespace KLib
{
    public static class Utilities
    {
        public static void AppendToJsonFile(string filePath, string jsonToAppend)
        {
            string existingJson = System.IO.File.ReadAllText(filePath);
            int insertIndex = existingJson.LastIndexOf('}');
            existingJson = existingJson.Insert(insertIndex, "," + jsonToAppend.Substring(1, jsonToAppend.Length-2));
            System.IO.File.WriteAllText(filePath, existingJson);
        }   
    }
}
