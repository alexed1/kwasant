using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
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
        private IImapClient[] _clients;
        private readonly IConfigRepository _configRepository;

        //warning: if you remove this empty constructor, Activator calls to this type will fail.
        public InboundEmail()
            : this(ObjectFactory.GetInstance<IConfigRepository>(), null)
        {
          
        }

        //be careful about using this form. can get into problems involving disposal.
        public InboundEmail(IConfigRepository configRepository, params IImapClient[] clients)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            if (clients != null)
            {
                if (clients.Any(c => c == null))
                    throw new ArgumentException("Some of the passed clients are null.", "clients");
                _clients = new IImapClient[clients.Length];
                clients.CopyTo(_clients, 0);
            }
            _configRepository = configRepository;
        }

        private IImapClient CreateIntakeClient()
        {
            return new ImapClient(
                _configRepository.Get("InboundEmailHost"),
                _configRepository.Get<int>("InboundEmailPort"),
                _configRepository.Get("INBOUND_EMAIL_USERNAME"),
                _configRepository.Get("INBOUND_EMAIL_PASSWORD"),
                AuthMethod.Login,
                _configRepository.Get<bool>("InboundEmailUseSSL"));
        }

/*
        private IImapClient CreateInvitationRepliesClient()
        {
            return new ImapClient(
                _configRepository.Get("SchedulingEmailHost"),
                _configRepository.Get<int>("SchedulingEmailPort"),
                _configRepository.Get("Scheduling_EMAIL_USERNAME"),
                _configRepository.Get("Scheduling_EMAIL_PASSWORD"),
                AuthMethod.Login,
                _configRepository.Get<bool>("SchedulingEmailUseSSL"));
        }
*/

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromSeconds(10).TotalMilliseconds; }
        }

        protected override void Run()
        {
            try
            {
                if (_clients == null)
                {
                    _clients = new[]
                                   {
                                       CreateIntakeClient(),
                                       //CreateInvitationRepliesClient()
                                   };
                }
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
            
            Logger.GetLogger().Info(GetType().Name + " - Querying inbound accounts...");
            var messageInfos = _clients.AsParallel().SelectMany(GetMessageInfos).ToArray();

            string logString;

            //the difference in syntax makes it easy to have nonzero hits stand out visually in the log dashboard
            if (messageInfos.Any())
                logString = GetType().Name + " - " + messageInfos.Count() + " emails found!";
            else
                logString = GetType().Name + " - 0 emails found...";
            Logger.GetLogger().Info(logString);

            foreach (var messageInfo in messageInfos)
            {
                try
                {
                    if (IsInvitationResponse(messageInfo))
                    {
                        ProcessInvitationResponse(messageInfo);
                    }
                    else
                    {
                        ProcessBookingRequest(messageInfo);
                    }
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

        private bool IsInvitationResponse(InboundEmailMessageInfo messageInfo)
        {
            var attachedCalendar = messageInfo.Message.AlternateViews
                .FirstOrDefault(av => string.Equals(av.ContentType.MediaType, "application/ics", StringComparison.Ordinal));
            if (attachedCalendar != null)
            {
                string content;
                using (var contentStream = new MemoryStream())
                {
                    attachedCalendar.ContentStream.CopyTo(contentStream);
                    attachedCalendar.ContentStream.Position = 0;
                    contentStream.Position = 0;
                    using (var sr = new StreamReader(contentStream))
                    {
                        content = sr.ReadToEnd();
                    }
                }
                if (content.Contains("METHOD:REPLY"))
                {
                    return true;
                }
            }
            return false;
        }

        private void ProcessInvitationResponse(InboundEmailMessageInfo messageInfo)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                InvitationResponseDO curInvitationResponse =
                    Email.ConvertMailMessageToEmail(uow.InvitationResponseRepository,
                                                    messageInfo.Message);

                (new InvitationResponse()).Process(uow, curInvitationResponse);

                uow.SaveChanges();

                AlertManager.EmailReceived(curInvitationResponse.Id, uow.UserRepository.GetByEmailAddress(curInvitationResponse.From).Id);
            }
        }

        private void ProcessBookingRequest(InboundEmailMessageInfo messageInfo)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo,
                                                                                  messageInfo.Message);

                //assign the owner of the booking request to be the owner of the From address

                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                AlertManager.BookingRequestCreated(bookingRequest.Id);
                AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
            }
        }

        private class InboundEmailMessageInfo
        {
            public IImapClient Client { get; set; }
            public string Mailbox { get; set; }
            public uint Uid { get; set; }
            public MailMessage Message { get; set; }
        }

        private static IEnumerable<InboundEmailMessageInfo> GetMessageInfos(IImapClient client)
        {
            var allMessageInfos = client.ListMailboxes()
                .SelectMany(mailbox => client
                                           .Search(SearchCondition.Unseen(), mailbox)
                                           .Select(uid => new InboundEmailMessageInfo { Client = client, Mailbox = mailbox, Uid = uid, Message = client.GetMessage(uid, mailbox: mailbox) }))
                .Where(messageInfo => messageInfo.Message.From != null)
                .ToList();
            return allMessageInfos
                .Select(messageInfo => messageInfo.Message.Headers["Message-ID"])
                .Distinct(StringComparer.Ordinal)
                .Select(id => allMessageInfos.First(messageInfo => string.Equals(messageInfo.Message.Headers["Message-ID"], id, StringComparison.Ordinal)))
                .ToList();
        }

        protected override void CleanUp()
        {
            if (_clients != null)
                Array.ForEach(_clients, client => client.Dispose());
        }
    }
}
