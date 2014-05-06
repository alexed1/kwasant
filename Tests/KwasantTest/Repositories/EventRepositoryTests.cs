using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;

namespace KwasantTest.Repositories
{
    [TestFixture]
    public class EventRepositoryTests
    {

        public ICustomerRepository customerRepo;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");

            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            customerRepo = new CustomerRepository(_uow);
            _fixture = new FixtureData(_uow);
        }

        [Test]
        public void Test()
        {
            
        }
    }
}
