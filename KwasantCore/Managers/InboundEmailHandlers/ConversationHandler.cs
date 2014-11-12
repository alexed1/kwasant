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
                    return true;
                }
            }
            return false;
        }
    }
}
