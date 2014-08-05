using System;
using System.Linq;
using Data.Entities;
using Data.Interfaces;

namespace Data.Repositories
{
    public class RemoteCalendarLinkRepository : GenericRepository<RemoteCalendarLinkDO>, IRemoteCalendarLinkRepository
    {
        internal RemoteCalendarLinkRepository(IUnitOfWork uow)
            : base(uow)
        {

        }

        public RemoteCalendarLinkDO GetOrCreate(IRemoteCalendarAuthData authData, string remoteName, CalendarDO localCalendarToCreateLinkOn = null)
        {
            if (authData == null)
                throw new ArgumentNullException("authData");
            var link = GetQuery().FirstOrDefault(l =>
                                                 l.Provider.Id == authData.Provider.Id &&
                                                 l.RemoteCalendarName == remoteName &&
                                                 l.LocalCalendar.OwnerID == authData.User.Id);
            if (link == null)
            {
                var localCalendar = localCalendarToCreateLinkOn ??
                                    new CalendarDO()
                                        {
                                            Name = string.Format("{0}:{1}", authData.Provider.Name, remoteName),
                                            Owner = (UserDO) authData.User,
                                            OwnerID = authData.User.Id
                                        }; 

                link = new RemoteCalendarLinkDO
                           {
                               LocalCalendar = localCalendar,
                               Provider = (RemoteCalendarProviderDO) authData.Provider,
                               RemoteCalendarName = remoteName
                           };
                Add(link);
            }
            
            return link;
        }
    }

    public interface IRemoteCalendarLinkRepository : IGenericRepository<RemoteCalendarLinkDO>
    {
    }
}
