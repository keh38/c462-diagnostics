using System;
using System.Collections;
using System.Collections.Generic;
using KLib.Signals;
using KLibU;
using KLib;
using KLib.Expressions;
using Unity.VisualScripting;
using System.IO;
using UnityEngine;
using Protocols;
using System.Text;

namespace Turandot.Schedules
{
    public enum TestedEars { None, Left, Right, Both}

    public class ScriptArguments
    {
        public Laterality laterality;
        public VarDimension dimension;
        public string expression;
        public string flag;
        public int value;

        public ScriptArguments()
        {
            laterality = Laterality.None;
            dimension = VarDimension.X;
        }
    }

    public class Script
    {
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }

        public string ProtocolRootName { get; set; }
        private bool ShouldSerializeProtocolRootName() { return false; }

        public bool SingleProtocolFile { get; set; }
        private bool ShouldSerializeSingleProtocolFile() { return false; }

        public List<string> ConfigFiles { get; set; }
        private bool ShouldSerializeConfigFiles() { return false; }

        public TestedEars TestedEars { get; set; }

        private bool ShouldSerializeTestedEars() { return false; }
        public string Groups { get; set; }
        private bool ShouldSerializeGroups() { return false; }

        public VarDimension Dim { get; set; }
        private bool ShouldSerializeDim() { return false; }

        public string Expression { get; set; }
        private bool ShouldSerializeExpression() { return false; }

        public Order Order { get; set; }
        private bool ShouldSerializeOrder() { return false; }

        public int SplitAfter { get; set; }
        private bool ShouldSerializeSplitAfter() { return false; }

        public Script()
        {
            Name = "Untitled";
            ProtocolRootName = "";
            SingleProtocolFile = false;
            ConfigFiles = new List<string>();
            TestedEars = TestedEars.None;
            Groups = "";
            Dim = VarDimension.X;
            Expression = "";
            Order = Order.Interleave;
            SplitAfter = 1;
        }

        public void Apply(string protocolFolder)
        {
            if (ConfigFiles.Count == 0) return;

            float[] values = null;
            int[] groups = new int[] { 0 };

            if (!string.IsNullOrEmpty(Expression))
            {
                values = Expressions.Evaluate(Expression);
                if (Order == Order.Random || Order == Order.Interleave)
                {
                    KMath.Permute(values);
                }
            }

            if (!string.IsNullOrEmpty(Groups))
            {
                groups = Expressions.EvaluateToInt(Groups);
            }

            int nfile = 1;
            int nperFile = 1;
            if (values != null)
            {
                nperFile = values.Length;
                if (values.Length > 0 && SplitAfter > 0)
                {
                    nfile = Mathf.CeilToInt(values.Length / SplitAfter);
                    nperFile = SplitAfter;
                }
            }

            string protocolRootName = Name;
            if (!string.IsNullOrEmpty(ProtocolRootName))
            {
                protocolRootName = ProtocolRootName;
            }

            List<ProtocolEntry> combinedEntries = new List<ProtocolEntry>();

            int i1 = 0;
            for (int k = 0; k < nfile; k++)
            {
                var args = new ScriptArguments();

                if (values != null)
                {
                    args.dimension = Dim;
                    args.expression = "[";
                    int i2 = Mathf.Min(i1 + nperFile, values.Length);
                    for (int kv = i1; kv < i2; kv++) args.expression += $"{values[kv]} ";
                    args.expression += "]";
                    i1 = i2;
                }

                if (TestedEars == TestedEars.None)
                {
                    args.laterality = Laterality.None;
                    var entries = CreateEntries(args);

                    if (SingleProtocolFile)
                    {
                        combinedEntries.AddRange(entries);
                    }
                    else
                    {
                        string protocolName = $"{protocolRootName}-{k + 1}";
                        CreateOneProtocolFile(protocolFolder, protocolName, entries);
                    }
                }
                else
                {
                    if (TestedEars == TestedEars.Left || TestedEars == TestedEars.Both)
                    {
                        args.laterality = Laterality.Left;
                        var entries = CreateEntries(args);
                        if (SingleProtocolFile)
                        {
                            combinedEntries.AddRange(entries);
                        }
                        else
                        {
                            string protocolName = $"{protocolRootName}-{k + 1}Left";
                            CreateOneProtocolFile(protocolFolder, protocolName, entries);
                        }
                    }
                    if (TestedEars == TestedEars.Right || TestedEars == TestedEars.Both)
                    {
                        args.laterality = Laterality.Right;
                        var entries = CreateEntries(args);
                        if (SingleProtocolFile)
                        {
                            combinedEntries.AddRange(entries);
                        }
                        else
                        {
                            string protocolName = $"{protocolRootName}-{k + 1}Right";
                            CreateOneProtocolFile(protocolFolder, protocolName, entries);
                        }
                    }
                }
                if (SingleProtocolFile)
                {
                    CreateOneProtocolFile(protocolFolder, protocolRootName, combinedEntries);
                }
            }
        }

        private List<ProtocolEntry> CreateEntries(ScriptArguments args)
        {
            List<ProtocolEntry> entries = new List<ProtocolEntry>();

            string serializedArgs = Files.JSONSerializeToString(args, Newtonsoft.Json.Formatting.None);

            foreach (string configFile in ConfigFiles)
            {
                var entry = new ProtocolEntry()
                {
                    Title = $"{configFile}-{args.laterality}-{args.expression}",
                    Scene = "Turandot",
                    Settings = $"{configFile}:{serializedArgs}"
                };
                entries.Add(entry);
            }

            return entries;
        }

        private void CreateOneProtocolFile(string folder, string name, List<ProtocolEntry> entries)
        {
            Protocol protocol = new Protocol();
            protocol.Title = name;
            protocol.FullAuto = true;
            protocol.Tests.AddRange(entries);
            Files.XmlSerialize(protocol, Path.Combine(folder, $"{name}.xml"));
        }
    }
}