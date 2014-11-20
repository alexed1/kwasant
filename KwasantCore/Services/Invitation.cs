using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers;
using KwasantICS.DDay.iCal;
using KwasantICS.DDay.iCal.Serialization.iCalendar.Serializers;
using RazorEngine;
using Utilities;
using Encoding = System.Text.Encoding;

namespace KwasantCore.Services
{
    public class Invitation
    {
        private readonly IConfigRepository _configRepository;
        private readonly EmailAddress _emailAddress;

        public Invitation(IConfigRepository configRepository, EmailAddress emailAddress)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
            _emailAddress = emailAddress;
        }

        public void Dispatch(IUnitOfWork uow, InvitationDO curInvitation)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (curInvitation == null)
                throw new ArgumentNullException("curInvitation");

            uow.EnvelopeRepository.ConfigurePlainEmail(curInvitation);
        }

        public InvitationDO Generate(IUnitOfWork uow, int curType, AttendeeDO curAttendee, EventDO curEvent)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (curAttendee == null)
                throw new ArgumentNullException("curAttendee");
            if (curEvent == null)
                throw new ArgumentNullException("curEvent");

            string replyToEmail = _configRepository.Get("replyToEmail");

            var emailAddressRepository = uow.EmailAddressRepository;
            if (curEvent.Attendees == null)
                curEvent.Attendees = new List<AttendeeDO>();

            InvitationDO curInvitation = new InvitationDO();
            curInvitation.ConfirmationStatus = ConfirmationStatus.Unnecessary;

            var toEmailAddress = emailAddressRepository.GetOrCreateEmailAddress(curAttendee.EmailAddress.Address);
            toEmailAddress.Name = curAttendee.Name;
            curInvitation.AddEmailRecipient(EmailParticipantType.To, toEmailAddress);

            //configure the sender information
            curInvitation.From = _emailAddress.GetFromEmailAddress(uow, toEmailAddress, curEvent.CreatedBy);

            var replyToAddress = emailAddressRepository.GetOrCreateEmailAddress(replyToEmail);
            curInvitation.ReplyTo = replyToAddress;

            var userID = uow.UserRepository.GetOrCreateUser(curAttendee.EmailAddress).Id;

            if (curType == InvitationType.ChangeNotification)
            {
                curInvitation = GenerateChangeNotification(uow, curInvitation, curEvent, userID);
            }
            else
            {
                curInvitation = GenerateInitialInvite(uow, curInvitation, curEvent, userID);
            }

            //prepare the outbound email
            curInvitation.EmailStatus = EmailState.Queued;
            if (curEvent.Emails == null)
                curEvent.Emails = new List<EmailDO>();

            var calendar = Event.GenerateICSCalendarStructure(curEvent);
            AttachCalendarToEmail(calendar, curInvitation);

            curEvent.Emails.Add(curInvitation);

            uow.InvitationRepository.Add(curInvitation);

            return curInvitation;
        }

        private static String GetAuthTokenForBaseURL(IUnitOfWork uow, string userID)
        {
            return uow.AuthorizationTokenRepository.GetAuthorizationTokenURL(Server.ServerUrl, userID, "Invitation", new Dictionary<string, object>
                {
                    {"action", "Clicked on Invitation Header"}
                });
        }

        private InvitationDO GenerateInitialInvite(IUnitOfWork uow, InvitationDO curInvitation, EventDO curEvent, string userID)
        {
            //string endtime = curEvent.EndDate.ToUniversalTime().ToString("hh:mmtt");
            //string subjectDate = curEvent.StartDate.ToUniversalTime().ToString("ddd MMM dd, yyyy hh:mmtt - ") + endtime + " +00:00";
            string endtime = curEvent.EndDate.ToString("hh:mm tt");
            var timezone = System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            string subjectDate = curEvent.StartDate.ToString("ddd MMM dd, yyyy hh:mm tt - ") + endtime + " +" + timezone.ToString();


            curInvitation.InvitationType = InvitationType.InitialInvite;
            curInvitation.Subject = String.Format(_configRepository.Get("emailSubject"), GetOriginatorName(curEvent), curEvent.Summary, subjectDate);
            curInvitation.HTMLText = GetEmailHTMLTextForNew(uow, curEvent, userID);
            curInvitation.PlainText = GetEmailPlainTextForNew(uow, curEvent, userID);
            return curInvitation;
        }

        private InvitationDO GenerateChangeNotification(IUnitOfWork uow, InvitationDO curInvitation, EventDO curEvent, string userID)
        {
            string endtime = curEvent.EndDate.ToString("hh:mm tt");
            var timezone = System.TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
            string subjectDate = curEvent.StartDate.ToString("ddd MMM dd, yyyy hh:mm tt - ") + endtime + " +" + timezone.ToString();

            curInvitation.InvitationType = InvitationType.ChangeNotification;
            curInvitation.Subject = String.Format(_configRepository.Get("emailSubjectUpdated"), GetOriginatorName(curEvent), curEvent.Summary, subjectDate);
            curInvitation.HTMLText = GetEmailHTMLTextForUpdate(uow, curEvent, userID);
            curInvitation.PlainText = GetEmailPlainTextForUpdate(uow, curEvent, userID);
            return curInvitation;
        }

        private void AttachCalendarToEmail(iCalendar iCal, EmailDO emailDO)
        {
            iCalendarSerializer serializer = new iCalendarSerializer(iCal);
            string fileToAttach = serializer.Serialize(iCal);

            AttachmentDO attachmentDO = GetAttachment(fileToAttach);

            attachmentDO.Email = emailDO;
            emailDO.Attachments.Add(attachmentDO);
        }


        private AttachmentDO GetAttachment(string fileToAttach)
        {
            return Email.CreateNewAttachment(
                new System.Net.Mail.Attachment(
                    new MemoryStream(Encoding.UTF8.GetBytes(fileToAttach)),
                    new ContentType { MediaType = "application/ics", Name = "invite.ics" }
                    ) { TransferEncoding = TransferEncoding.Base64 });
        }

        //if we have a first name and last name, use them together
        //else if we have a first name only, use that
        //else if we have just an email address, use the portion preceding the @ unless there's a name
        //else throw
        public string GetOriginatorName(EventDO curEventDO)
        {
            UserDO originator = curEventDO.CreatedBy;
            return User.GetDisplayName(originator);
            }

        private String GetEmailHTMLTextForUpdate(IUnitOfWork uow, EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.HTMLEventInvitation_Update, new RazorViewModel(eventDO, userID, GetAuthTokenForBaseURL(uow, userID)));
        }

        private String GetEmailPlainTextForUpdate(IUnitOfWork uow, EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.PlainEventInvitation_Update, new RazorViewModel(eventDO, userID, GetAuthTokenForBaseURL(uow, userID)));
        }

        private String GetEmailHTMLTextForNew(IUnitOfWork uow, EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.HTMLEventInvitation, new RazorViewModel(eventDO, userID, GetAuthTokenForBaseURL(uow, userID)));
        }

        private String GetEmailPlainTextForNew(IUnitOfWork uow, EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.PlainEventInvitation, new RazorViewModel(eventDO, userID, GetAuthTokenForBaseURL(uow, userID)));
        }

    }
}
