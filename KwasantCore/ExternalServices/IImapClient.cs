using System;
using System.Collections.Generic;
using System.Net.Mail;
using S22.Imap;

namespace KwasantCore.ExternalServices
{
    public interface IImapClient
    {
        void Initialize(String serverURL, int port, bool useSSL);
        void Login(string username, string password, AuthMethod method);
        IEnumerable<MailMessage> GetMessages(IEnumerable<uint> uids, bool seen = true, string mailbox = null);
        IEnumerable<uint> Search(SearchCondition criteria, string mailbox = null);
        void Dispose();
        event EventHandler<IdleMessageEventArgsWrapper> NewMessage;
    }

    public class IdleMessageEventArgsWrapper
    {
        public IImapClient Client { get; private set; }

        public IdleMessageEventArgsWrapper(IImapClient client)
        {
            Client = client;
        }
    }
}
