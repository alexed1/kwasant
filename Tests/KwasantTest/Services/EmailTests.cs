using System.Collections.Generic;
using Data.Entities;
using Data.Entities.Enumerations;
using Data.Interfaces;
using Data.Repositories;
using FluentValidation;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;
using Utilities;

namespace KwasantTest.Services
{
    [TestFixture]
    public class EmailTests
    {
        private IUnitOfWork _uow;
        private BookingRequestRepository _bookingRequestRepo;
        private FixtureData _fixture;
        private EventDO _curEventDO;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();

            //_bookingRequestRepo = new BookingRequestRepository(_uow);
            _fixture = new FixtureData();
            _curEventDO = new EventDO();
        }

       
        [Test]
        [Category("Email")]
        public void CanConstructEmailWithEmailDO()
        {
            //SETUP  
            EmailDO _curEmailDO = _fixture.TestEmail1();
           
            //EXECUTE
            Email curEmail = new Email(_uow, _curEmailDO);
            curEmail.Send();
            _uow.SaveChanges();

            //VERIFY
            var envelope = _uow.EnvelopeRepository.FindOne(e => e.Email.Id == _curEmailDO.Id);
            Assert.NotNull(envelope, "Envelope was not created.");
            Assert.AreEqual(envelope.Handler, EnvelopeDO.GmailHander, "Envelope handler should be Gmail");
            Assert.AreEqual(EmailStatus.QUEUED, _curEmailDO.EmailStatus);
        }

        [Test]
        [Category("Email")]
        public void CanSendTemplateEmail()
        {
            // SETUP
            EmailDO _curEmailDO = _fixture.TestEmail1();
            const string templateName = "test_template";
            
            // EXECUTE
            var email = new Email(_uow);
            email.SendTemplate(templateName,
                               _curEmailDO,
                               new Dictionary<string, string>() {{"test_key", "test_value"}});

            // VERIFY
            var envelope = _uow.EnvelopeRepository.FindOne(e => e.Email.Id == _curEmailDO.Id);
            Assert.NotNull(envelope, "Envelope was not created.");
            Assert.AreEqual(envelope.TemplateName, templateName);
            Assert.AreEqual(envelope.Handler, EnvelopeDO.MandrillHander, "Envelope handler should be Mandrill");
        }

        [Test]
        [Category("Email")]
        public void FailsToSendInvalidEmail()
        {
            // SETUP
            EmailDO curEmailDO = _fixture.TestEmail1();
            curEmailDO.Subject = "";

            // EXECUTE
            var email = new Email(_uow);

            // VERIFY
            Assert.Throws<ValidationException>(() => { email.Send(curEmailDO); _uow.SaveChanges(); }, "Email should fail to be sent as it is invalid.");
        }

        [Test]
        [Category("Email")]
        public void FailsToSendInvalidTemplatedEmail()
        {
            // SETUP
            EmailDO curEmailDO = _fixture.TestEmail1();
            curEmailDO.Subject = "";

            // EXECUTE
            var email = new Email(_uow);

            // VERIFY
            Assert.Throws<ValidationException>(() => email.SendTemplate("test_template", curEmailDO, null), "Email should fail to be sent as it is invalid.");
        }
    }
}
