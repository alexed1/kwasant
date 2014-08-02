using System.Collections.Generic;
using System.Linq;
using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers;
using StructureMap;
using TrackingStatus = Data.Constants.TrackingStatus;

namespace Daemons
{
    /// <summary>
    /// This Daemon looks for new booking requests, or unprocessed booking requests based on TrackingStatusDO.
    /// New booking requests are sent to the communication manager, which will then send off emails/smses to specific people
    /// </summary>
    public class OperationsMonitor : Daemon
    {
        public override int WaitTimeBetweenExecution
        {
            get { return 10000; }
        }

        protected override void Run()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

            TrackingStatus<BookingRequestDO> ts = new TrackingStatus<BookingRequestDO>(bookingRequestRepo);
            List<BookingRequestDO> unprocessedBookingRequests = ts.GetUnprocessedEntities(TrackingType.BookingState).ToList();
            if (!unprocessedBookingRequests.Any()) 
                return;

            CommunicationManager cm = new CommunicationManager();
            cm.ProcessBRNotifications(unprocessedBookingRequests);
            unprocessedBookingRequests.ForEach(br => ts.SetStatus(TrackingType.BookingState, br, TrackingStatus.Processed));

            uow.SaveChanges();
        }
    }
}
