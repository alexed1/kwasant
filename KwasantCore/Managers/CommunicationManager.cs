using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Interfaces;
using KwasantCore.Managers.APIManagers.Packagers;
using StructureMap;
using Microsoft.WindowsAzure;
using KwasantCore.Services;
using Utilities;

namespace KwasantCore.Managers
{
    public class CommunicationManager
    {
        private readonly IConfigRepository _configRepository;
        private readonly EmailAddress _emailAddress;

        public CommunicationManager(IConfigRepository configRepository, EmailAddress emailAddress)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            if (emailAddress == null)
                throw new ArgumentNullException("emailAddress");
            _configRepository = configRepository;
            _emailAddress = emailAddress;
        }

        //Register for interesting events
        public void SubscribeToAlerts()
        {
            AlertManager.AlertExplicitCustomerCreated += NewExplicitCustomerWorkflow;
            AlertManager.AlertCustomerCreated += NewCustomerWorkflow;
            AlertManager.AlertBookingRequestCreated += BookingRequestCreated;
            AlertManager.AlertBookingRequestNeedsProcessing += BookingRequestNeedsProcessing;
        }

        private void BookingRequestNeedsProcessing(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                var email = ObjectFactory.GetInstance<Email>();
                string message = "BookingRequest Needs Processing <br/>Subject : " + bookingRequestDO.Subject;
                string subject = "BookingRequest Needs Processing";
                string toRecipient = _configRepository.Get("EmailAddress_BrNotify");
                string fromAddress = _configRepository.Get<string>("EmailAddress_GeneralInfo");
                EmailDO curEmail = email.GenerateBasicMessage(uow, subject, message, fromAddress, toRecipient);
                uow.EnvelopeRepository.ConfigurePlainEmail(curEmail);
                uow.SaveChanges();
            }
        }

        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewExplicitCustomerWorkflow(string curUserId)
        {
            GenerateWelcomeEmail(curUserId);
        }

        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(UserDO userDO)
        {
            ObjectFactory.GetInstance<ITracker>().Identify(userDO);
        }

        public void BookingRequestCreated(int bookingRequestId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var bookingRequestDO = uow.BookingRequestRepository.GetByKey(bookingRequestId);
                ObjectFactory.GetInstance<ITracker>().Track(bookingRequestDO.User, "BookingRequest", "Submit", new Dictionary<string, object> { { "BookingRequestId", bookingRequestDO.Id } });
            }
        }

        public void GenerateWelcomeEmail(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var curUser = uow.UserRepository.GetByKey(curUserId);
                EmailDO curEmail = new EmailDO();
                curEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(_configRepository.Get("EmailFromAddress_DirectMode"), _configRepository.Get("EmailFromName_DirectMode"));
                curEmail.AddEmailRecipient(EmailParticipantType.To, curUser.EmailAddress);
                curEmail.Subject = "Welcome to Kwasant";
               
                uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmail, "2e411208-7a0d-4a72-a005-e39ae018d708", null); //welcome to kwasant v2 template
                uow.SaveChanges();
            }
        }

        public void DispatchNegotiationRequests(IUnitOfWork uow, int negotiationID)
        {
            DispatchNegotiationRequests(uow, uow.NegotiationsRepository.GetByKey(negotiationID));
        }

        public void DispatchNegotiationRequests(IUnitOfWork uow, NegotiationDO negotiationDO)
        {
            if (negotiationDO.Attendees == null)
                return;

            var user = ObjectFactory.GetInstance<User>();
            foreach (var attendee in negotiationDO.Attendees)
            {
                var emailDO = new EmailDO();
                var emailAddressDO = _emailAddress.GetFromEmailAddress(uow, attendee.EmailAddress, negotiationDO.BookingRequest.User);
                emailDO.From = emailAddressDO;
                emailDO.FromID = emailAddressDO.Id;
                emailDO.AddEmailRecipient(EmailParticipantType.To, attendee.EmailAddress);
                //emailDO.Subject = "Regarding:" + negotiationDO.Name;
                emailDO.Subject = string.Format("Need Your Response on {0} {1} event: {2}",
                    negotiationDO.BookingRequest.User.FirstName,
                    (negotiationDO.BookingRequest.User.LastName ?? ""),
                    "RE: " + negotiationDO.Name);

                var responseUrl = String.Format("NegotiationResponse/View?negotiationID={0}", negotiationDO.Id);

                var userDO = uow.UserRepository.GetOrCreateUser(attendee.EmailAddress);
                var tokenURL = uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(responseUrl, userDO);

                uow.EmailRepository.Add(emailDO);
                var actualHtml =
                    @"
{0}. {1}? <br/>
Proposed Answers: {2}
";
                var generated = new List<String>();
                for (var i = 0; i < negotiationDO.Questions.Count; i++)
                {
                    var question = negotiationDO.Questions[i];
                    var currentQuestion = String.Format(actualHtml, i + 1, question.Text, question.Answers.Any() ? String.Join(", ", question.Answers.Select(a => a.Text)) : "[None proposed]");
                    generated.Add(currentQuestion);
                }

                string templateName;
                // Max Kostyrkin: currently User#GetMode returns Direct if user has a booking request or has a password, otherwise Delegate.
                switch (user.GetMode(userDO))
                {
                    case CommunicationMode.Direct:
                        templateName = _configRepository.Get("CR_template_for_creator");

                        break;
                    case CommunicationMode.Delegate:
                        templateName = _configRepository.Get("CR_template_for_existing_user");

                        break;
                    case CommunicationMode.Precustomer:
                        templateName = _configRepository.Get("CR_template_for_precustomer");

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var currBr = new BookingRequest();
                
                uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, templateName,
                    new Dictionary<string, string>
                    {
                        {"RESP_URL", tokenURL},
                        {"questions", String.Join("<br/>", generated)},
                        {"conversationthread", currBr.GetConversationThread(negotiationDO.BookingRequest)}
                    });
            }
            negotiationDO.NegotiationState = NegotiationState.AwaitingClient;
        }

        private bool EventHasChanged(IUnitOfWork uow, EventDO eventDO)
        {
            //Stub method for now
            return true;
        }

        public void ProcessBRNotifications(IList<BookingRequestDO> bookingRequests)
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            CommunicationConfigurationRepository communicationConfigurationRepo = uow.CommunicationConfigurationRepository;
            foreach (CommunicationConfigurationDO communicationConfig in communicationConfigurationRepo.GetAll().ToList())
            {
                if (communicationConfig.CommunicationType == CommunicationType.Sms)
                {
                    SendBRSMSes(bookingRequests);
                }
                else if (communicationConfig.CommunicationType == CommunicationType.Email)
                {
                    SendBREmails(communicationConfig.ToAddress, bookingRequests, uow);
                }
                else
                {
                    throw new Exception(String.Format("Invalid communication type '{0}'", communicationConfig.CommunicationType));
                }
            }
            uow.SaveChanges();
        }

        private void SendBRSMSes(IEnumerable<BookingRequestDO> bookingRequests)
        {
            if (bookingRequests.Any())
            {
                var twil = ObjectFactory.GetInstance<ISMSPackager>();
                string toNumber = CloudConfigurationManager.GetSetting("TwilioToNumber");
                twil.SendSMS(toNumber, "Inbound Email has been received");
            }
        }

        private void SendBREmails(String toAddress, IEnumerable<BookingRequestDO> bookingRequests, IUnitOfWork uow)
        {
            EmailRepository emailRepo = uow.EmailRepository;
            const string message = "A new booking request has been created. From '{0}'.";
            foreach (BookingRequestDO bookingRequest in bookingRequests)
            {
                EmailDO outboundEmail = new EmailDO
                {
                    Subject = "New booking request!",
                    HTMLText = String.Format(message, bookingRequest.From.Address),
                    EmailStatus = EmailState.Queued
                };

                var toEmailAddress = uow.EmailAddressRepository.GetOrCreateEmailAddress(toAddress);
                outboundEmail.AddEmailRecipient(EmailParticipantType.To, toEmailAddress);
                outboundEmail.From = _emailAddress.GetFromEmailAddress(uow, toEmailAddress, bookingRequest.User);

                uow.EnvelopeRepository.ConfigurePlainEmail(outboundEmail);
                emailRepo.Add(outboundEmail);
            }
        }
    }

    public class RazorViewModel
    {
        public String EmailBasicText { get { return ObjectFactory.GetInstance<IConfigRepository>().Get("emailBasicText"); } }
        public String UserID { get; set; }
        public bool IsAllDay { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public String Summary { get; set; }
        public String Description { get; set; }
        public String Location { get; set; }
        public String AuthTokenURL { get; set; }
        public List<RazorAttendeeViewModel> Attendees { get; set; }

        public RazorViewModel(EventDO ev, String userID, String authTokenURL)
        {
            IsAllDay = ev.IsAllDay;
            StartDate = ev.StartDate.DateTime;
            EndDate = ev.EndDate.DateTime;
            Summary = ev.Summary;
            Description = ev.Description;
            Location = ev.Location;
            AuthTokenURL = authTokenURL;
            Attendees = ev.Attendees.Select(a => new RazorAttendeeViewModel { Name = a.Name, EmailAddress = a.EmailAddress.Address }).ToList();
            UserID = userID;
        }

        public class RazorAttendeeViewModel
        {
            public String EmailAddress { get; set; }
            public String Name { get; set; }
        }
    }
}