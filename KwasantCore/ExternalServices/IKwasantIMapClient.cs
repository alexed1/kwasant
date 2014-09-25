using System;
using S22.Imap;

namespace KwasantCore.ExternalServices
{
    public interface IKwasantIMapClient : IImapClient
    {
        void Initialize(String serverURL, int port, bool useSSL);
    }
}
