using Data.DataAccessLayer.Infrastructure;
using Data.DataAccessLayer.Interfaces;
using Data.Models;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace Data.DataAccessLayer.StructureMap
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
                For<IEmailStatus>().Use<EmailStatusDO>();
                For<IInvitation>().Use<InvitationDO>();
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
                For<IUnitOfWork>().Use(new UnitOfWork(new ShnexyDbContext()));

            }
        }

        #endregion
    }
}

