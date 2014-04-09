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
                For<IAttachment>().Use<Attachment>();
                For<IAttendee>().Use<Attendee>();
                For<IBookingRequest>().Use<BookingRequest>();
                For<ICustomer>().Use<Customer>();
                For<IEmail>().Use<Email>();
                For<IEmailAddress>().Use<EmailAddress>();
                For<IEmailStatus>().Use<EmailStatus>();
                For<IInvitation>().Use<Invitation>();
                For<IUser>().Use<User>();
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

