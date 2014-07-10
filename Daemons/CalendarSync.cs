using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Data.Interfaces;
using KwasantCore.Managers;
using KwasantCore.Managers.APIManager.CalDAV;
using StructureMap;
using Utilities.Logging;

namespace Daemons
{
    class CalendarSync : Daemon
    {
        private readonly Dictionary<int, DateTimeOffset> _synchronizationTime = new Dictionary<int, DateTimeOffset>(); 

        #region Overrides of Daemon

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
                    foreach (var curCalendar in uow.CalendarRepository.GetAll())
                    {
                        DateTimeOffset lastSynchronized, synchronizationTime;
                        if (!_synchronizationTime.TryGetValue(curCalendar.Id, out lastSynchronized))
                            lastSynchronized = DateTimeOffset.MinValue;
                        synchronizationTime = DateTimeOffset.UtcNow;
                        try
                        {
                            SyncManager syncManager = new SyncManager(new CalDAVClientFactory());
                            await syncManager.SyncNowAsync(uow, curCalendar, lastSynchronized);
                            uow.SaveChanges();
                            _synchronizationTime[curCalendar.Id] = synchronizationTime;
                        }
                        catch (Exception ex)
                        {
                            Logger.GetLogger().Error(string.Format("Error occured on synchronization for calendar: {0}.", curCalendar.Id), ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger().Error("Error occured. Shutting down...", ex);
            }
        }

        #endregion
    }
}
