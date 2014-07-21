using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public interface IHttpChannel
    {
        Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactoryMethod, string userId);
    }
}