using Data.Constants;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
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

        public class ShnexyCoreRegistry : Registry
        {
            public ShnexyCoreRegistry()
            {
                For<IAttachment>().Use<AttachmentDO>();
                For<IAttendee>().Use<AttendeeDO>();
                For<IBookingRequest>().Use<BookingRequestDO>();
                For<ICustomer>().Use<CustomerDO>();
                For<IEmail>().Use<EmailDO>();
                For<IEmailAddress>().Use<EmailAddressDO>();
                For<IEvent>().Use<EventDO>();
                For<IUser>().Use<UserDO>();
            }
        }

        public class DevMode : ShnexyCoreRegistry 
        {
            public DevMode()
            {
                For<IUnitOfWork>().Use(new UnitOfWork(new ShnexyDbContext()));

            }
        }

        public class TestMode : ShnexyCoreRegistry
        {
            public TestMode()
            {
                For<IUnitOfWork>().Use(new UnitOfWork(new MockedDBContext()));
            }
        }

        #endregion
    }
}

