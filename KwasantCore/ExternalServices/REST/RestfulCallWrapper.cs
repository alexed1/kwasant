using System;
using KwasantCore.Managers.APIManager.Transmitters.Restful;
using Utilities;

namespace KwasantCore.ExternalServices.REST
{
    public class RestfulCallWrapper : IRestfullCall
    {
        private RestfulCall _client;
        public void Initialize(string baseURL, string resource, Method method)
        {
            _client = new RestfulCall(baseURL, resource, method);
        }

        public void AddBody(string body, string contentType)
        {
            _client.AddBody(body, contentType);
        }

        public IRestfulResponse Execute()
        {
            return new RestfulResponseWrapper(_client.Execute());
        }
    }
}
