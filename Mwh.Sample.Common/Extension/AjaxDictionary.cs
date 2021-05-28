using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Mwh.Sample.Common.Extension
{
    /// <summary>
    /// Special Dictionary for Use with Restful / AJAX Calls
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public sealed class AjaxDictionary<TKey, TValue>
    {
        /// <summary>
        /// The dictionary
        /// </summary>
        private Dictionary<TKey, TValue> _Dictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxDictionary{TKey, TValue}"/> class.
        /// </summary>
        public AjaxDictionary() { _Dictionary = new Dictionary<TKey, TValue>(); }
        /// <summary>
        /// Initializes a new instance of the <see cref="AjaxDictionary{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
        public AjaxDictionary(SerializationInfo info, StreamingContext context)
        { _Dictionary = new Dictionary<TKey, TValue>(); }

        /// <summary>
        /// Gets or sets the <see cref="TValue"/> with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>TValue.</returns>
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

        /// <summary>
        /// Adds the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Add(Dictionary<TKey, TValue> value) { _Dictionary = value; }
        /// <summary>
        /// Adds the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Add(TKey key, TValue value) { _Dictionary.Add(key, value); }

        /// <summary>
        /// Gets the list.
        /// </summary>
        /// <returns>List&lt;System.String&gt;.</returns>
        public List<string> GetList()
        {
            List<string> list = new List<string>();
            foreach (var item in _Dictionary)
            {
                list.Add(string.Format("{0} - {1}", item.Key, item.Value));
            }
            return list;
        }

        /// <summary>
        /// Gets the object data.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="context">The context.</param>
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
