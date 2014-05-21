using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Repositories
{
    [TestFixture]
    public class EventRepositoryTests
    {

        public IUserRepository userRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            userRepo = new UserRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        [Test]
        public void Test()
        {
            
        }
    }
}
