using System;
using Microsoft.WindowsAzure;

namespace UtilitiesLib
{
    public static class ConfigurationHelper
    {
        public static String GetConfigurationValue(String key, bool required = true)
        {
            return GetConfigurationValue<String>(key, required);
        }

        public static T GetConfigurationValue<T>(String key, bool required = true)
        {
            var stringValue = CloudConfigurationManager.GetSetting(key);
            if(String.IsNullOrEmpty(stringValue) && required)
                throw new Exception("Key '" + key + "' not found.");

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

            throw new Exception("Invalid value for '" + key + "'");
        }
    }
}
