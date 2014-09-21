using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Validations;
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

        public Invitation(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
        }

        public void Dispatch(IUnitOfWork uow, InvitationDO curInvitation)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (curInvitation == null)
                throw new ArgumentNullException("curInvitation");

            Email email = new Email(uow);
            email.Send(curInvitation);
        }

        public InvitationDO Generate(IUnitOfWork uow, int curType, AttendeeDO curAttendee, EventDO curEvent)
        {
            if (uow == null)
                throw new ArgumentNullException("uow");
            if (curAttendee == null)
                throw new ArgumentNullException("curAttendee");
            if (curEvent == null)
                throw new ArgumentNullException("curEvent");

            string fromEmail, fromName;
            if (curAttendee.EmailAddress.Address == curEvent.BookingRequest.User.EmailAddress.Address)
            {
                fromEmail = _configRepository.Get("EmailFromAddress_DirectMode");
                fromName = _configRepository.Get("EmailFromAddress_DirectMode");
            }
            else
            {
                fromEmail = _configRepository.Get("EmailFromAddress_DelegateMode");
                fromName = String.Format(_configRepository.Get("EmailFromName_DelegateMode"), GetOriginatorName(curEvent));
            }
            string replyToEmail = _configRepository.Get("replyToEmail");

            var emailAddressRepository = uow.EmailAddressRepository;
            if (curEvent.Attendees == null)
                curEvent.Attendees = new List<AttendeeDO>();

            InvitationDO curInvitation = new InvitationDO();
            curInvitation.ConfirmationStatus = ConfirmationStatus.Unnecessary;

            //configure the sender information
            var fromEmailAddr = emailAddressRepository.GetOrCreateEmailAddress(fromEmail);
            fromEmailAddr.Name = fromName;
            curInvitation.From = fromEmailAddr;

            var replyToAddress = emailAddressRepository.GetOrCreateEmailAddress(replyToEmail);
            curInvitation.ReplyTo = replyToAddress;

            var toEmailAddress = emailAddressRepository.GetOrCreateEmailAddress(curAttendee.EmailAddress.Address);
            toEmailAddress.Name = curAttendee.Name;
            curInvitation.AddEmailRecipient(EmailParticipantType.To, toEmailAddress);

            var user = new User();
            var userID = user.GetOrCreateFromBR(uow, curAttendee.EmailAddress).Id;

            if (curType == InvitationType.ChangeNotification)
            {
                curInvitation = GenerateChangeNotification(curInvitation, curEvent, userID);
            }
            else
            {
                curInvitation = GenerateInitialInvite(curInvitation, curEvent, userID);
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

        private InvitationDO GenerateInitialInvite(InvitationDO curInvitation, EventDO curEvent, string userID)
        {
            string endtime = curEvent.EndDate.ToUniversalTime().ToString("hh:mmtt");
            string subjectDate = curEvent.StartDate.ToUniversalTime().ToString("ddd MMM dd, yyyy hh:mmtt - ") + endtime + " +00:00";

            curInvitation.InvitationType = InvitationType.InitialInvite;
            curInvitation.Subject = String.Format(_configRepository.Get("emailSubject"), GetOriginatorName(curEvent), curEvent.Summary, subjectDate);
            curInvitation.HTMLText = GetEmailHTMLTextForNew(curEvent, userID);
            curInvitation.PlainText = GetEmailPlainTextForNew(curEvent, userID);
            return curInvitation;
        }

        private InvitationDO GenerateChangeNotification(InvitationDO curInvitation, EventDO curEvent, string userID)
        {
            string endtime = curEvent.EndDate.ToUniversalTime().ToString("hh:mmtt");
            string subjectDate = curEvent.StartDate.ToUniversalTime().ToString("ddd MMM dd, yyyy hh:mmtt - ") + endtime + " +00:00";

            curInvitation.InvitationType = InvitationType.ChangeNotification;
            curInvitation.Subject = String.Format(_configRepository.Get("emailSubjectUpdated"), GetOriginatorName(curEvent), curEvent.Summary, subjectDate);
            curInvitation.HTMLText = GetEmailHTMLTextForUpdate(curEvent, userID);
            curInvitation.PlainText = GetEmailPlainTextForUpdate(curEvent, userID);
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
            string firstName = originator.FirstName;
            string lastName = originator.LastName;
            if (firstName != null)
            {
                if (lastName == null)
                    return firstName;

                return firstName + " " + lastName;
            }

            EmailAddressDO curEmailAddress = originator.EmailAddress;
            if (curEmailAddress.Name != null)
                return curEmailAddress.Name;

            if (curEmailAddress.Address.IsEmailAddress())
                return curEmailAddress.Address.Split(new[] { '@' })[0];

            throw new ArgumentException("Failed to extract originator info from this Event. Something needs to be there.");
        }

        private String GetEmailHTMLTextForUpdate(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.HTMLEventInvitation_Update, new RazorViewModel(eventDO, userID));
        }

        private String GetEmailPlainTextForUpdate(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.PlainEventInvitation_Update, new RazorViewModel(eventDO, userID));
        }

        private String GetEmailHTMLTextForNew(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.HTMLEventInvitation, new RazorViewModel(eventDO, userID));
        }

        private String GetEmailPlainTextForNew(EventDO eventDO, String userID)
        {
            return Razor.Parse(Properties.Resources.PlainEventInvitation, new RazorViewModel(eventDO, userID));
        }

    }
}
