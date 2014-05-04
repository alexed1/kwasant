using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using S22.Imap;

using StructureMap;

namespace Daemons
{
    public class InboundEmail : Daemon
    {
        private readonly IImapClient _client;

        private static string GetIMAPServer()
        {
            return ConfigurationManager.AppSettings["InboundEmailHost"];
        }

        private static int GetIMAPPort()
        {
            int port;
            if (int.TryParse(ConfigurationManager.AppSettings["InboundEmailPort"], out port))
                return port;
            throw new Exception("Invalid value for 'InboundEmailPort'");
        }

        private static string GetUserName()
        {
            return ConfigurationManager.AppSettings["InboundEmailUserName"];
        }
        private static string GetPassword()
        {
            return ConfigurationManager.AppSettings["InboundEmailPassword"];
        }

        private static bool UseSSL()
        {
            bool useSSL;
            if (bool.TryParse(ConfigurationManager.AppSettings["InboundEmailUseSSL"], out useSSL))
                return useSSL;
            throw new Exception("Invalid value for 'InboundEmailUseSSL'");
        }

        public InboundEmail()
            : this(new ImapClient(GetIMAPServer(), GetIMAPPort(), GetUserName(), GetPassword(), AuthMethod.Login, UseSSL()))
        {
         
        }

        public InboundEmail(IImapClient client)
        {
            _client = client;
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            IEnumerable<uint> uids = _client.Search(SearchCondition.Unseen());
            List<MailMessage> messages = _client.GetMessages(uids).ToList();

            IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            EmailRepository emailRepository = new EmailRepository(unitOfWork);
            foreach (MailMessage message in messages)
            {
                BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(unitOfWork);
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);

                BookingRequest.ProcessBookingRequest(unitOfWork, bookingRequest);
            }
            emailRepository.UnitOfWork.SaveChanges();
        }

        protected override void CleanUp()
        {
            _client.Dispose();
        }
    }
}
