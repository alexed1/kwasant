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
using UtilitiesLib.Logging;

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

            Logger.GetLogger().Info(GetType().Name + " - Querying inbound account...");
            IEnumerable<uint> uids = _client.Search(SearchCondition.Unseen()).ToList();
            Logger.GetLogger().Info(GetType().Name + " - " + uids.Count() + " emails found...");

            foreach (var uid in uids)
            {
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                BookingRequestRepository bookingRequestRepo = new BookingRequestRepository(unitOfWork);

                var message = _client.GetMessage(uid);
                try
                {
                    BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                    BookingRequest.ProcessBookingRequest(unitOfWork, bookingRequest);
                }
                catch (Exception e)
                {
                    Logger.GetLogger().Error("Failed to process inbound message.", e);
                    _client.RemoveMessageFlags(uid, null, MessageFlag.Seen);
                    Logger.GetLogger().Info("Message marked as unread.");
                }

                bookingRequestRepo.UnitOfWork.SaveChanges();
            }
        }

        protected override void CleanUp()
        {
            _client.Dispose();
        }
    }
}
