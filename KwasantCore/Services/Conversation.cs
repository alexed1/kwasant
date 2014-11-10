using System;
using System.Linq;
using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;

namespace KwasantCore.Services
{
    public class Conversation
    {
        public static BookingRequestDO Match(IUnitOfWork uow, String subject, String fromAddress)
        {
            var fromID = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress).Id;
            if (fromID == 0)
                return null;
            
            var currentFromEmailID = fromID;

            return uow.BookingRequestRepository.GetQuery().FirstOrDefault(br =>
                (
                    br.State != BookingRequestState.Invalid &&  //Don't match to invalid booking requests
                    br.State != BookingRequestState.Finished    //Don't match to old booking requests
                ) &&
                (
                    "RE: " + br.Subject == subject ||           //If the new message is 'RE: [oldSubject]"
                    br.Subject == subject                       //If the new message is '[oldSubject]'
                ) &&
                (
                    //We want to check if they're a participant in either the booking request, OR the booking requests conversation
                    br.ConversationMembers.Union(new[] {br}).Any(
                        c =>
                            c.Recipients.Any(r => r.EmailAddressID == currentFromEmailID) ||    //Check if they're a recipient
                            c.FromID == currentFromEmailID                                      //Check if they're the sender
                        )
                    )
                );
        }

        public static void AddEmail(IUnitOfWork uow, MailMessage message, BookingRequestDO existingBookingRequest)
        {
            var curEmail = Email.ConvertMailMessageToEmail(uow.EmailRepository, message);
            Email.FixInlineImages(curEmail);
            curEmail.ConversationId = existingBookingRequest.Id;
            uow.UserRepository.GetOrCreateUser(curEmail.From);
            if (existingBookingRequest.State == BookingRequestState.AwaitingClient ||
                existingBookingRequest.State == BookingRequestState.Resolved)
            {
                existingBookingRequest.State = BookingRequestState.NeedsBooking;
            }

            uow.SaveChanges();
            
            // alerts
            if (existingBookingRequest.State == BookingRequestState.Booking)
            {
                AlertManager.ConversationMemberAdded(existingBookingRequest.Id);
            }

            AlertManager.ConversationMatched(curEmail.Id, curEmail.Subject, existingBookingRequest.Id);
        }
    }
}
