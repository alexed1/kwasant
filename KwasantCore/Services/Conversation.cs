using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using Utilities.Logging;

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
            string loggerInfo = "Adding inbound email id = " + curEmail.Id + ", subject = " + existingBookingRequest.Subject + " to existing conversation id = " + existingBookingRequest.Id + ", with BR subject = " + existingBookingRequest.Subject;
            Logger.GetLogger().Info(loggerInfo);
        }
    }
}
