using Data.Entities;
using Data.Interfaces;
using StructureMap.Configuration.DSL;
using IUser = Data.Interfaces.IUser;

namespace Data.Infrastructure.StructureMap
{
    public class DatabaseStructureMapBootStrapper
    {
        public class KwasantCoreRegistry : Registry
        {
            public KwasantCoreRegistry()
            {
                For<IAttachment>().Use<AttachmentDO>();
                For<IAttendeeDO>().Use<AttendeeDO>();
                //For<IBookingRequestDO>().Use<BookingRequestDO>();
                For<IEmail>().Use<EmailDO>();
                For<IEmailAddressDO>().Use<EmailAddressDO>();
                For<IUser>().Use<UserDO>();
                For<ICalendar>().Use<CalendarDO>();
                For<IAspNetRoles>().Use<AspNetRolesDO>();
                For<IAspNetUserRoles>().Use<AspNetUserRolesDO>();
             
            }
        }

        public class LiveMode : KwasantCoreRegistry 
        {
            public LiveMode()
            {
                //Do not remove _ => (This gives us lazy execution, and a new unit of work & context each call). Removing this will cause the application to be unstable with threads.
                For<IUnitOfWork>().Use(_ => new UnitOfWork(new KwasantDbContext()));
            }
        }

        public class TestMode : KwasantCoreRegistry
        {
            public TestMode()
            {
                For<IUnitOfWork>().Use(new UnitOfWork(new MockedDBContext()));
                For<ISecurityServices>().Use(new MockedSecurityServices());
            }
        }
    }
}