using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;

using UnityEngine;

// https://www.codeproject.com/Articles/24332/Creating-a-Custom-Collection-for-Use-in-a-Property

namespace Turandot.Screen
{
    public class ManikinSpec
    {
        public string Name { get; set; }
        private bool ShouldSerializeName() { return false; }
        public string Label { get; set; }
        private bool ShouldSerializeLabel() { return false; }
        public string Image { get; set; }
        private bool ShouldSerializeImage() { return false; }
        public float StartPosition { get; set; }
        private bool ShouldSerializeStartPosition() { return false; }


        public ManikinSpec()
        {
            Name = "Manikin";
            Label = "Label";
            Image = "Image";
            StartPosition = 0.5f;
        }
    }

    public class ManikinCollection : CollectionBase
    {
        public ManikinSpec this[int index]
        {
            get { return (ManikinSpec)List[index]; }
        }
        public void Add(ManikinSpec ms)
        {
            List.Add(ms);
        }
        public void Remove(ManikinSpec ms)
        {
            List.Remove(ms);
        }
    }

    public class ManikinCollectionEditor : CollectionEditor
    {
        public ManikinCollectionEditor(Type type) : base(type)
        {
        }

        protected override string GetDisplayText(object value)
        {
            ManikinSpec item = new ManikinSpec();
            item = (ManikinSpec)value;

            return base.GetDisplayText(item.Name);
        }
    }
}
