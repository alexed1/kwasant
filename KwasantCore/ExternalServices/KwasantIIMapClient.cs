using System;
using System.Collections.Generic;
using System.Net.Mail;
using S22.Imap;

namespace KwasantCore.ExternalServices
{
    public class KwasantImapClient : IKwasantIMapClient
    {
        private ImapClient _internalClient;
        
        public void Initialize(String serverURL, int port, bool useSSL)
        {
            _internalClient = new ImapClient(serverURL, port, useSSL);
            _internalClient.NewMessage += (sender, args) => { if (NewMessage != null) NewMessage(sender, args); };
            _internalClient.MessageDeleted += (sender, args) => { if (MessageDeleted != null) MessageDeleted(sender, args); };
        }

        public void Dispose()
        {
            _internalClient.Dispose();
        }

        public void Login(string username, string password, AuthMethod method)
        {
            _internalClient.Login(username, password, method);
        }

        public void Logout()
        {
            _internalClient.Logout();
        }

        public IEnumerable<string> Capabilities()
        {
            return _internalClient.Capabilities();
        }

        public bool Supports(string capability)
        {
            return _internalClient.Supports(capability);
        }

        public void RenameMailbox(string mailbox, string newName)
        {
            _internalClient.RenameMailbox(mailbox, newName);
        }

        public void DeleteMailbox(string mailbox)
        {
            _internalClient.DeleteMailbox(mailbox);
        }

        public void CreateMailbox(string mailbox)
        {
            _internalClient.CreateMailbox(mailbox);
        }

        public IEnumerable<string> ListMailboxes()
        {
            return _internalClient.ListMailboxes();
        }

        public void Expunge(string mailbox = null)
        {
            _internalClient.Expunge(mailbox);
        }

        public MailboxInfo GetMailboxInfo(string mailbox = null)
        {
            return _internalClient.GetMailboxInfo(mailbox);
        }

        public IEnumerable<uint> Search(SearchCondition criteria, string mailbox = null)
        {
            return _internalClient.Search(criteria, mailbox);
        }

        public MailMessage GetMessage(uint uid, bool seen = true, string mailbox = null)
        {
            return _internalClient.GetMessage(uid, seen, mailbox);
        }

        public MailMessage GetMessage(uint uid, FetchOptions options, bool seen = true, string mailbox = null)
        {
            return _internalClient.GetMessage(uid, options, seen, mailbox);
        }

        public MailMessage GetMessage(uint uid, ExaminePartDelegate callback, bool seen = true, string mailbox = null)
        {
            return _internalClient.GetMessage(uid, callback, seen, mailbox);
        }

        public IEnumerable<MailMessage> GetMessages(IEnumerable<uint> uids, bool seen = true, string mailbox = null)
        {
            return _internalClient.GetMessages(uids, seen, mailbox);
        }

        public IEnumerable<MailMessage> GetMessages(IEnumerable<uint> uids, ExaminePartDelegate callback, bool seen = true, string mailbox = null)
        {
            return _internalClient.GetMessages(uids, callback, seen, mailbox);
        }

        public IEnumerable<MailMessage> GetMessages(IEnumerable<uint> uids, FetchOptions options, bool seen = true, string mailbox = null)
        {
            return _internalClient.GetMessages(uids, options, seen, mailbox);
        }

        public uint StoreMessage(MailMessage message, bool seen = false, string mailbox = null)
        {
            return _internalClient.StoreMessage(message, seen, mailbox);
        }

        public IEnumerable<uint> StoreMessages(IEnumerable<MailMessage> messages, bool seen = false, string mailbox = null)
        {
            return _internalClient.StoreMessages(messages, seen, mailbox);
        }

        public void CopyMessage(uint uid, string destination, string mailbox = null)
        {
            _internalClient.CopyMessage(uid, destination, mailbox);
        }

        public void CopyMessages(IEnumerable<uint> uids, string destination, string mailbox = null)
        {
            _internalClient.CopyMessages(uids, destination, mailbox);
        }

        public void MoveMessage(uint uid, string destination, string mailbox = null)
        {
            _internalClient.MoveMessage(uid, destination, mailbox);
        }

        public void MoveMessages(IEnumerable<uint> uids, string destination, string mailbox = null)
        {
            _internalClient.MoveMessages(uids, destination, mailbox);
        }

        public void DeleteMessage(uint uid, string mailbox = null)
        {
            _internalClient.DeleteMessage(uid, mailbox);
        }

        public void DeleteMessages(IEnumerable<uint> uids, string mailbox = null)
        {
            _internalClient.DeleteMessages(uids, mailbox);
        }

        public IEnumerable<MessageFlag> GetMessageFlags(uint uid, string mailbox = null)
        {
            return _internalClient.GetMessageFlags(uid, mailbox);
        }

        public void SetMessageFlags(uint uid, string mailbox, params MessageFlag[] flags)
        {
            _internalClient.SetMessageFlags(uid, mailbox, flags);
        }

        public void AddMessageFlags(uint uid, string mailbox, params MessageFlag[] flags)
        {
            _internalClient.AddMessageFlags(uid, mailbox, flags);
        }

        public void RemoveMessageFlags(uint uid, string mailbox, params MessageFlag[] flags)
        {
            _internalClient.RemoveMessageFlags(uid, mailbox, flags);
        }

        public string DefaultMailbox
        {
            get
            {
                return _internalClient.DefaultMailbox;
            }
            set
            {
                _internalClient.DefaultMailbox = value;
            }
        }

        public bool Authed
        {
            get
            {
                return _internalClient.Authed;
            }
        }
        
        public event EventHandler<IdleMessageEventArgs> NewMessage;
        public event EventHandler<IdleMessageEventArgs> MessageDeleted;
    }
}
