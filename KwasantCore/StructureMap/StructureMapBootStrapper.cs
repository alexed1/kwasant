using Data.Entities;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.Authorizers;
using KwasantCore.Managers.APIManager.Authorizers.Google;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using KwasantCore.Managers.APIManager.Packagers.Twilio;
using KwasantCore.Services;
using StructureMap;
using StructureMap.Configuration.DSL;

namespace KwasantCore.StructureMap
{
    public class StructureMapBootStrapper
    {
        public enum DependencyType
        {
            TEST = 0,
            LIVE = 1
        }

        #region Method

        public static void ConfigureDependencies(DependencyType type)
        {

            switch (type)
            {
                case DependencyType.TEST:
                    ObjectFactory.Initialize(x => x.AddRegistry<TestMode>());
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                    break;
            }
        }

        public class KwasantCoreRegistry : Registry
        {
            public KwasantCoreRegistry()
            {
                For<IEvent>().Use<Event>();
            }
        }

        public class LiveMode : DatabaseStructureMapBootStrapper.LiveMode
        {
            public LiveMode()
            {
                For<ISMSPackager>().Use(new TwilioPackager());
                For<IEmailPackager>().Use(new GmailPackager()).Named(EnvelopeDO.GmailHander);
                For<IEmailPackager>().Use(new MandrillPackager()).Named(EnvelopeDO.MandrillHander);

                For<IOAuthAuthorizer>().Use<GoogleCalendarAuthorizer>().Named("Google");

                For<IKwasantRoleStore>().Use(new KwasantRoleStore());
                For<IKwasantUserStore>().Use(new KwasantUserStore());
            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
                //we need to run tests that "really send it". may want to also do some mocks
                For<IEmailPackager>().Use(new GmailPackager()).Named(EnvelopeDO.GmailHander);
                For<IEmailPackager>().Use(new MandrillPackager()).Named(EnvelopeDO.MandrillHander);

                For<IKwasantRoleStore>().Use(new MockedRoleStore());
                For<IKwasantUserStore>().Use(new MockedUserStore());
            }
        }

        #endregion       
    }
}