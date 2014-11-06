using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Utilities.Logging;

namespace KwasantCore.Services
{
    public class Conversation
    {
        public static BookingRequestDO Match(IUnitOfWork uow, String subject, String fromAddress)
        {
            var fromID = uow.EmailAddressRepository.GetOrCreateEmailAddress(fromAddress).Id;
            if (fromID == 0)
                return null;
            
            Func<BookingRequestDO, bool> isValidBookingRequest =
                br => br.State != BookingRequestState.Invalid && //Don't match to invalid booking requests
                        br.State != BookingRequestState.Finished;  //Don't match to old booking requests
                    
            Func<BookingRequestDO, bool> isSimilarSubject = 
                br =>
                ("RE: " + br.Subject == subject || //If the new message is 'RE: [oldSubject]"
                    br.Subject == subject);        //If the new message is '[oldSubject]'

            var currentFromEmailID = fromID;
            Func<BookingRequestDO, bool> isMatchingRecipient =
                br =>
                {
                    //We want to check if they're a participant in either the booking request, OR the booking requests conversation
                    var allEmailsInBookingRequestAndConversation = br.ConversationMembers.Union(new[] { br });
                    return allEmailsInBookingRequestAndConversation.Any(
                        c =>
                            c.Recipients.Any(r => r.EmailAddressID == currentFromEmailID) ||    //Check if they're a recipient
                            c.FromID == currentFromEmailID                                      //Check if they're the sender
                        );
                };

            return uow.BookingRequestRepository.GetQuery().FirstOrDefault(
                br => 
                    isValidBookingRequest(br) && 
                    isSimilarSubject(br) && 
                    isMatchingRecipient(br)
                );
        }

        public static void AddEmail(IUnitOfWork uow,BookingRequestDO existingBookingRequest, EmailDO curEmail)
        {
            curEmail.ConversationId = existingBookingRequest.Id;
            uow.UserRepository.GetOrCreateUser(curEmail.From);
            existingBookingRequest.State = BookingRequestState.NeedsBooking;
            uow.SaveChanges();
            string loggerInfo = "Adding inbound email id = " + curEmail.Id + ", subject = " + existingBookingRequest.Subject + " to existing conversation id = " + existingBookingRequest.Id + ", with BR subject = " + existingBookingRequest.Subject;
            Logger.GetLogger().Info(loggerInfo);
        }
    }
}
