using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace Daemons.InboundEmailHandlers
{
    class ConversationHandler : IInboundEmailHandler
    {
        #region Implementation of IInboundEmailHandler

        public bool Process(MailMessage message)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequestDO existingBookingRequest = Conversation.Match(uow, message.Subject, message.From.Address);
                if (existingBookingRequest != null)
                {
                    Conversation.ProcessReceivedEmail(uow, message, existingBookingRequest);
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
