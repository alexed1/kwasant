using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using Daemons.InboundEmailHandlers;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using S22.Imap;
using StructureMap;
using Utilities;
using Utilities.Logging;

namespace Daemons
{
    public class InboundEmail : Daemon
    {
        private IImapClient _client;
        private readonly IConfigRepository _configRepository;
        private readonly IInboundEmailHandler[] _handlers;

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
            : this(ObjectFactory.GetInstance<IConfigRepository>(), null)
        {
          
        }

        //be careful about using this form. can get into problems involving disposal.
        public InboundEmail(IConfigRepository configRepository, IImapClient client)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
            _client = client;
            // last handler must be BookingRequestHandler as it processes any message that has not been processed by others.
            _handlers = new IInboundEmailHandler[]
                            {
                                new InvitationResponseHandler(),
                                new BookingRequestHandler(),
                            };
        }

        private IImapClient CreateIntakeClient()
        {
            return new ImapClient(
                _configRepository.Get("InboundEmailHost"),
                _configRepository.Get<int>("InboundEmailPort"),
                _configRepository.Get("INBOUND_EMAIL_USERNAME"),
                _configRepository.Get("INBOUND_EMAIL_PASSWORD"),
                AuthMethod.Login,
                _configRepository.Get<bool>("InboundEmailUseSSL"));
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            try
            {
                if (_client == null)
                {
                    _client = CreateIntakeClient();
                }
            }
            catch (ConfigurationException ex)
            {
                Logger.GetLogger().Error("Error occured on startup... shutting down", ex);
                Stop();
                return;
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error("Error occured on startup... restarting.", ex);
                return;
            }
            
            Logger.GetLogger().Info(GetType().Name + " - Querying inbound accounts...");
            try
            {
                var allMessageInfos = _client.ListMailboxes()
                    .SelectMany(mailbox => _client
                        .Search(SearchCondition.Unseen(), mailbox)
                        .Select(
                            uid =>
                                new
                                {
                                    Client = _client,
                                    Mailbox = mailbox,
                                    Uid = uid,
                                    Message = _client.GetMessage(uid, mailbox: mailbox)
                                }))
                    .Where(messageInfo => messageInfo.Message.From != null)
                    .ToList();
                var messageInfos = allMessageInfos
                    .Select(messageInfo => messageInfo.Message.Headers["Message-ID"])
                    .Distinct(StringComparer.Ordinal)
                    .Select(
                        id =>
                            allMessageInfos.First(
                                messageInfo =>
                                    string.Equals(messageInfo.Message.Headers["Message-ID"], id,
                                        StringComparison.Ordinal)))
                    .ToList();


                string logString;

                //the difference in syntax makes it easy to have nonzero hits stand out visually in the log dashboard
                if (messageInfos.Any())
                    logString = GetType().Name + " - " + messageInfos.Count() + " emails found!";
                else
                    logString = GetType().Name + " - 0 emails found...";
                Logger.GetLogger().Info(logString);

                foreach (var messageInfo in messageInfos)
                {
                    try
                    {
                        var handlerIndex = 0;
                        while (handlerIndex < _handlers.Length
                               && !_handlers[handlerIndex].Process(messageInfo.Message))
                        {
                            handlerIndex++;
                        }
                        if (handlerIndex >= _handlers.Length)
                            throw new ApplicationException("Message hasn't been processed by any handler.");
                    }
                    catch (Exception e)
                    {
                        AlertManager.EmailProcessingFailure(messageInfo.Message.Headers["Date"], e.Message);
                        Logger.GetLogger().Error(string.Format("EmailProcessingFailure Reported. ObjectID = {0}",
                            messageInfo.Message.Headers["Message-ID"]));
                        messageInfo.Client.AddMessageFlags(messageInfo.Uid, messageInfo.Mailbox, MessageFlag.Seen);
                    }
                }
            }
            catch (SocketException ex)  //we were getting strange socket errors after time, and it looks like a reset solves things
            {
                CleanUp();
                _client = null; //this will get recreated the next time this daemon runs
                AlertManager.EmailProcessingFailure(DateTime.Now.to_S(), "Got that SocketException");
                Logger.GetLogger().Error("Hit SocketException. Trying to reset the IMAP Client.", ex);

            }

            catch (Exception ex)
            {
                Logger.GetLogger().Error("Error occured on querying... restarting.", ex);
            }
        }

        protected override void CleanUp()
        {
            if (_client != null)
                _client.Dispose();
        }
    }
}
