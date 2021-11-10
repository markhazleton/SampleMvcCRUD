namespace Mwh.Sample.Common.Extension;
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
    private readonly Dictionary<TKey, TValue> _Dictionary;

    /// <summary>
    /// Initializes a new instance of the <see cref="AjaxDictionary{TKey, TValue}"/> class.
    /// </summary>
    public AjaxDictionary() { _Dictionary = new Dictionary<TKey, TValue>(); }

    /// <summary>
    /// Initializes a new instance of the <see cref="AjaxDictionary{TKey, TValue}"/> class.
    /// </summary>

    /// <summary>
    /// Gets or sets the with the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>TValue.</returns>
    public TValue this[TKey key]
    {
        get
        {
            _Dictionary.TryGetValue(key, out TValue vOut);
            return vOut;
        }
        set
        {
            _Dictionary.TryGetValue(key, out TValue vOut);
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
    /// Adds the specified dictionary into the current dictionary
    /// </summary>
    /// <param name="value">The value.</param>
    public void Add(Dictionary<TKey, TValue> value)
    {
        foreach (var item in value.Keys)
        {
            Add(item, value[item]);
        }
    }

    /// <summary>
    /// Adds the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    public void Add(TKey key, TValue value)
    {
        if (_Dictionary.ContainsKey(key))
            _Dictionary[key] = value;
        else
            _Dictionary.Add(key, value);
    }

    /// <summary>
    /// Gets the list.
    /// </summary>
    /// <returns>List&lt;System.String&gt;.</returns>
    public List<string> GetList()
    {
        List<string> list = new();
        foreach (var item in _Dictionary)
        {
            list.Add($"{item.Key} - {item.Value}");
        }
        return list;
    }

    /// <summary>
    /// Gets the object data.
    /// </summary>
    /// <param name="info">The information.</param>
    public void GetObjectData(SerializationInfo info)
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
