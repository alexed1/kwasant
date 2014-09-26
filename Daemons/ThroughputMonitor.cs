using System;
using System.Linq;
using Data.Interfaces;
using Data.States;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Managers.APIManagers.Packagers;
using StructureMap;
using Utilities;

namespace Daemons
{
    /// <summary>
    /// This Daemon looks for new booking requests, or unprocessed booking requests based on TrackingStatusDO.
    /// New booking requests are sent to the communication manager, which will then send off emails/smses to specific people
    /// </summary>
    public class ThroughputMonitor : Daemon<ThroughputMonitor>
    {
        private readonly IConfigRepository _configRepository;

        public ThroughputMonitor()
            : this(ObjectFactory.GetInstance<IConfigRepository>())
        {
        }

        public ThroughputMonitor(IConfigRepository configRepository)
        {
            if (configRepository == null)
                throw new ArgumentNullException("configRepository");
            _configRepository = configRepository;
        }

        public override int WaitTimeBetweenExecution
        {
            get { return 60*60*1000; } //1 hour
        }

        protected override void Run()
        {
            var startTimeStr = _configRepository.Get<string>("ThroughputCheckingStartTime");
            var endTimeStr = _configRepository.Get<string>("ThroughputCheckingEndTime");

            var startTime = DateTimeOffset.Parse(startTimeStr);
            var endTime = DateTimeOffset.Parse(endTimeStr).AddDays(1);    //We need to add days - since the end time is in the morning (For example 8pm -> 4am).
                                                                    //Not adding the AddDays() would mean we're never in the time frame (after 8pm and before 4am on the same dame).

            var currentTime = DateTimeOffset.Now;

            if (currentTime > startTime && currentTime < endTime)
            {
                //We have to compare with a datetime - EF doesn't support operations like subtracts of datetimes, checking by ticks, etc.
                //The below creates a datetime which represents thirty minutes ago. Anything 'less' than this time is older than 30 minutes.
                var thirtyMinutesAgo = currentTime.Subtract(new TimeSpan(0, 0, 30, 0));
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    //Per Wiki: https://maginot.atlassian.net/wiki/display/SH/Processing+Delay+Alerts
                    //Once every hour, the monitor should query for BookingRequests that have status Unprocessed or CheckOut and are at least 30 minutes old, as measured by comparing the current time to the DateCreated time.
                    var oldBookingRequests =
                        uow.BookingRequestRepository.GetQuery()
                            .Where(
                                br =>
                                    (br.State == BookingRequestState.Unstarted || br.State == BookingRequestState.Booking) &&
                                    br.DateCreated <= thirtyMinutesAgo)
                            .ToList();

                    if (oldBookingRequests.Any())
                    {
                        string toNumber = _configRepository.Get<string>("TwilioToNumber");
                        var tw = ObjectFactory.GetInstance<ISMSPackager>();
                        tw.SendSMS(toNumber, oldBookingRequests.Count() + " Booking requests are over-due by 30 minutes.");
                        LogSuccess(oldBookingRequests.Count() + " Booking requests are over-due by 30 minutes.");
                    }
                }
            }
        }
    }
}
