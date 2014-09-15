using System;
using Data.Entities;
using Data.Interfaces;
using StructureMap;
using Utilities;

namespace KwasantCore.Services
{
    public class AuthorizationToken
    {
        public String GetAuthorizationTokenURL(IUnitOfWork uow, String url, UserDO userDO)
        {
            return GetAuthorizationTokenURL(uow, url, userDO.Id);
        }

        public String GetAuthorizationTokenURL(IUnitOfWork uow, String url, String userID)
        {
            var token = GetAuthorizationToken(uow, url, userID);

            var responseUrl = String.Format("{0}tokenAuth?token={1}",
                    Server.ServerUrl,
                    token);

            return responseUrl;
        }

        private String GetAuthorizationToken(IUnitOfWork uow, String url, String userID)
        {
            var newTokenLink = new AuthorizationTokenDO
            {
                RedirectURL = url,
                UserID = userID,
                ExpiresAt = DateTime.Now.AddDays(10)
            };
            uow.AuthorizationTokenRepository.Add(newTokenLink);
 

            return newTokenLink.Id.ToString();
        }
    }
}
