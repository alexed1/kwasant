using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using KwasantCore.Managers.APIManager.Authorizers;

namespace KwasantCore.Managers.APIManager.CalDAV
{
    public class OAuthHttpChannel : IHttpChannel
    {
        private readonly IOAuthAuthorizer _authorizer;

        public OAuthHttpChannel(IOAuthAuthorizer authorizer)
        {
            _authorizer = authorizer;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(Func<HttpRequestMessage> requestFactoryMethod, string userId)
        {
            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                do
                {
                    using (var request = requestFactoryMethod())
                    {
                        var accessToken = await _authorizer.GetAccessTokenAsync(userId, CancellationToken.None);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        response = await client.SendAsync(request);
                        if (!response.IsSuccessStatusCode)
                        {
                            if (response.StatusCode == HttpStatusCode.Unauthorized)
                            {
                                await _authorizer.RefreshTokenAsync(userId, CancellationToken.None);
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
    }
}