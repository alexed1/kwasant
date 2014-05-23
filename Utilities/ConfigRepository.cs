using System;
using Microsoft.WindowsAzure;

namespace UtilitiesLib
{
    public static class ConfigRepository
    {
        public static String Get(String key, bool required = true)
        {
            return Get<String>(key, required);
        }

        public static T Get<T>(String key, bool required = true)
        {
            var stringValue = CloudConfigurationManager.GetSetting(key);
            if(String.IsNullOrEmpty(stringValue) && required)
                throw new ConfigurationException("Key '" + key + "' not found.");

            var returnType = typeof (T);

            if (returnType == typeof(String))
            {
                return (T)(object)stringValue;
            }
            if (returnType == typeof(bool))
            {
                bool value;
                if (bool.TryParse(stringValue, out value))
                    return (T)(object)value;
            }
            else if (returnType == typeof(int))
            {
                int value;
                if (int.TryParse(stringValue, out value))
                    return (T)(object)value;
            }

            throw new ConfigurationException("Invalid value for '" + key + "'");
        }
    }

    public class ConfigurationException : Exception { public ConfigurationException(String message) : base(message) { } }
}
