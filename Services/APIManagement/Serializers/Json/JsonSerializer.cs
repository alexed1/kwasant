using System.Collections.Generic;
using Newtonsoft.Json;

namespace Shnexy.Services.APIManagement.Serializers.Json
{
    /// <summary>
    /// Default JSON serializer
    /// Doesn't currently use the SerializeAs attribute, defers to Newtonsoft's attributes
    /// </summary>
    public class JsonSerializer
    {        
        private JsonSerializerSettings _settings;

        /// <summary>
        /// Default serializer
        /// </summary>
        public JsonSerializer()
        {
            _settings = new JsonSerializerSettings
                            {
                                MissingMemberHandling = MissingMemberHandling.Ignore,
                                NullValueHandling = NullValueHandling.Include,
                                DefaultValueHandling = DefaultValueHandling.Include,
                                PreserveReferencesHandling=PreserveReferencesHandling.Objects                               
                            };
        }

        /// <summary>
        /// Serialize the object as JSON
        /// </summary>
        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj, _settings);
        }

        /// <summary>
        /// Deserializes JSON response to object
        /// </summary>
        public T Deserialize<T>(string responseJson)
        {
            if (responseJson == null) return default(T);
            if (responseJson.StartsWith("{\"messages\":{")) return default(T);


            return responseJson == "[]"
                ? default(T)
                : JsonConvert.DeserializeObject<T>(responseJson, _settings);
        }

        /// <summary>
        /// Deserializes JSON response to List of object
        /// </summary>
        public IList<T> DeserializeList<T>(string responseJson)
        {
            if (responseJson == null) return default(IList<T>);
            if (responseJson.StartsWith("{\"messages\":{")) return default(IList<T>);
            if (responseJson == "[]") return default(IList<T>);

            var result = JsonConvert.DeserializeObject<object>(responseJson, _settings);
            IList<T> listT = new List<T>();
            foreach (var item in ((Newtonsoft.Json.Linq.JObject)(result)))
            {
                listT.Add(Deserialize<T>(item.Value.ToString()));
            }
            return listT;
        }
    }
}
