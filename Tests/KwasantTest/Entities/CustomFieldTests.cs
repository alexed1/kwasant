using System.Collections.Generic;
using System.Linq;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Infrastructure;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Models
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
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
          
            _trackingStatusRepository = new TrackingStatusRepository(_uow);
            _emailRepo = new EmailRepository(_uow);

            _trackingStatus = new TrackingStatus<EmailDO>(_trackingStatusRepository, _emailRepo);

            _fixture = new FixtureData(_uow);
        }



        [Test]
        public void TestWithoutStatus()
        {
            EmailDO emailOne = new EmailDO() {EmailID = 1, From = _fixture.TestEmailAddress1()};
            EmailDO emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmailAddress1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.UNPROCESSED});
            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWithoutStatus().ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.EmailID, t.First().EmailID);
        }

        [Test]
        public void TestWhereTrackingStatus()
        {
            EmailDO emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmailAddress1() };
            EmailDO emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmailAddress1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.UNPROCESSED });
            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailTwo.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.PROCESSED });
            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWhereTrackingStatus(ts => ts.Status == TrackingStatus.PROCESSED).ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.EmailID, t.First().EmailID);
        }

        [Test]
        public void TestWithStatus()
        {
            EmailDO emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmailAddress1() };
            EmailDO emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmailAddress1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.UNPROCESSED });
            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailTwo.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.PROCESSED });
            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWithStatus().ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(emailOne.EmailID, t.First().EmailID);
            Assert.AreEqual(emailTwo.EmailID, t.Skip(1).First().EmailID);
        }

        [Test]
        public void TestGetStatus()
        {
            EmailDO emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmailAddress1() };
            EmailDO emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmailAddress1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.UNPROCESSED });
            _uow.SaveChanges();

            TrackingStatusDO firstStatus = _trackingStatus.GetStatus(emailOne);
            Assert.NotNull(firstStatus);
            Assert.AreEqual(TrackingStatus.UNPROCESSED, firstStatus.Status);

            TrackingStatusDO secondStatus = _trackingStatus.GetStatus(emailTwo);
            Assert.Null(secondStatus);
        }

        [Test]
        public void TestSetStatus()
        {
            EmailDO emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmailAddress1() };

            _emailRepo.Add(emailOne);
            _uow.SaveChanges();

            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());

            TrackingStatusDO status = _trackingStatus.GetStatus(emailOne);
            Assert.Null(status);
            
            _trackingStatus.SetStatus(emailOne, TrackingStatus.UNPROCESSED);
            _uow.SaveChanges();

            status = _trackingStatus.GetStatus(emailOne);
            Assert.NotNull(status);
            Assert.AreEqual(TrackingStatus.UNPROCESSED, status.Status);
            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.SetStatus(emailOne, TrackingStatus.PROCESSED);
            _uow.SaveChanges();

            status = _trackingStatus.GetStatus(emailOne);
            Assert.NotNull(status);
            Assert.AreEqual(TrackingStatus.PROCESSED, status.Status);
            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());
        }


        [Test]
        public void TestDeleteStatus()
        {
            EmailDO emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmailAddress1() };

            _emailRepo.Add(emailOne);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.UNPROCESSED });
            _uow.SaveChanges();

            Assert.AreEqual(1, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.DeleteStatus(emailOne);
            _uow.SaveChanges();
            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());

            _trackingStatus.DeleteStatus(emailOne);
            _uow.SaveChanges();
            Assert.AreEqual(0, _trackingStatusRepository.GetAll().Count());
        }

        [Test]
        public void TestGetUnprocessedEntities()
        {
            EmailDO emailOne = new EmailDO() { EmailID = 1, From = _fixture.TestEmailAddress1() };
            EmailDO emailTwo = new EmailDO() { EmailID = 2, From = _fixture.TestEmailAddress1() };
            EmailDO emailThree = new EmailDO() { EmailID = 3, From = _fixture.TestEmailAddress1() };

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);
            _emailRepo.Add(emailThree);

            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailOne.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.PROCESSED });
            _trackingStatusRepository.Add(new TrackingStatusDO { ForeignTableID = emailTwo.EmailID, ForeignTableName = "EmailDO", Status = TrackingStatus.UNPROCESSED });

            _uow.SaveChanges();

            List<EmailDO> unprocessed = _trackingStatus.GetUnprocessedEntities().ToList();

            Assert.AreEqual(2, unprocessed.Count);
            Assert.IsNotNull(unprocessed.First());
            Assert.AreEqual(TrackingStatus.UNPROCESSED, _trackingStatus.GetStatus(unprocessed.First()).Status);

            Assert.Null(_trackingStatus.GetStatus(unprocessed.Skip(1).First()));
        }
    }
}
