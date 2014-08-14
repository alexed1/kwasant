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
        private  ImapClient _client;
        public string username;
        public string password;

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
        {
          
        }

        //be careful about using this form. can get into problems involving disposal.
        public InboundEmail(ImapClient client)
        {
            _client = client;
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

        

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            ImapClient client;
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
            var messages = client.ListMailboxes()
                .SelectMany(mailbox => client
                                           .Search(SearchCondition.Unseen(), mailbox)
                                           .Select(uid => new {Mailbox = mailbox, Uid = uid}))
                .ToList();
            


            string logString;

            //the difference in syntax makes it easy to have nonzero hits stand out visually in the log dashboard
            if (messages.Any())           
                logString = GetType().Name + " - " + messages.Count() + " emails found!";      
            else
                logString = GetType().Name + " - 0 emails found...";
            Logger.GetLogger().Info(logString);

            foreach (var emailMessage in messages)
            {
                IUnitOfWork unitOfWork = ObjectFactory.GetInstance<IUnitOfWork>();
                BookingRequestRepository bookingRequestRepo = unitOfWork.BookingRequestRepository;
                
                var message = client.GetMessage(emailMessage.Uid);                
                if (message.From == null)
                {
                    continue;
                }

                try
                {
                    BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);
                    //assign the owner of the booking request to be the owner of the From address

                    (new BookingRequest()).Process(unitOfWork, bookingRequest);

                    unitOfWork.SaveChanges();
                    
                    AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
                }
                catch (Exception e)
                {
                    AlertManager.EmailProcessingFailure(message.Headers["Date"], e.Message);
                    Logger.GetLogger().Error("EmailProcessingFailure Reported. ObjectID =" + emailMessage);
                    client.AddMessageFlags(emailMessage.Uid, emailMessage.Mailbox, MessageFlag.Seen);
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
