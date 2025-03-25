using System.Collections.Generic;

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
    }
}