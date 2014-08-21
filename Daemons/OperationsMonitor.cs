using System;
using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using Data.States;
using KwasantCore.Managers;
using KwasantCore.Services;
using StructureMap;
using Utilities;

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
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                //Process Timing Out BR status "CheckedOut" to "Unprocessed"
                int maxBRIdle = Convert.ToInt32(ConfigRepository.Get<string>("MaxBRIdle"));
                DateTimeOffset date = DateTimeOffset.Now;
                DateTimeOffset time = date.AddMinutes(-maxBRIdle);
                List<BookingRequestDO> bookingRequestDOs = new List<BookingRequestDO>();
                bookingRequestDOs = uow.BookingRequestRepository.GetAll().Where(x => x.BookingRequestState == BookingRequestState.CheckedOut && x.LastUpdated.DateTime < time.DateTime && !string.IsNullOrEmpty(x.BookerId)).ToList();
                if (bookingRequestDOs.Count() > 0)
                {
                    BookingRequest _br = new BookingRequest();
                    _br.ProcessTimeout(bookingRequestDOs);
                }
                //---------------

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;

                TrackingStatus<BookingRequestDO> ts = new TrackingStatus<BookingRequestDO>(bookingRequestRepo);
                List<BookingRequestDO> unprocessedBookingRequests = ts.GetUnprocessedEntities(TrackingType.BookingState).ToList();
                if (!unprocessedBookingRequests.Any()) 
                    return;

                CommunicationManager cm = new CommunicationManager();
                cm.ProcessBRNotifications(unprocessedBookingRequests);
                unprocessedBookingRequests.ForEach(br => ts.SetStatus(TrackingType.BookingState, br, TrackingState.Processed));

                uow.SaveChanges();

            }
        }
    }
}
