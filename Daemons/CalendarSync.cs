using System;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManagers.Packagers.CalDAV;
using StructureMap;
using Utilities.Logging;

namespace Daemons
{
    class CalendarSync : Daemon<CalendarSync>
    {
        private readonly ICalDAVClientFactory _calDAVClientFactory;

        public CalendarSync()
            : this(ObjectFactory.GetInstance<ICalDAVClientFactory>())
        {
            
        }

        public CalendarSync(ICalDAVClientFactory calDAVClientFactory)
        {
            if (calDAVClientFactory == null)
                throw new ArgumentNullException("calDAVClientFactory");
            _calDAVClientFactory = calDAVClientFactory;
        }

        public override int WaitTimeBetweenExecution
        {
            get { return (int)TimeSpan.FromHours(1).TotalMilliseconds; }
        }

        protected override async void Run()
        {
            try
            {
                using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
                {
                    foreach (var curUser in uow.UserRepository.GetAll())
                    {
                        try
                        {
                            CalendarSyncManager calendarSyncManager = new CalendarSyncManager(_calDAVClientFactory);
                            await calendarSyncManager.SyncNowAsync(uow, curUser);
                            uow.SaveChanges();
                            Logger.GetLogger().InfoFormat("Calendars synchronized for user: {0}.", curUser.Id);
                        }
                        catch (Exception ex)
                        {
                            Logger.GetLogger().Error(string.Format("Error occured on calendar synchronization for user: {0}.", curUser.Id), ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error("Error occured. Shutting down...", ex);
            }
        }
    }
}
