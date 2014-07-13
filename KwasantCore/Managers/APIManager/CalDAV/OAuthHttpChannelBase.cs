using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using KwasantCore.Managers.APIManager.Authorizers;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public abstract class OAuthHttpChannelBase : IHttpChannel
    {
        public async Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactoryMethod)
        {
            HttpResponseMessage response;
            using (HttpClient client = new HttpClient())
            {
                do
                {
                    using (var request = requestFactoryMethod())
                    {
                        var accessToken = await Authorizer.GetAccessTokenAsync(CancellationToken.None);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                await Authorizer.RefreshTokenAsync(CancellationToken.None);
                                response.Dispose();
                            }
                            else
                            {
                                response.EnsureSuccessStatusCode();
                            }
                        }
                    }
                } while (!response.IsSuccessStatusCode);
            }
            return response;
        }

        public abstract IOAuthAuthorizer Authorizer { get; }
    }
}