using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IImapClient _client;
        private readonly IConfigRepository _configRepository;
        public string username;
        public string password;

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
            : this(null, ObjectFactory.GetInstance<IConfigRepository>())
        {
          
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

        private string GetUserName()
        {
            return _configRepository.Get("INBOUND_EMAIL_USERNAME");
        }
        private string GetPassword()
        {
            return _configRepository.Get("INBOUND_EMAIL_PASSWORD");
        }

        private bool UseSSL()
        {
            return _configRepository.Get<bool>("InboundEmailUseSSL");
        }

        

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            IImapClient client;
            try
            {
                client = _client ?? new ImapClient(GetIMAPServer(), GetIMAPPort(), UseSSL());
                string curUser = username ?? GetUserName();
                string curPwd = password ?? GetPassword();
                client.Login(curUser, curPwd, AuthMethod.Login );
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

            Logger.GetLogger().Info(GetType().Name + " - Querying inbound account...");
            var allMessageInfos = client.ListMailboxes()
                .SelectMany(mailbox => client
                                           .Search(SearchCondition.Unseen(), mailbox)
                                           .Select(uid => new { Mailbox = mailbox, Uid = uid, Message = client.GetMessage(uid, mailbox: mailbox) }))
                .Where(messageInfo => messageInfo.Message.From != null)
                .ToList();
            var messageInfos = allMessageInfos
                .Select(messageInfo => messageInfo.Message.Headers["Message-ID"])
                .Distinct(StringComparer.Ordinal)
                .Select(id => allMessageInfos.First(messageInfo => string.Equals(messageInfo.Message.Headers["Message-ID"], id, StringComparison.Ordinal)))
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
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                BookingRequestRepository bookingRequestRepo = unitOfWork.BookingRequestRepository;

                try
                {
                    BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, messageInfo.Message);
                    
                    //assign the owner of the booking request to be the owner of the From address

                    (new BookingRequest()).Process(unitOfWork, bookingRequest);

                    unitOfWork.SaveChanges();

                    AlertManager.BookingRequestCreated(bookingRequest.Id);
                    AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
                }
                catch (Exception e)
                {
                    AlertManager.EmailProcessingFailure(messageInfo.Message.Headers["Date"], e.Message);
                    Logger.GetLogger().Error(string.Format("EmailProcessingFailure Reported. ObjectID = {0}", messageInfo.Message.Headers["Message-ID"]));
                    client.AddMessageFlags(messageInfo.Uid, messageInfo.Mailbox, MessageFlag.Seen);
                }
            }

            client.Dispose();
        }

        protected override void CleanUp()
        {
            if(_client != null)
                _client.Dispose();
        }
    }
}
