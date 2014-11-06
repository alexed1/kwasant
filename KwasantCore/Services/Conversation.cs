using System.Linq;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.States;

namespace KwasantCore.Services
{
    public class Conversation
    {
        public static BookingRequestDO Match(IUnitOfWork uow, EmailDO curEmail)
        {
            return (from t in uow.BookingRequestRepository.GetQuery()
                    where ("RE: " + t.Subject == curEmail.Subject || t.Subject == curEmail.Subject)
                    && (t.Recipients.Any(e => e.EmailAddressID == curEmail.From.Id) || t.FromID == curEmail.From.Id)
                    select t).FirstOrDefault();
        }

        public static void AddEmail(IUnitOfWork uow,BookingRequestDO existingBookingRequest, EmailDO curEmail)
        {
            curEmail.ConversationId = existingBookingRequest.Id;
            uow.UserRepository.GetOrCreateUser(curEmail.From);
            existingBookingRequest.State = BookingRequestState.NeedsBooking;
            uow.SaveChanges();

            AlertManager.ConversationMatched(curEmail.Id, curEmail.Subject, existingBookingRequest.Id);
        }
    }
}
