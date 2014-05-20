using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Entities
{
    [TestFixture]
    internal class CustomFieldTests
    {
        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies("test");
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            _trackingStatusRepository = new TrackingStatusRepository(_uow);
            _emailRepo = new EmailRepository(_uow);


            _trackingStatus = new TestCustomField(_trackingStatusRepository, _emailRepo);

            _fixture = new FixtureData(_uow);
        }

        private TestCustomField _trackingStatus;
        private EmailRepository _emailRepo;
        private TrackingStatusRepository _trackingStatusRepository;
        private IUnitOfWork _uow;
        private FixtureData _fixture;

        private class TestCustomField : GenericCustomField<TrackingStatusDO, EmailDO>
        {
            public TestCustomField(IGenericRepository<TrackingStatusDO> trackingStatusRepo,
                IGenericRepository<EmailDO> foreignRepo) : base(trackingStatusRepo, foreignRepo)
            {
            }

            public IQueryable<EmailDO> GetEntitiesWithoutStatus()
            {
                return GetEntitiesWithoutCustomFields();
            }

            public IQueryable<EmailDO> GetEntitiesWhereTrackingStatus(
                Expression<Func<TrackingStatusDO, bool>> customFieldPredicate)
            {
                return GetEntitiesWithCustomField(customFieldPredicate);
            }

            public IQueryable<EmailDO> GetEntitiesWithStatus()
            {
                return GetEntitiesWithCustomField();
            }

            public TrackingStatusDO GetStatus(EmailDO entityDO)
            {
                return GetCustomField(entityDO);
            }

            public void SetStatus(EmailDO entityDO, TrackingStatus status)
            {
                GetOrCreateCustomField(entityDO).Status = status;
            }

            public void DeleteStatus(EmailDO entityDO)
            {
                DeleteCustomField(entityDO);
            }
        }

        [Test]
        public void TestDeleteStatus()
        {
            var emailOne = new EmailDO {Id = 1};
            emailOne.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());

            _emailRepo.Add(emailOne);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                Id = emailOne.Id,
                ForeignTableName = "EmailDO",
                Status = TrackingStatus.UNPROCESSED
            });
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
        public void TestGetStatus()
        {
            var emailOne = new EmailDO {Id = 1};
            var emailTwo = new EmailDO {Id = 2};
            emailOne.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());
            emailTwo.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                Id = emailOne.Id,
                ForeignTableName = "EmailDO",
                Status = TrackingStatus.UNPROCESSED
            });
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
            var emailOne = new EmailDO {Id = 1};
            emailOne.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());

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
        public void TestWhereTrackingStatus()
        {
            var emailOne = new EmailDO {Id = 1};
            var emailTwo = new EmailDO {Id = 2};
            emailOne.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());
            emailTwo.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                Id = emailOne.Id,
                ForeignTableName = "EmailDO",
                Status = TrackingStatus.UNPROCESSED
            });
            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                Id = emailTwo.Id,
                ForeignTableName = "EmailDO",
                Status = TrackingStatus.PROCESSED
            });
            _uow.SaveChanges();

            List<EmailDO> t =
                _trackingStatus.GetEntitiesWhereTrackingStatus(ts => ts.Status == TrackingStatus.PROCESSED).ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.Id, t.First().Id);
        }

        [Test]
        public void TestWithStatus()
        {
            var emailOne = new EmailDO {Id = 1};
            var emailTwo = new EmailDO {Id = 2};

            emailOne.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());
            emailTwo.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                Id = emailOne.Id,
                ForeignTableName = "EmailDO",
                Status = TrackingStatus.UNPROCESSED
            });
            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                Id = emailTwo.Id,
                ForeignTableName = "EmailDO",
                Status = TrackingStatus.PROCESSED
            });
            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWithStatus().ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(2, t.Count);
            Assert.AreEqual(emailOne.Id, t.First().Id);
            Assert.AreEqual(emailTwo.Id, t.Skip(1).First().Id);
        }

        [Test]
        public void TestWithoutStatus()
        {
            var emailOne = new EmailDO {Id = 1};
            var emailTwo = new EmailDO {Id = 2};
            emailOne.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());
            emailTwo.AddEmailParticipant(EmailParticipantType.FROM, _fixture.TestEmailAddress1());

            _emailRepo.Add(emailOne);
            _emailRepo.Add(emailTwo);

            _trackingStatusRepository.Add(new TrackingStatusDO
            {
                Id = emailOne.Id,
                ForeignTableName = "EmailDO",
                Status = TrackingStatus.UNPROCESSED
            });
            _uow.SaveChanges();

            List<EmailDO> t = _trackingStatus.GetEntitiesWithoutStatus().ToList();
            Assert.AreEqual(2, _emailRepo.GetAll().Count());
            Assert.AreEqual(1, t.Count);
            Assert.AreEqual(emailTwo.Id, t.First().Id);
        }
    }
}