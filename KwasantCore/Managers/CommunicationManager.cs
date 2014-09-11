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

        public CommunicationManager(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
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

            foreach (var attendee in negotiationDO.Attendees)
            {
                var emailDO = new EmailDO();
                emailDO.From = uow.EmailAddressRepository.GetOrCreateEmailAddress(_configRepository.Get("EmailFromAddress_DirectMode"), _configRepository.Get("EmailFromName_DirectMode"));
                emailDO.AddEmailRecipient(EmailParticipantType.To, attendee.EmailAddress);
                //emailDO.Subject = "Regarding:" + negotiationDO.Name;
                emailDO.Subject = "Need Your Response on " + negotiationDO.BookingRequest.User.FirstName + " "
                    + (negotiationDO.BookingRequest.User.LastName != null ? negotiationDO.BookingRequest.User.LastName : "") + "event: " + negotiationDO.Name;

                var responseUrl = String.Format("{0}NegotiationResponse/View?negotiationID={1}", 
                    Server.ServerUrl, 
                    negotiationDO.Id);

                uow.EmailRepository.Add(emailDO);

                string templateName = "clarification_request_v3";
                
                uow.EnvelopeRepository.ConfigureTemplatedEmail(emailDO, templateName, new Dictionary<string, string>() { { "RESP_URL", responseUrl } });
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

                outboundEmail.From = uow.EmailAddressRepository.GetOrCreateEmailAddress("scheduling@kwasant.com", "Kwasant Scheduling Services");

                outboundEmail.AddEmailRecipient(EmailParticipantType.To, uow.EmailAddressRepository.GetOrCreateEmailAddress(toAddress));

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