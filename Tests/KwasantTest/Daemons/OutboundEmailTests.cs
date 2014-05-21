using KwasantCore.StructureMap;
using NUnit.Framework;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OutboundEmailTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
        }
    }
}
