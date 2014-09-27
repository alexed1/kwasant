using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mail;
using S22.Imap;

namespace KwasantCore.ExternalServices
{
    public class ImapClientWrapper : IImapClient
    {
        private ImapClient _internalClient;
        private ServiceManager<ImapClientWrapper> _serviceManager;
        
        public void Initialize(String serverURL, int port, bool useSSL)
        {
            _serviceManager = new ServiceManager<ImapClientWrapper>("Imap Service: " + serverURL, "Email Services");

            _internalClient = new ImapClient(serverURL, port, useSSL);
            
            _serviceManager.LogEvent("Installed event listener for new messages.");
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
            _serviceManager.LogEvent("Retrieving messages...");
            try
            {
                var messages = _internalClient.GetMessages(uids, seen, mailbox).ToList();
                _serviceManager.LogSucessful(messages.Count + " messages retrieved.");
                return messages;
            }
            catch (Exception ex)
            {
                _serviceManager.LogFail(ex, "Failed to retrieve messages.");
                throw;
            }
        }


        private event EventHandler<IdleMessageEventArgsWrapper> NewMessageInternal;
        public event EventHandler<IdleMessageEventArgsWrapper> NewMessage
        {
            add
            {
                _serviceManager.LogEvent("New consumer of 'NewMessage' event added.");
                NewMessageInternal += value;
                _internalClient.NewMessage += InternalClientOnNewMessage;
            }
            remove
            {
                _serviceManager.LogEvent("Removed consumer of 'NewMessage' event.");
                NewMessageInternal -= value;
                _internalClient.NewMessage -= InternalClientOnNewMessage;
            }
        }

        private void InternalClientOnNewMessage(object sender, IdleMessageEventArgs idleMessageEventArgs)
        {
            if (NewMessageInternal != null)
                NewMessageInternal(sender, new IdleMessageEventArgsWrapper(this));
        }


        private event EventHandler<IdleErrorEventArgsWrapper> IdleErrorInternal;
        public event EventHandler<IdleErrorEventArgsWrapper> IdleError
        {
            add
            {
                _serviceManager.LogEvent("New consumer of 'IdleError' event added.");
                IdleErrorInternal += value;
                _internalClient.IdleError += InternalClientOnNewMessage;
            }
            remove
            {
                _serviceManager.LogEvent("Removed consumer of 'IdleError' event.");
                IdleErrorInternal -= value;
                _internalClient.IdleError -= InternalClientOnNewMessage;
            }
        }

        private void InternalClientOnNewMessage(object sender, IdleErrorEventArgs idleMessageEventArgs)
        {
            if (IdleErrorInternal != null)
                IdleErrorInternal(sender, new IdleErrorEventArgsWrapper(this, idleMessageEventArgs.Exception));
        }


        private event EventHandler<IdleEndedEventArgsWrapper> IdleEndedInternal;
        public event EventHandler<IdleEndedEventArgsWrapper> IdleEnded
        {
            add
            {
                _serviceManager.LogEvent("New consumer of 'IdleEnded' event added.");
                IdleEndedInternal += value;
                _internalClient.IdleEnded += InternalClientOnNewMessage;
            }
            remove
            {
                _serviceManager.LogEvent("Removed consumer of 'IdleEnded' event.");
                IdleEndedInternal -= value;
                _internalClient.IdleEnded -= InternalClientOnNewMessage;
            }
        }

        private void InternalClientOnNewMessage(object sender, IdleEndedEventArgs idleMessageEventArgs)
        {
            if (IdleEndedInternal != null)
                IdleEndedInternal(sender, new IdleEndedEventArgsWrapper(this));
        }
    }
}
