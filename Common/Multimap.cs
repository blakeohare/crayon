using System.Collections;
using System.Collections.Generic;

namespace Common
{
    public class Multimap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        private TValue[] EMPTY_BUCKET = new TValue[0];
        private Dictionary<TKey, List<TValue>> lookup = new Dictionary<TKey, List<TValue>>();

        public Multimap() { }
        
        public IEnumerable<TValue> this[TKey key]
        {
            get
            {
                List<TValue> bucket;
                if (this.lookup.TryGetValue(key, out bucket))
                {
                    return bucket;
                }
                return EMPTY_BUCKET;
            }
        }

        public Multimap<TKey, TValue> Add(TKey key, params TValue[] values)
        {
            List<TValue> bucket;
            if (this.lookup.TryGetValue(key, out bucket))
            {
                bucket.AddRange(values);
            }
            else
            {
                this.lookup[key] = new List<TValue>(values);
            }
            return this;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (TKey key in this.lookup.Keys)
            {
                foreach (TValue value in this.lookup[key])
                {
                    yield return new KeyValuePair<TKey, TValue>(key, value);
                }
            }
        }

        public IEnumerable<TValue> GetValueEnumerator()
        {
            foreach (TKey key in this.lookup.Keys)
            {
                foreach (TValue value in this.lookup[key])
                {
                    yield return value;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IList<TKey> Keys
        {
            get { return new List<TKey>(this.lookup.Keys); }
        }
    }
}
