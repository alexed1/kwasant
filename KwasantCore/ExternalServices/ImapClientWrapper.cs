using System;
using System.Collections.Generic;
using System.Net.Mail;
using S22.Imap;

namespace KwasantCore.ExternalServices
{
    public class ImapClientWrapper : IImapClient
    {
        private ImapClient _internalClient;
        
        public void Initialize(String serverURL, int port, bool useSSL)
        {
            _internalClient = new ImapClient(serverURL, port, useSSL);
            _internalClient.NewMessage += (sender, args) => { if (NewMessage != null) NewMessage(sender, new IdleMessageEventArgsWrapper(this)); };
        }

        public IEnumerable<uint> Search(SearchCondition criteria, string mailbox = null)
        {
            return _internalClient.Search(criteria, mailbox);
        }

        public void Dispose()
        {
            _internalClient.Dispose();
        }

        public void Login(string username, string password, AuthMethod method)
        {
            _internalClient.Login(username, password, method);
        }

        public IEnumerable<MailMessage> GetMessages(IEnumerable<uint> uids, bool seen = true, string mailbox = null)
        {
            return _internalClient.GetMessages(uids, seen, mailbox);
        }

        public event EventHandler<IdleMessageEventArgsWrapper> NewMessage;
    }
}
