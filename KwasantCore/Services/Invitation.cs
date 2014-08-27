using System;
using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Data.Validations;
using KwasantCore.Managers;
using RazorEngine;
using Utilities;

namespace KwasantCore.Services
{
    public class Invitation
    {
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

            string fromEmail = ConfigRepository.Get("fromEmail");
            string fromName = ConfigRepository.Get("fromName");

            var emailAddressRepository = uow.EmailAddressRepository;
            if (curEvent.Attendees == null)
                curEvent.Attendees = new List<AttendeeDO>();

            InvitationDO curInvitation = new InvitationDO();

            //configure the sender information
            var fromEmailAddr = emailAddressRepository.GetOrCreateEmailAddress(fromEmail);
            fromEmailAddr.Name = fromName;
            curInvitation.From = fromEmailAddr;

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

            curEvent.Emails.Add(curInvitation);

            uow.InvitationRepository.Add(curInvitation);

            return curInvitation;
        }

        private InvitationDO GenerateInitialInvite(InvitationDO curInvitation, EventDO curEvent, string userID)
        {
            curInvitation.Subject = String.Format(ConfigRepository.Get("emailSubject"), GetOriginatorName(curEvent), curEvent.Summary, curEvent.StartDate);
            curInvitation.HTMLText = GetEmailHTMLTextForNew(curEvent, userID);
            curInvitation.PlainText = GetEmailPlainTextForNew(curEvent, userID);
            return curInvitation;
        }

        private InvitationDO GenerateChangeNotification(InvitationDO curInvitation, EventDO curEvent, string userID)
        {
            curInvitation.Subject = String.Format(ConfigRepository.Get("emailSubjectUpdated"), GetOriginatorName(curEvent), curEvent.Summary, curEvent.StartDate);
            curInvitation.HTMLText = GetEmailHTMLTextForUpdate(curEvent, userID);
            curInvitation.PlainText = GetEmailPlainTextForUpdate(curEvent, userID);
            return curInvitation;
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
