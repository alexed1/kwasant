using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using System.Linq;
using StructureMap;

namespace Daemons.InboundEmailHandlers
{
    class BookingRequestHandler : IInboundEmailHandler
    {
        #region Implementation of IInboundEmailHandler

        public bool Process(MailMessage message)
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                EmailRepository emailRepo = uow.EmailRepository;
                EmailDO email = Email.ConvertMailMessageToEmail(emailRepo, message);

                var curEmail = (from t in uow.EmailRepository.GetAll()
                                where t.Subject == email.Subject
                                && (t.Recipients.Any(e => e.EmailID == email.From.Id) || t.FromID == email.From.Id)
                                select t).FirstOrDefault();

                if (curEmail != null)
                {
                    email.ConversationId = curEmail.Id;
                    var user = new User();
                    UserDO curUser = user.GetOrCreateFromBR(uow, email.From);
                    uow.SaveChanges();
                }
                else
                {
                    BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(uow.BookingRequestRepository, message);
                    (new BookingRequest()).Process(uow, bookingRequest);
                    uow.SaveChanges();
                    AlertManager.BookingRequestCreated(bookingRequest.Id);
                    AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
                }
            }
            return true;
        }

        #endregion
    }
}