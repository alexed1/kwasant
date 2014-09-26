using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Newtonsoft.Json;
using Utilities;

namespace KwasantCore.Services
{
    public class AuthorizationToken
    {
        public String GetAuthorizationTokenURL(IUnitOfWork uow, String url, UserDO userDO, String segmentEventName = null, Dictionary<String, Object> segmentTrackingProperties = null)
        {
            return GetAuthorizationTokenURL(uow, url, userDO.Id, segmentEventName, segmentTrackingProperties);
        }

        public String GetAuthorizationTokenURL(IUnitOfWork uow, String url, String userID, String segmentEventName = null, Dictionary<String, Object> segmentTrackingProperties = null)
        {
            var token = GetAuthorizationToken(uow, url, userID, segmentEventName, segmentTrackingProperties);

            var responseUrl = String.Format("{0}tokenAuth?token={1}",
                    Server.ServerUrl,
                    token);

            return responseUrl;
        }

        private String GetAuthorizationToken(IUnitOfWork uow, String url, String userID, String segmentEventName = null, Dictionary<String, Object> segmentTrackingProperties = null)
        {
            var newTokenLink = new AuthorizationTokenDO
            {
                RedirectURL = url,
                UserID = userID,
                ExpiresAt = DateTime.Now.AddDays(10),
                SegmentTrackingEventName = segmentEventName
            };

            if (segmentTrackingProperties != null)
                newTokenLink.SegmentTrackingProperties = JsonConvert.SerializeObject(segmentTrackingProperties);
            
            uow.AuthorizationTokenRepository.Add(newTokenLink);
            return newTokenLink.Id.ToString();
        }
    }
}
