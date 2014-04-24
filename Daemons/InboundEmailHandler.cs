using System;
using System.Collections.Generic;
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
    public class InboundEmailHandler : Daemon
    {
        private readonly ImapClient _client;

        private static string GetIMAPServer()
        {
            return "imap.gmail.com";
        }

        private static int GetIMAPPort()
        {
            return 993;
        }

        private string GetUserName()
        {
            return "alexlucre1";
        }
        private string GetPassword()
        {
            return "lucrelucre";
        }

        private static bool UseSSL()
        {
            return true;
        }

        public InboundEmailHandler()
        {
            _client = new ImapClient(GetIMAPServer(), GetIMAPPort(), GetUserName(), GetPassword(), AuthMethod.Login, UseSSL());
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
