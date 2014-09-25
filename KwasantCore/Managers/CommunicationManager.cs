using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using Data.Validations;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using StructureMap;
using Microsoft.WindowsAzure;
using KwasantCore.Services;
using Utilities;
using Encoding = System.Text.Encoding;

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
            AlertManager.AlertCustomerCreated += NewCustomerWorkflow;
        }

        //this is called when a new customer is created, because the communication manager has subscribed to the alertCustomerCreated alert.
        public void NewCustomerWorkflow(string curUserId)
        {
            GenerateWelcomeEmail(curUserId);  
        }

        public void GenerateWelcomeEmail(string curUserId)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // WARNING: 'user' parameter must not be used as reference in scope of this UnitOfWork as it is attached to another UnitOfWork
                var curUser = uow.UserRepository.GetByKey(curUserId);
                EmailDO curEmail = new EmailDO();
                curEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(_configRepository.Get("EmailFromAddress_DirectMode"), _configRepository.Get("EmailFromName_DirectMode"));
                curEmail.AddEmailRecipient(EmailParticipantType.To, curUser.EmailAddress);
                curEmail.Subject = "Welcome to Kwasant";
                uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmail, "welcome_to_kwasant_v2", null);
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
                emailDO.From = _emailAddress.GetFromEmailAddress(uow, attendee.EmailAddress, negotiationDO.BookingRequest.User);
                emailDO.AddEmailRecipient(EmailParticipantType.To, attendee.EmailAddress);
                //emailDO.Subject = "Regarding:" + negotiationDO.Name;
                emailDO.Subject = string.Format("Need Your Response on {0} {1} event: {2}", 
                    negotiationDO.BookingRequest.User.FirstName, 
                    (negotiationDO.BookingRequest.User.LastName ?? ""), 
                    negotiationDO.Name);

                var responseUrl = String.Format("NegotiationResponse/View?negotiationID={0}", 
                    negotiationDO.Id);

                var authToken = new AuthorizationToken();
                var tokenURL = authToken.GetAuthorizationTokenURL(uow, responseUrl, user.GetOrCreateFromBR(uow, attendee.EmailAddress));

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
                    var currentQuestion = String.Format(actualHtml, i + 1, question.Text, String.Join(", ", question.Answers.Select(a => a.Text)));
                    generated.Add(currentQuestion);
                }

                string templateName;
                // Max Kostyrkin: currently User#GetMode returns Direct if user has a booking request or has a password, otherwise Delegate.
                switch (user.GetMode(uow, user.Get(uow, attendee.EmailAddress)))
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

                uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, templateName,
                    new Dictionary<string, string>
                    {
                        {"RESP_URL", tokenURL}
                        ,
                        {"questions", String.Join("<br/>", generated)}
                    });


            }
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
                } else if (communicationConfig.CommunicationType == CommunicationType.Email)
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
        public List<RazorAttendeeViewModel> Attendees { get; set; }

        public RazorViewModel(EventDO ev, String userID)
        {
            IsAllDay = ev.IsAllDay;
            StartDate = ev.StartDate.DateTime;
            EndDate = ev.EndDate.DateTime;
            Summary = ev.Summary;
            Description = ev.Description;
            Location = ev.Location;
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