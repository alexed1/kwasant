using Data.Entities;
using Data.Interfaces;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Data.Infrastructure.StructureMap
{
    public class DatabaseStructureMapBootStrapper
    {
        public class KwasantCoreRegistry : Registry
        {
            public KwasantCoreRegistry()
            {
                For<IAttachment>().Use<AttachmentDO>();
                For<IAttendee>().Use<AttendeeDO>();
                For<IBookingRequest>().Use<BookingRequestDO>();
                For<IEmail>().Use<EmailDO>();
                For<IEmailAddress>().Use<EmailAddressDO>();
                For<IEvent>().Use<EventDO>();
                For<IUser>().Use<UserDO>();
                For<ICalendar>().Use<CalendarDO>();
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
            }
        }
    }
}