using System;
using System.Net.Mail;
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
        
        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
        {
            _configRepository = ObjectFactory.GetInstance<IConfigRepository>();   
        }

        //be careful about using this form. can get into problems involving disposal.
        public InboundEmail(IImapClient client, IConfigRepository configRepository)
        {
            _client = client;
            _configRepository = configRepository;
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

        private static void ProcessMessageInfo(MailMessage messageInfo)
        {
            var logString = "Processing message with subject '" + messageInfo.Subject + "'";
            Logger.GetLogger().Info(logString);
            
            IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = unitOfWork.BookingRequestRepository;

            try
            {
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, messageInfo);

                //assign the owner of the booking request to be the owner of the From address

                (new BookingRequest()).Process(unitOfWork, bookingRequest);

                unitOfWork.SaveChanges();

                AlertManager.BookingRequestCreated(bookingRequest.Id);
                AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
            }
            catch (Exception e)
            {
                AlertManager.EmailProcessingFailure(messageInfo.Headers["Date"], e.Message);
                Logger.GetLogger().Error(String.Format("EmailProcessingFailure Reported. ObjectID = {0}", messageInfo.Headers["Message-ID"]));
            }
        }

        protected override void CleanUp()
        {
            if(_client != null)
                _client.Dispose();
        }
    }
}
