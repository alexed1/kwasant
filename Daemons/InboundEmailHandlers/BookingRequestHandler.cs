using System.Net.Mail;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
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
                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                BookingRequestDO bookingRequest = Email.ConvertMailMessageToEmail(bookingRequestRepo, message);

                //assign the owner of the booking request to be the owner of the From address

                (new BookingRequest()).Process(uow, bookingRequest);

                uow.SaveChanges();

                AlertManager.BookingRequestCreated(bookingRequest.Id);
                AlertManager.EmailReceived(bookingRequest.Id, bookingRequest.User.Id);
            }
            return true;
        }

        #endregion
    }
}