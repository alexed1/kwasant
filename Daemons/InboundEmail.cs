using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
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

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
        {
            
        }
        private static string GetIMAPServer()
        {
            return ConfigRepository.Get("InboundEmailHost");
        }

        private static int GetIMAPPort()
        {
            return ConfigRepository.Get<int>("InboundEmailPort");
        }

        private static string GetUserName()
        {
            return ConfigRepository.Get("INBOUND_EMAIL_USERNAME");
        }
        private static string GetPassword()
        {
            return ConfigRepository.Get("INBOUND_EMAIL_PASSWORD");
        }

        private static bool UseSSL()
        {
            return ConfigRepository.Get<bool>("InboundEmailUseSSL");
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
            IImapClient client;
            try
            {
                client = _client ??
                         new ImapClient(GetIMAPServer(), GetIMAPPort(), GetUserName(), GetPassword(), AuthMethod.Login, UseSSL());

                
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
            IEnumerable<uint> uids = client.Search(SearchCondition.Unseen()).ToList();

            string logString;

            //the difference in syntax makes it easy to have nonzero hits stand out visually in the log dashboard
            if (uids.Any())           
                logString = GetType().Name + " - " + uids.Count() + " emails found!";      
            else
                logString = GetType().Name + " - 0 emails found...";
            Logger.GetLogger().Info(logString);

            foreach (var uid in uids)
            {
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                BookingRequestRepository bookingRequestRepo = unitOfWork.BookingRequestRepository;
                
                var message = client.GetMessage(uid);                
                
                try
                {
                    BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                    //assign the owner of the booking request to be the owner of the From address
                    bookingRequest.User =
                        unitOfWork.UserRepository.FindOne(u => u.EmailAddress.Address == bookingRequest.From.Address);
                    BookingRequest.ProcessBookingRequest(unitOfWork, bookingRequest);

                    unitOfWork.SaveChanges();
                }
                catch (Exception e)
                {
                    Logger.GetLogger().Error("Failed to process inbound message.", e);
                    client.RemoveMessageFlags(uid, null, MessageFlag.Seen);
                    Logger.GetLogger().Info("Message marked as unread.");
                    throw new ApplicationException(e.Message);
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
