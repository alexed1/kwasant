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
                For<IEmail>().Use<EmailDO>();
                For<IEmailAddress>().Use<EmailAddressDO>();
                For<IEvent>().Use<EventDO>();
                For<IPerson>().Use<PersonDO>();
                For<IUser>().Use<UserDO>();
            }
        }

        public class DevMode : KwasantCoreRegistry 
        {
            public DevMode()
            {
                For<IUnitOfWork>().Use(new UnitOfWork(new KwasantDbContext()));

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

