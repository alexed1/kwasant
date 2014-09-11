﻿using System;
using System.Net.Mail;
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
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
          
            _handlers = new IInboundEmailHandler[]
                            {
                                new InvitationResponseHandler(),
                                new BookingRequestHandler(),
                            };
        }

        //be careful about using this form. can get into problems involving disposal.
        public InboundEmail(IImapClient client, IConfigRepository configRepository)
        {
            _client = client;
            _configRepository = configRepository;

            _handlers = new IInboundEmailHandler[]
                            {
                                new InvitationResponseHandler(),
                                new BookingRequestHandler(),
                            };
        }

        private string GetIMAPServer()
        {
            return _configRepository.Get("InboundEmailHost");
        }

        private int GetIMAPPort()
        {
            return _configRepository.Get<int>("InboundEmailPort");
        }

        public String UserName;
        private string GetUserName()
        {
            return UserName ?? _configRepository.Get("INBOUND_EMAIL_USERNAME");
        }

        public String Password;
        private string GetPassword()
            {
            return Password ??_configRepository.Get("INBOUND_EMAIL_PASSWORD");
        }

        private bool UseSSL()
                {
            return _configRepository.Get<bool>("InboundEmailUseSSL");
                }

        public override int WaitTimeBetweenExecution
        {
            get
            {
                return -1;
            }
        }

        private IImapClient Client
            {
            get
            {
                if (_client != null)
                    return _client;

                try
                {
                    _client = new ImapClient(GetIMAPServer(), GetIMAPPort(), UseSSL());
                    string curUser = GetUserName();
                    string curPwd = GetPassword();
                    _client.Login(curUser, curPwd, AuthMethod.Login);
            }
            catch (Exception ex)
            {
                    Logger.GetLogger().Error("Error occured on startup... shutting down", ex);
                }

                return _client;
            }
            }
            
        protected override void Run()
                                {
            GetUnreadMessages(Client);
            Client.NewMessage += (sender, args) => GetUnreadMessages(args.Client);
        }

        private void GetUnreadMessages(IImapClient client)
        {
            var messages = client.GetMessages(client.Search(SearchCondition.Unseen()));

            foreach (var message in messages)
                ProcessMessageInfo(message);
        }

        private void ProcessMessageInfo(MailMessage messageInfo)
        {
            var logString = "Processing message with subject '" + messageInfo.Subject + "'";
                Logger.GetLogger().Info(logString);

                    try
                    {
                        var handlerIndex = 0;
                        while (handlerIndex < _handlers.Length
                    && !_handlers[handlerIndex].Process(messageInfo))
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
            if(_client != null)
                _client.Dispose();
        }
    }
}
