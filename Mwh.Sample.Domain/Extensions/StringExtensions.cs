
namespace Mwh.Sample.Domain.Extensions;
/// <summary>
/// Extensions to <see cref="DateTime" />
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Gets the decimal from string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>System.Decimal.</returns>
    public static decimal GetDecimalFromString(this string? str, decimal defaultValue)
    {
        bool parsed = decimal.TryParse(str, out decimal returnDecimal);
        return parsed ? returnDecimal : defaultValue;
    }

    /// <summary>
    /// Gets the int from string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
    public static int? GetIntFromString(this string? str, int? defaultValue)
    {
        if (str == null) return defaultValue;
        bool parsed = int.TryParse(str, out int returnInt);
        return parsed ? returnInt : defaultValue;
    }

    /// <summary>
    /// Gets the int from string.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>System.Int32.</returns>
    public static int GetIntFromString(this string? str, int defaultValue)
    {
        bool parsed = int.TryParse(str, out int returnInt);
        if (parsed) return returnInt;
        return parsed ? returnInt : defaultValue;
    }

    /// <summary>
    /// Indexes the of NTH.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="value">The value.</param>
    /// <param name="nth">The NTH.</param>
    /// <returns>System.Int32.</returns>
    /// <exception cref="ArgumentException">Can not find the zeroth index of substring in string. Must start with 1</exception>
    public static int IndexOfNth(this string? str, string? value, int nth = 1)
    {
        if (str == null) return 0;

        if (value == null) return 0;

        if (nth <= 0) throw new ArgumentException("Can not find the zeroth index of substring in string. Must start with 1");

        int offset = str.IndexOf(value);

        for (int i = 1; i < nth; i++)
        {
            if (offset == -1)
                return -1;
            offset = str.IndexOf(value, offset + 1);
        }

        return offset;
    }

    /// <summary>
    /// Lefts the specified length.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="length">The length.</param>
    /// <returns>System.String.</returns>
    public static string Left(this string? str, int length)
    {
        if (str == null)
            return string.Empty;
        if (str.Length <= length)
            return str;
        return str.Substring(0, Math.Min(length, str.Length));
    }

    /// <summary>
    /// Rights the specified length.
    /// </summary>
    /// <param name="str">The string.</param>
    /// <param name="length">The length.</param>
    /// <returns>System.String.</returns>
    public static string Right(this string? sValue, int iMaxLength)
    {
        //Check if the value is valid
        if (string.IsNullOrEmpty(sValue))
        {
            //Set valid empty string as string could be null
            sValue = string.Empty;
        }
        else if (sValue.Length > iMaxLength)
        {
            //Make the string no longer than the max length
            sValue = sValue.Substring(sValue.Length - iMaxLength, iMaxLength);
        }

        //Return the string
        return sValue;
    }


    /// <summary>
    /// Extension Method for String to Trim and Check for null in a single function
    /// </summary>
    /// <param name="value">The String to be trimmed</param>
    /// <returns>Trimmed string or string.empty is the value was null</returns>
    public static string TrimIfNotNull(this string? value)
    {
        return value == null ? string.Empty : value.Trim();
    }
}
