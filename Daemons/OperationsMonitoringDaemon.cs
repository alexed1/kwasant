using System;
using System.Linq;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.CommunicationManager;
using KwasantCore.Services;
using StructureMap;

namespace Daemons
{
    public class OperationsMonitoringDaemon : Daemon
    {
        public override int WaitTimeBetweenExecution
        {
            get { return 10000; }
        }

        protected override void Run()
        {
            IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>();
            TrackingStatusRepository trackingStatusRepo = new TrackingStatusRepository(uow);
            BookingRequestRepository emailRepo = new BookingRequestRepository(uow);

            TrackingStatus<BookingRequestDO> ts = new TrackingStatus<BookingRequestDO>(trackingStatusRepo, emailRepo);
            var unprocessedBookingRequests = ts.GetUnprocessedEntities().ToList();
            if (!unprocessedBookingRequests.Any()) 
                return;

            var cm = new CommunicationManager();
            cm.ProcessBRNotifications(unprocessedBookingRequests);
            unprocessedBookingRequests.ForEach(br => ts.SetStatus(br, TrackingStatus.PROCESSED));

            uow.SaveChanges();
        }
    }
}
