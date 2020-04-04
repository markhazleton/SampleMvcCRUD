using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mwh.Sample.Common.Extension
{
    /// <summary>
    /// Special Dictionary for Use with Restful / AJAX Calls
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public sealed class AjaxDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _Dictionary;

        public AjaxDictionary() { _Dictionary = new Dictionary<TKey, TValue>(); }
        public AjaxDictionary(SerializationInfo info, StreamingContext context)
        { _Dictionary = new Dictionary<TKey, TValue>(); }

        public TValue this[TKey key]
        {
            get
            {
                TValue vOut;
                _Dictionary.TryGetValue(key, out vOut);
                return vOut;
            }
            set
            {
                TValue vOut;
                _Dictionary.TryGetValue(key, out vOut);
                if (vOut == null)
                {
                    _Dictionary.Add(key, value);
                }
                else
                {
                    _Dictionary[key] = value;
                }
            }
        }

        public void Add(Dictionary<TKey, TValue> value) { _Dictionary = value; }
        public void Add(TKey key, TValue value) { _Dictionary.Add(key, value); }

        public List<string> GetList()
        {
            List<string> list = new List<string>();
            foreach (var item in _Dictionary)
            {
                list.Add(string.Format("{0} - {1}", item.Key, item.Value));
            }
            return list;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            foreach (TKey key in _Dictionary.Keys)
            {
                if (key != null)
                {
                    info.AddValue(key.ToString(), _Dictionary[key]);
                }
            }
        }
    }
}
