using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using Microsoft.WindowsAzure;
using S22.Imap;

using StructureMap;

namespace Daemons
{
    public class InboundEmail : Daemon
    {
        private readonly bool _isValid = true;
        private readonly IImapClient _client;
        private static string GetIMAPServer()
        {
            return CloudConfigurationManager.GetSetting("InboundEmailHost");
        }

        private static int GetIMAPPort()
        {
            int port;
            if (int.TryParse(CloudConfigurationManager.GetSetting("InboundEmailPort"), out port))
                return port;
            throw new Exception("Invalid value for 'InboundEmailPort'");
        }

        private static string GetUserName()
        {
            return CloudConfigurationManager.GetSetting("INBOUND_EMAIL_USERNAME");
        }
        private static string GetPassword()
        {
            return CloudConfigurationManager.GetSetting("INBOUND_EMAIL_PASSWORD");
        }

        private static bool UseSSL()
        {
            bool useSSL;
            if (bool.TryParse(CloudConfigurationManager.GetSetting("InboundEmailUseSSL"), out useSSL))
                return useSSL;
            throw new Exception("Invalid value for 'InboundEmailUseSSL'");
        }

        public InboundEmail()
        {
            try
            {
                _client = new ImapClient(GetIMAPServer(), GetIMAPPort(), GetUserName(), GetPassword(), AuthMethod.Login, UseSSL());
            }
            catch (Exception ex)
            {
                //We log in the future
                _isValid = false;
                throw new ApplicationException(ex.Message); //we were generating exceptions here and missing them
            }
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
            if (!_isValid)
                return;

            IEnumerable<uint> uids = _client.Search(SearchCondition.Unseen());
            List<MailMessage> messages = _client.GetMessages(uids).ToList();

            IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(unitOfWork);
            foreach (MailMessage message in messages)
            {
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                BookingRequest.ProcessBookingRequest(unitOfWork, bookingRequest);
            }
            bookingRequestRepo.UnitOfWork.SaveChanges();
        }

        protected override void CleanUp()
        {
            _client.Dispose();
        }
    }
}
