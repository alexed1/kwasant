using System;

namespace KwasantCore.ExternalServices
{
    public interface IImapClient : S22.Imap.IImapClient
    {
        void Initialize(String serverURL, int port, bool useSSL);
    }
}
