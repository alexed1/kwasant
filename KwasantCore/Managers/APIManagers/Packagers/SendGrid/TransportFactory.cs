using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SendGrid;
using Utilities;

namespace KwasantCore.Managers.APIManagers.Packagers.SendGrid
{
    class TransportFactory
    {
        public static Web CreateWeb(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            var credentials = new NetworkCredential
            {
                UserName = configRepository.Get("OutboundUserName"),
                Password = configRepository.Get("OutboundUserPassword")
            };
            return new Web(credentials);
        }
    }
}
