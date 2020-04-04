using System;

namespace Mwh.Sample.Common.Extension
{
    /// <summary>
    /// Extensions to <see cref="DateTime"/>
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Extension Method for String to Trim and Check for null in a single function
        /// </summary>
        /// <param name="value">The String to be trimmed</param>
        /// <returns>Trimmed string or string.empty is the value was null</returns>
        public static string TrimIfNotNull(this string value)
        {
            if (value != null)
            {
                return value.Trim();
            }
            return string.Empty;
        }

        public static string Left(this string str, int length)
        {
            if (str == null)
                return string.Empty;
            if (str.Length <= length)
                return str;
            return str.Substring(0, Math.Min(length, str.Length));
        }

        public static string Right(this string str, int length)
        {
            if (str == null)
                return string.Empty;
            if (str.Length <= length)
                return str;
            return str.Substring(str.Length - Math.Min(length, str.Length));
        }

        public static int IndexOfNth(this string str, string value, int nth = 1)
        {
            if (str == null)
                return 0;

            if (value == null)
                return 0;

            if (nth <= 0)
                throw new ArgumentException("Can not find the zeroth index of substring in string. Must start with 1");

            int offset = str.IndexOf(value);

            for (int i = 1; i < nth; i++)
            {
                if (offset == -1)
                    return -1;
                offset = str.IndexOf(value, offset + 1);
            }

            return offset;
        }

        public static int? GetIntFromString(this string str, int? defaultValue)
        {
            int returnInt = defaultValue ?? 0;
            Boolean parsed = Int32.TryParse(str, out returnInt);
            if (parsed)
            {
                return returnInt;
            }
            else
            {
                return defaultValue;
            }
        }

        public static int GetIntFromString(this string str, int defaultValue)
        {
            int returnInt = defaultValue;
            Boolean parsed = Int32.TryParse(str, out returnInt);
            if (!parsed)
                returnInt = defaultValue;
            return returnInt;
        }

        public static decimal GetDecimalFromString(this string str, decimal defaultValue)
        {
            decimal returnDecimal = defaultValue;
            Boolean parsed = Decimal.TryParse(str, out returnDecimal);
            if (!parsed)
                returnDecimal = defaultValue;
            return returnDecimal;
        }
    }
}
