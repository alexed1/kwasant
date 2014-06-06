using System;
using System.Linq.Expressions;
using Data.Infrastructure;
using Data.Infrastructure.StructureMap;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.Packagers;
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
                For<IEmailPackager>().Use(new GmailPackager());

                For<IKwasantRoleStore>().Use(new KwasantRoleStore());
                For<IKwasantUserStore>().Use(new KwasantUserStore());
            }
        }

        public class TestMode : DatabaseStructureMapBootStrapper.TestMode
        {
            public TestMode()
            {
                For<IEmailPackager>().Use(new GmailPackager()); //we need to run tests that "really send it". may want to also do some mocks

                For<IKwasantRoleStore>().Use(new MockedRoleStore());
                For<IKwasantUserStore>().Use(new MockedUserStore());
            }
        }

        #endregion       
    }
}