using System;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers.RemoteCalendar;
using StructureMap;
using Utilities.Logging;

namespace Daemons
{
    class CalendarSync : Daemon<CalendarSync>
    {
        private readonly IRemoteCalendarServiceClientFactory _remoteCalendarServiceClientFactory;

        public CalendarSync()
            : this(ObjectFactory.GetInstance<IRemoteCalendarServiceClientFactory>())
        {
            
        }

        public CalendarSync(IRemoteCalendarServiceClientFactory remoteCalendarServiceClientFactory)
        {
            if (remoteCalendarServiceClientFactory == null)
                throw new ArgumentNullException("remoteCalendarServiceClientFactory");
            _remoteCalendarServiceClientFactory = remoteCalendarServiceClientFactory;
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromHours(1).TotalMilliseconds; }
        }

        protected override void Run()
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    foreach (var curUser in uow.UserRepository.GetAll())
                    {
                        try
                        {
                            CalendarSyncManager calendarSyncManager = new CalendarSyncManager(_remoteCalendarServiceClientFactory);
                            calendarSyncManager.SyncNowAsync(uow, curUser).Wait();
                            uow.SaveChanges();

                            LogSuccess("Calendars synchronized for user: " + curUser.Id);
                        }
                        catch (Exception ex)
                        {
                            LogFail(ex, string.Format("Error occured on calendar synchronization for user: {0}.", curUser.Id));
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFail(ex, "Error occured. Shutting down...");
                throw;
            }
        }
    }
}
