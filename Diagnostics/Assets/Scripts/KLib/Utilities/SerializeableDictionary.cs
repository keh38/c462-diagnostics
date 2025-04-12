using System.Collections.Generic;
using System.Linq;

namespace KLib
{
    public class SerializeableDictionary<T>
    {
        public class Entry
        {
            public string key;
            public T value;
            public Entry() { }
        }
        public List<Entry> entries = new List<Entry>();
        public T this[string key]
        {
            get
            {
                T result = default(T);
                var entry = entries.Find(x => x.key.Equals(key));
                if (entry != null)
                {
                    result = entry.value;
                }
                return result;
            }
            set
            {
                if (string.IsNullOrEmpty(key)) return;

                var e = entries.Find(x => x.key.Equals(key));
                if (e == null)
                {
                    entries.Add(new Entry() { key = key, value = value });
                }
                else
                {
                    e.value = value;
                }
            }
        }

        public void RenameKey(int index, string newName)
        {
            if (index < entries.Count)
            {
                entries[index].key = newName;
            }
        }

        public void RenameKey(string oldName, string newName)
        {
            var e = entries.Find(x => x.key.Equals(oldName));
            if (e != null)
            {
                e.key = newName;
            }
        }

        public void RemoveKey(string key)
        {
            var e = entries.Find(x => x.key.Equals(key));
            if (e != null)
            {
                entries.Remove(e);
            }
        }

        public void Sort()
        {
            entries.Sort((x, y) => x.key.CompareTo(y.key));
        }

    }
}