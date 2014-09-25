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
            _serviceManager = new ServiceManager<ImapClientWrapper>("Imap Service");

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
            _serviceManager.LogAttempt("Retrieving messages...");
            var messages = _internalClient.GetMessages(uids, seen, mailbox).ToList();
            _serviceManager.LogSucessful(messages.Count + " messages retrieved.");
            return messages;
        }

        //The below is a manual implementation of .NET's event system
        //We need to do this so we can have the add {} and remove {} methods
        //We can't use the automatic event generator, because we need to wrap the event args
        //The below code is essentially the same as a usual 'event EventHandler NewMessage' declaration
        protected EventHandlerList EventDelegateCollection = new EventHandlerList();
        static readonly object NewMessageEventKey = new object();
        private readonly Dictionary<EventHandler<IdleMessageEventArgsWrapper>, EventHandler<IdleMessageEventArgs>> _eventMapping = new Dictionary<EventHandler<IdleMessageEventArgsWrapper>, EventHandler<IdleMessageEventArgs>>();
        public event EventHandler<IdleMessageEventArgsWrapper> NewMessage
        {
            add
            {
                EventDelegateCollection.AddHandler(NewMessageEventKey, value);
                EventHandler<IdleMessageEventArgs> internalClientOnNewMessage = (sender, args) => ((EventHandler<IdleMessageEventArgsWrapper>)EventDelegateCollection[NewMessageEventKey])(sender, new IdleMessageEventArgsWrapper(this));
                _internalClient.NewMessage += internalClientOnNewMessage;
                _eventMapping[value] = internalClientOnNewMessage;
            }
            remove
            {
                EventDelegateCollection.RemoveHandler(NewMessageEventKey, value);
                if (!_eventMapping.ContainsKey(value)) return;
                var oldEvent = _eventMapping[value];
                _internalClient.NewMessage -= oldEvent;
            }
        }
    }
}
