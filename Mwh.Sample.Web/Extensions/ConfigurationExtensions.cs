namespace Mwh.Sample.Web.Extensions;

/// <summary>
/// ConfigurationExtensions
/// </summary>
public static class ConfigurationExtensions
{
    private static int GetInt(string Value, string? defaultValue = null)
    {
        if (!string.IsNullOrEmpty(Value))
        {

            return Value.Split<int>(',', out _).FirstOrDefault();
        }
        if (string.IsNullOrEmpty(defaultValue))
        {
            return default;
        }
        return defaultValue.Split<int>(',', out _).FirstOrDefault();
    }
    private static int[] GetIntList(string Value, string? defaultValue = null)
    {

        if (!string.IsNullOrEmpty(Value))
        {
            return Value.Split<int>(',', out _).ToArray() ?? Array.Empty<int>();
        }

        if (string.IsNullOrEmpty(defaultValue))
        {
            return Array.Empty<int>();
        }
        return defaultValue.Split<int>(',', out _).ToArray() ?? Array.Empty<int>();
    }
    private static string GetString(string Value, string? defaultValue = null)
    {
        if (!string.IsNullOrEmpty(Value))
        {
            return Value ?? String.Empty;
        }
        if (string.IsNullOrEmpty(defaultValue))
        {
            return String.Empty;
        }
        return defaultValue ?? string.Empty;
    }

    private static string[] GetStringList(string Value, string? defaultValue = null)
    {
        if (!string.IsNullOrEmpty(Value))
        {
            return Value.Split(",") ?? Array.Empty<string>();
        }

        if (string.IsNullOrEmpty(defaultValue))
        {
            return Array.Empty<string>();
        }
        return defaultValue.Split(",") ?? Array.Empty<string>();
    }
    private static List<T> Split<T>(this string @this, char separator, out bool AllConverted)
    {
        List<T> returnVals = new();
        AllConverted = true;
        var itens = @this.Split(separator);
        foreach (var item in itens)
        {
            try
            {
                returnVals.Add((T)Convert.ChangeType(item, typeof(T)));
            }
            catch { AllConverted = false; }
        }
        return returnVals;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="_config"></param>
    /// <param name="configKey"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static int GetInt(this IConfiguration _config, string configKey, string? defaultValue = null)
    {
        return GetInt(_config.GetSection(configKey).Value, defaultValue);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="_config"></param>
    /// <param name="configKey"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static int[] GetIntList(this IConfiguration _config, string configKey, string? defaultValue = null)
    {
        return GetIntList(_config.GetSection(configKey).Value, defaultValue);
    }
    /// <summary>
    /// Get String from Configuration
    /// </summary>
    /// <param name="_config"></param>
    /// <param name="configKey"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string GetString(this IConfiguration _config, string configKey, string? defaultValue = null)
    {
        return GetString(_config.GetSection(configKey).Value, defaultValue);
    }
    /// <summary>
    /// Get a List of string from Configuration
    /// </summary>
    /// <param name="_config"></param>
    /// <param name="configKey"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public static string[] GetStringList(this IConfiguration _config, string configKey, string? defaultValue = null)
    {
        return GetStringList(_config.GetSection(configKey).Value, defaultValue);
    }
}


