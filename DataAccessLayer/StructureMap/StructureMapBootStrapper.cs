using Shnexy.DataAccessLayer.Repositories;
using Shnexy.Models;
using StructureMap;
using StructureMap.Configuration.DSL;

using Shnexy.DataAccessLayer;
using Shnexy.DataAccessLayer.Interfaces;

namespace Shnexy.DataAccessLayer.StructureMap
{
    public class StructureMapBootStrapper
    {
        #region Method

        public static void ConfigureDependencies()
        {
            ObjectFactory.Initialize(x => x.AddRegistry<ControllerRegistry>());
        }

        public static void TestConfigureDependencies()
        {
            ObjectFactory.Initialize(x => x.AddRegistry<TestDBControllerRegistry>());
        }

        public class ControllerRegistry : Registry
        {
            public ControllerRegistry()
            {
                For<IEmail>().Use<Email>();
                For<IEvent>().Use<Event>();
                For<IEmailRepository>().Use<EmailRepository>();
                For<IUnitOfWork>().Use(new UnitOfWork(new ShnexyDbContext()));
            }
        }

        public class TestDBControllerRegistry : Registry //merge this
        {
            public TestDBControllerRegistry()
            {
                For<IEmail>().Use<Email>();
                
                
            }
        }

        #endregion
    }
}

