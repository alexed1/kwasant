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
    public class OperationsMonitor : Daemon<OperationsMonitor>
    {
        private IConfigRepository _configRepository;

        public OperationsMonitor()
            : this(ObjectFactory.GetInstance<IConfigRepository>())
        {
            
        }

        private OperationsMonitor(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
        }

        public override int WaitTimeBetweenExecution
        {
            get { return 10000; }
        }

        protected override void Run()
        {
            using (IUnitOfWork uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {  
                //Event: A checkedout BR has timed out
                //Action: change BR status "CheckedOut" to "Unprocessed"
                double maxBRIdleMinutes = Convert.ToDouble(_configRepository.Get<string>("MaxBRIdle"));

                DateTimeOffset idleTimeLimit = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(maxBRIdleMinutes));
                List<BookingRequestDO> staleBRList =  uow.BookingRequestRepository.GetAll().Where(x => x.State == BookingRequestState.Booking && x.LastUpdated.DateTime < idleTimeLimit.DateTime).ToList();
                BookingRequest _br = new BookingRequest();
                foreach (var br in staleBRList)
                {
                    _br.Timeout(uow, br);
                    LogSuccess("Booking request timed out");
                }

                //Event: A reserved BR has timed out
                //Action: make BR available to other bookers than preferred one
                double maxBRReservationPeriodMinutes = Convert.ToDouble(_configRepository.Get<string>("MaxBRReservationPeriod"));

                DateTimeOffset reservationTimeLimit = DateTimeOffset.Now.Subtract(TimeSpan.FromMinutes(maxBRReservationPeriodMinutes));
                List<BookingRequestDO> timedOutBRList = uow.BookingRequestRepository.GetAll()
                    .Where(x => x.State == BookingRequestState.NeedsBooking &&
                        x.Availability != BookingRequestAvailability.ReservedPB &&
                        x.BookerID == null &&
                        x.PreferredBookerID != null &&
                        x.LastUpdated.DateTime < reservationTimeLimit.DateTime).ToList();
                foreach (var br in timedOutBRList)
                {
                    _br.ReservationTimeout(uow, br);
                    LogSuccess("Booking request reservation timed out");
                }


                /////////////////////////////////////////////////////
                //Event

                BookingRequestRepository bookingRequestRepo = uow.BookingRequestRepository;
                TrackingStatus<BookingRequestDO> ts = new TrackingStatus<BookingRequestDO>(bookingRequestRepo);
                List<BookingRequestDO> unprocessedBookingRequests = ts.GetUnprocessedEntities(TrackingType.BookingState).ToList();
                if (!unprocessedBookingRequests.Any()) 
                    return;
                CommunicationManager cm = ObjectFactory.GetInstance<CommunicationManager>();
                cm.ProcessBRNotifications(unprocessedBookingRequests);

                //what does this do? alex
                foreach (var unprocessedBR in unprocessedBookingRequests)
                    ts.SetStatus(TrackingType.BookingState, unprocessedBR, TrackingType.TestState);
                
                uow.SaveChanges();
            }
        }
    }
}
