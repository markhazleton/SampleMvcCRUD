using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Mwh.Sample.Common.Extension
{
    /// <summary>
    /// ENUM Extension Methods
    /// </summary>
    public static class EnumExtension
    {

        /// <summary>
        /// Gets Display Name of enum 
        /// </summary>
        /// <param name="e">enum key name</param>
        /// <returns>enum value</returns>
        public static string GetDisplayName(this Enum e)
        {
            if (e == null)
                return string.Empty;

            var fieldInfo = e.GetType().GetField(e.ToString());

            var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null)
                return string.Empty;
            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : e.ToString();
        }
        /// <summary>
        /// Returns whether the given enum value is a defined value for its type.
        /// </summary>
        public static bool IsDefined<T>(this T enumValue)
            where T : Enum => EnumValueCache<T>.DefinedValues.Contains(enumValue);

        /// <summary>
        /// Generates Dictionary of int,string for an Enum
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static Dictionary<int, string> ToDictionary(this Enum enumValue)
        {
            var enumType = enumValue.GetType();
            return Enum.GetValues(enumType).Cast<Enum>().ToDictionary(t => (int)(object)t, t => t.ToString());
        }

        /// <summary>
        /// Caches the defined values for each enum type for which this class is accessed.
        /// </summary>
        private static class EnumValueCache<T>
            where T : Enum
        {
            public static readonly HashSet<T> DefinedValues = new HashSet<T>((T[])Enum.GetValues(typeof(T)));
        }
    }
}

