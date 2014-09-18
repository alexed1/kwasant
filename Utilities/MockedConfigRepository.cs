using System;
using System.Configuration;

namespace Utilities
{
    public class MockedConfigRepository : IConfigRepository
    {
        public string Get(string key)
        {
            return Get<string>(key);
        }

        public string Get(string key, string defaultValue)
        {
            return Get<string>(key, defaultValue);
        }

        public T Get<T>(String key)
        {
            return InternalGet<T>(key);
        }

        public T Get<T>(string key, T defaultValue)
        {
            return InternalGet(key, defaultValue, true);
        }

        private T InternalGet<T>(String key, T defaultValue = default(T), bool defaultProvided = false)
        {
            var stringValue = ConfigurationManager.AppSettings[key];

            if (String.IsNullOrEmpty(stringValue))
            {
                if (!defaultProvided)
                    throw new ConfigurationException("Key '" + key + "' not found.");

                return defaultValue;
            }


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
