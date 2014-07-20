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
                    foreach (var curUser in uow.UserRepository.GetAll())
                    {
                        try
                        {
                            SyncManager syncManager = new SyncManager(_calDAVClientFactory);
                            await syncManager.SyncNowAsync(uow, curUser);
                            uow.SaveChanges();
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

        #endregion
    }
}
