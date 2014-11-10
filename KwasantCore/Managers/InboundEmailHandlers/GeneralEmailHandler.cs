using System.Net.Mail;
using Data.Interfaces;
using KwasantCore.Services;
using StructureMap;

namespace KwasantCore.Managers.InboundEmailHandlers
{
    class GeneralEmailHandler : IInboundEmailHandler
    {
        public bool TryHandle(MailMessage message)
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                BookingRequest.ProcessNewBR(uow, message);
            }
            return true;
        }
    }
}