using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Mwh.Sample.Common.Extension
{
    /// <summary>
    /// ENUM Extension Methods
    /// </summary>
    public static class EnumExtension
    {
        /// <summary>
        /// Converts to Description String.
        /// </summary>
        /// <typeparam name="TEnum">The type of the t enum.</typeparam>
        /// <param name="enum">The enum.</param>
        /// <returns>System.String.</returns>
        public static string ToDescriptionString<TEnum>(this TEnum @enum)
        {
            FieldInfo info = @enum.GetType()?.GetField(@enum.ToString());
            var attributes = (DescriptionAttribute[])info?.GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes?[0].Description ?? @enum.ToString();
        }

        /// <summary>
        /// Gets Display Name of enum
        /// </summary>
        /// <param name="e">enum key name</param>
        /// <returns>enum value</returns>
        public static string GetDisplayName(this Enum e)
        {
            if (e == null)
                return string.Empty;

            var fieldInfo = e.GetType()?.GetField(e.ToString());

            if (!(fieldInfo?.GetCustomAttributes(typeof(DisplayAttribute), false) is DisplayAttribute[] descriptionAttributes))
                return string.Empty;

            return (descriptionAttributes.Length > 0) ? descriptionAttributes[0].Name : e.ToString();
        }
        /// <summary>
        /// Returns whether the given enum value is a defined value for its type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns><c>true</c> if the specified enum value is defined; otherwise, <c>false</c>.</returns>
        public static bool IsDefined<T>(this T enumValue)
            where T : Enum => EnumValueCache<T>.DefinedValues.Contains(enumValue);

        /// <summary>
        /// Generates Dictionary of int,string for an Enum
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>Dictionary&lt;System.Int32, System.String&gt;.</returns>
        public static Dictionary<int, string> ToDictionary(this Enum enumValue)
        {
            var enumType = enumValue.GetType();
            return Enum.GetValues(enumType).Cast<Enum>().ToDictionary(t => (int)(object)t, t => t.ToString());
        }

        /// <summary>
        /// Caches the defined values for each enum type for which this class is accessed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private static class EnumValueCache<T>
            where T : Enum
        {
            /// <summary>
            /// The defined values
            /// </summary>
            public static readonly HashSet<T> DefinedValues = new HashSet<T>((T[])Enum.GetValues(typeof(T)));
        }
    }
}

