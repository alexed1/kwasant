using Daemons;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Managers.APIManager.Packagers.Mandrill;
using KwasantCore.StructureMap;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OutboundEmailTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
        }
    }
}
