using System;
using System.Configuration;

namespace Utilities
{
    public class MockedConfigRepository : IConfigRepository
    {
        public string Get(string key)
        {
            return Get(key, true);
        }

        public String Get(String key, bool required)
        {
            return Get<String>(key, required);
        }

        public T Get<T>(string key)
        {
            return Get<T>(key, true);
        }

        public T Get<T>(String key, bool required)
        {
            var stringValue = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(stringValue) && required)
                throw new ConfigurationException("Key '" + key + "' not found.");

            var returnType = typeof(T);

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
}
