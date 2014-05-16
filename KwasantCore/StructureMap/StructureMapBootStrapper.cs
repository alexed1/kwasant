using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using StructureMap;
using StructureMap.Configuration.DSL;
using IEvent = Data.Interfaces.IEvent;

namespace KwasantCore.StructureMap
{
    public class StructureMapBootStrapper
    {
        #region Method

        public static void ConfigureDependencies(string type)
        {
            switch (type)
            {
                case "test":
                    ObjectFactory.Initialize(x => x.AddRegistry<TestMode>());
                    break;
                default:
                    ObjectFactory.Initialize(x => x.AddRegistry<DevMode>());
                    break;
            }
        }

        public class KwasantCoreRegistry : Registry
        {
            public KwasantCoreRegistry()
            {
                For<MandrillPackager>().Use<MandrillPackager>();
                For<IAttachment>().Use<AttachmentDO>();
                For<IAttendee>().Use<AttendeeDO>();
                For<IBookingRequest>().Use<BookingRequestDO>();
                For<ICustomer>().Use<CustomerDO>();
                For<IEmail>().Use<EmailDO>();
                For<IEmailAddress>().Use<EmailAddressDO>();
                For<IEvent>().Use<EventDO>();
                For<IUser>().Use<UserDO>();
                For<ICalendar>().Use<CalendarDO>();
                For<IPerson>().Use<PersonDO>();
            }
        }

        public class DevMode : KwasantCoreRegistry 
        {
            public DevMode()
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

        #endregion
    }
}

