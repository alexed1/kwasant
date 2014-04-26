using System.Data.Entity;
using System.Linq;
using Data.Entities;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using NUnit.Framework;
using ShnexyTest.Fixtures;
using StructureMap;

namespace ShnexyTest.Models
{
    [TestFixture]
    class CustomFieldTests
    {
        public TrackingStatus<EmailDO> _trackingStatus;
        private EmailRepository _emailRepo;
        private TrackingStatusRepository _trackingStatusRepository;
        public IUnitOfWork _uow;
        private FixtureData _fixture;

        [SetUp]
        public void Setup()
        {
            Database.SetInitializer(new ShnexyInitializer());
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _uow.Db.Database.Delete();
            _uow.Db.Database.Initialize(true);


            _trackingStatusRepository = new TrackingStatusRepository(_uow);
            _emailRepo = new EmailRepository(_uow);

            _trackingStatus = new TrackingStatus<EmailDO>(_trackingStatusRepository, _emailRepo);

            _fixture = new FixtureData(_uow);
        }



        [Test]
        public void TestWithoutStatus()
        {
            var emailOne = new EmailDO() {EmailID = 1, From = _fixture.TestEmail1()};
            var emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Value = "New Status"});
            _uow.SaveChanges();

            var t = _trackingStatus.GetEntitiesWithoutStatus().ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.EmailID, t.First().EmailID);
        }

        [Test]
        public void TestWhereTrackingStatus()
        {
            var emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmail1() };
            var emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Value = "New Status" });
            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailTwo.EmailID, ForeignTableName = "EmailDO", Value = "Other Status" });
            _uow.SaveChanges();

            var t = _trackingStatus.GetEntitiesWhereTrackingStatus(ts => ts.Value == "Other Status").ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.EmailID, t.First().EmailID);
        }

        [Test]
        public void TestWithStatus()
        {
            var emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmail1() };
            var emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Value = "New Status" });
            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailTwo.EmailID, ForeignTableName = "EmailDO", Value = "Other Status" });
            _uow.SaveChanges();

            var t = _trackingStatus.GetEntitiesWithStatus().ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(emailOne.EmailID, t.First().EmailID);
            Assert.AreEqual(emailTwo.EmailID, t.Skip(1).First().EmailID);
        }

        [Test]
        public void TestGetStatus()
        {
            var emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmail1() };
            var emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Value = "New Status" });
            _uow.SaveChanges();

            var firstStatus = _trackingStatus.GetStatus(emailOne);
            Assert.NotNull(firstStatus);
            Assert.AreEqual("New Status", firstStatus.Value);

            var secondStatus = _trackingStatus.GetStatus(emailTwo);
            Assert.Null(secondStatus);
        }

        [Test]
        public void TestSetStatus()
        {
            var emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);
            _uow.SaveChanges();

            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());

            var status = _trackingStatus.GetStatus(emailOne);
            Assert.Null(status);
            
            _trackingStatus.SetStatus(emailOne, "Test A");
            _uow.SaveChanges();

            status = _trackingStatus.GetStatus(emailOne);
            Assert.NotNull(status);
            Assert.AreEqual("Test A", status.Value);
            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.SetStatus(emailOne, "Test B");
            _uow.SaveChanges();

            status = _trackingStatus.GetStatus(emailOne);
            Assert.NotNull(status);
            Assert.AreEqual("Test B", status.Value);
            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());
        }


        [Test]
        public void TestDeleteStatus()
        {
            var emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmail1() };

            _emailRepo.Add(emailOne);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Value = "New Status" });
            _uow.SaveChanges();

            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.DeleteStatus(emailOne);
            _uow.SaveChanges();
            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.DeleteStatus(emailOne);
            _uow.SaveChanges();
            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());
        }
    }
}
