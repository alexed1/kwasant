using Data.Infrastructure.StructureMap;
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
                    DatabaseStructureMapBootStrapper.ConfigureDependencies(DatabaseStructureMapBootStrapper.DependencyType.TEST);
                    break;
                case DependencyType.LIVE:
                    ObjectFactory.Initialize(x => x.AddRegistry<LiveMode>());
                    DatabaseStructureMapBootStrapper.ConfigureDependencies(DatabaseStructureMapBootStrapper.DependencyType.LIVE);
                    break;
            }
        }

        public class KwasantCoreRegistry : Registry
        {
            public KwasantCoreRegistry()
            {
            }
        }

        public class LiveMode : KwasantCoreRegistry
        {
            public LiveMode()
            {
                
            }
        }

        public class TestMode : KwasantCoreRegistry
        {
            public TestMode()
            {
                
            }
        }

        #endregion
    }
}