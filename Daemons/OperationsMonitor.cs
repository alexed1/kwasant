using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.CommunicationManager;
using KwasantCore.Services;
using StructureMap;

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
            List<BookingRequestDO> unprocessedBookingRequests = ts.GetUnprocessedEntities(TrackingType.BOOKING_STATE).ToList();
            if (!unprocessedBookingRequests.Any()) 
                return;

            CommunicationManager cm = new CommunicationManager();
            cm.ProcessBRNotifications(unprocessedBookingRequests);
            unprocessedBookingRequests.ForEach(br => ts.SetStatus(TrackingType.BOOKING_STATE, br, TrackingStatus.PROCESSED));

            uow.SaveChanges();
        }
    }
}
