using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Mwh.Sample.Common.Extension
{
    /// <summary>
    /// LogExtensions Static Class
    /// </summary>
    public static class LogExtensions
    {
        /// <summary>
        /// GetSerializeObjectString
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialize"></param>
        /// <returns></returns>
        public static string GetSerializeObjectString<T>(this T objectToSerialize)
        {
            try
            {
                using (StringWriter writer = new StringWriter())
                {
                    XmlSerializer oXS = new XmlSerializer(typeof(T));
                    var myXML = new XmlDocument();
                    oXS.Serialize(writer, objectToSerialize);
                    myXML.LoadXml(writer.ToString());
                    return myXML.OuterXml.ToString();
                }
            }
            catch
            {
                return objectToSerialize.GetTextObjectString();
            }
        }

        /// <summary>
        /// GetSerializeObjectString
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="lstObjectToSerialize"></param>
        /// <returns></returns>
        public static string GetSerializeObjectString<T>(this List<T> lstObjectToSerialize)
        {
            try
            {
                using (StringWriter writer = new StringWriter())
                {
                    XmlSerializer oXS = new XmlSerializer(typeof(List<T>));
                    var myXML = new XmlDocument();
                    oXS.Serialize(writer, lstObjectToSerialize);
                    myXML.LoadXml(writer.ToString());
                    return myXML.OuterXml.ToString();
                }
            }
            catch
            {
                return lstObjectToSerialize.GetTextObjectString();
            }
        }

        /// <summary>
        /// IsSimpleType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsValueType ||
                type.IsPrimitive ||
                new Type[]
                {
                    typeof(String),
                    typeof(Decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        /// <summary>
        /// To get Dictionary of Single record with property.
        /// </summary>
        /// <param name="record">record as object</param>
        /// <returns>Dictionary object</returns>
        private static Dictionary<string, object> GetDictionaryWithPropertiesForOneRecord(object record)
        {
            if (record == null)
                return new Dictionary<string, object>();

            Type type = record.GetType();
            PropertyInfo[] properties = type.GetProperties();
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            foreach (PropertyInfo propertyInfo in properties)
            {
                if (propertyInfo != null)
                {
                    if (IsSimpleType(propertyInfo.PropertyType))
                    {
                        object value = propertyInfo.GetValue(record, new object[] { });
                        dictionary.Add(propertyInfo.Name, value);
                    }
                }
            }
            return dictionary;
        }

        /// <summary>
        /// To get record with their properties
        /// </summary>
        /// <param name="record">record as object</param>
        /// <returns>String</returns>
        private static string GetTextObjectString(this object record)
        {
            StringBuilder recordLog = new StringBuilder();
            Dictionary<string, object> recordDictionary = GetDictionaryWithPropertiesForOneRecord(record);
            int propertyCounter = 0;
            try
            {
                foreach (var keyValuePair in recordDictionary)
                {
                    propertyCounter += 1;
                    object thePropertyValue = recordDictionary[keyValuePair.Key];
                    if (thePropertyValue != null)
                    {
                        recordLog.AppendFormat("{0}:{1}|", keyValuePair.Key, keyValuePair.Value);
                    }
                    else
                    {
                        recordLog.AppendFormat("{0}:{1}| ", keyValuePair.Key, "[NULL]");
                    }
                }
            }
            catch (Exception ex)
            {
                recordLog.AppendLine(ex.Message);
            }
            finally
            {
            }
            return recordLog.ToString();
        }
    }
}
