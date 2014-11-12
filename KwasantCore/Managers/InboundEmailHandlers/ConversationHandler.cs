using System.Net.Mail;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace KwasantCore.Managers.InboundEmailHandlers
{
    class ConversationHandler : IInboundEmailHandler
    {
        public bool TryHandle(MailMessage message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var existingBookingRequest = Conversation.Match(uow, message.Subject, message.From.Address);
                if (existingBookingRequest != null)
                {
                    Conversation.AddEmail(uow, message, existingBookingRequest);

                    var br = new BookingRequest();
                    var fromUser = uow.UserRepository.GetOrCreateUser(message.From.Address);
                    br.AcknowledgeResponseToBookingRequest(uow, existingBookingRequest.Id, fromUser.Id);

                    uow.SaveChanges();
                    return true;
                }
            }
            return false;
        }
    }
}
