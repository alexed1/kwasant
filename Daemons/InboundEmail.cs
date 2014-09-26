using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Sockets;
using Daemons.InboundEmailHandlers;
using Data.Infrastructure;
using KwasantCore.ExternalServices;
using S22.Imap;
using StructureMap;
using Utilities;
using Utilities.Logging;
using IImapClient = KwasantCore.ExternalServices.IImapClient;

namespace Daemons
{
    public class InboundEmail : Daemon<InboundEmail>
    {
        private IImapClient _client;
        private readonly IConfigRepository _configRepository;
        private readonly IInboundEmailHandler[] _handlers;

        private readonly HashSet<String> _testSubjects = new HashSet<string>(); 
        public void RegisterTestEmailSubject(String subject)
        {
            lock (_testSubjects)
                _testSubjects.Add(subject);
        }

        public delegate void ExplicitCustomerCreatedHandler(string subject);
        public static event ExplicitCustomerCreatedHandler TestMessageRecieved;

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();
          
            _handlers = new IInboundEmailHandler[]
                            {
                                new InvitationResponseHandler(),
                                new BookingRequestHandler()
                            };

            AddTest("OutboundEmailDaemon_TestGmail", "Test Gmail");
            AddTest("OutboundEmailDaemon_TestMandrill", "Test Mandrill");
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
        public string GetUserName()
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
                    _client = ObjectFactory.GetInstance<IImapClient>();
                    _client.Initialize(GetIMAPServer(), GetIMAPPort(), UseSSL());

                    string curUser = GetUserName();
                    string curPwd = GetPassword();
                    _client.Login(curUser, curPwd, AuthMethod.Login);
                }
                catch (Exception ex)
                {
                    Logger.GetLogger().Error("Error occured on startup... shutting down", ex);
                    throw;
                }

                return _client;
            }
        }

        protected override void Run()
        {
            LogEvent("Waiting for messages at " + GetUserName() + "...");
            GetUnreadMessages(Client);
            Client.NewMessage += OnNewMessage;
        }

        protected override void StopInternal()
        {
            LogEvent("Removed email listener");
            Client.NewMessage -= OnNewMessage;
            IsRunning = false;
            LogEvent("Stopped.");
            SetFlag("State", "Stopped");
            base.StopInternal();
        }

        public void OnNewMessage(object sender, IdleMessageEventArgsWrapper args)
        {
            Logger.GetLogger().Info("New email notification recieved.");
            LogEvent("New email notification recieved.");
            GetUnreadMessages(args.Client);
        }
        
        private void GetUnreadMessages(IImapClient client)
        {
            try
            {
                LogEvent("Querying for messages...");
                var messages = client.GetMessages(client.Search(SearchCondition.Unseen())).ToList();
                LogSuccess(messages.Count + " messages recieved.");

                foreach (var message in messages)
                    ProcessMessageInfo(message);
            }
            catch (SocketException ex) //we were getting strange socket errors after time, and it looks like a reset solves things
            {
                CleanUp();
                _client = null; //this will get recreated the next time this daemon runs
                AlertManager.EmailProcessingFailure(DateTime.Now.to_S(), "Got that SocketException");
                LogFail(ex, "Hit SocketException. Trying to reset the IMAP Client.");
                Logger.GetLogger().Error("Hit SocketException. Trying to reset the IMAP Client.", ex);
            }
            catch (Exception e)
            {
                Logger.GetLogger().Error("Error occured in " + GetType().Name, e);
                LogFail(e);
            }
        }

        private void ProcessMessageInfo(MailMessage messageInfo)
        {
            var logString = "Processing message with subject '" + messageInfo.Subject + "'";
            Logger.GetLogger().Info(logString);

            lock (_testSubjects)
            {
                if (_testSubjects.Contains(messageInfo.Subject))
                {
                    LogEvent("Test message detected.");
                    _testSubjects.Remove(messageInfo.Subject);

                    if (TestMessageRecieved != null)
                    {
                        TestMessageRecieved(messageInfo.Subject);
                        LogSuccess();
                    }
                    else
                        LogFail(new Exception("No one was listening for test message event..."));

                    return;
                }
            }

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
                AlertManager.EmailProcessingFailure(messageInfo.Headers["Date"], e.Message);
                Logger.GetLogger().Error(string.Format("EmailProcessingFailure Reported. ObjectID = {0}", messageInfo.Headers["Message-ID"]));
                
            }
        }

        protected override void CleanUp()
        {
            if(_client != null)
                _client.Dispose();
        }
    }
}
