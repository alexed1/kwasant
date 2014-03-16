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
                For<IQueueRepository>().Use<QueueRepository>();
                
                For<IUnitOfWork>().Use(new UnitOfWork(new ShnexyDbContext()));
            }
        }

        public class TestDBControllerRegistry : Registry
        {
            public TestDBControllerRegistry()
            {
                For<IQueueRepository>().Use<QueueRepository>();
                
                
            }
        }

        #endregion
    }
}

