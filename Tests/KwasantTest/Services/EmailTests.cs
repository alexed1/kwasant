using System.Collections.Generic;
using Data.Entities;
using Data.Interfaces;
using Data.States;
using FluentValidation;
using KwasantTest.Fixtures;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Services
{
    [TestFixture]
    public class EmailTests : BaseTest
    {
        [Test]
        [Category("Email")]
        public void CanConstructEmailWithEmailDO()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                //SETUP  
                EmailDO _curEmailDO = fixture.TestEmail1();

                //EXECUTE
                uow.EnvelopeRepository.ConfigurePlainEmail(_curEmailDO);
                uow.SaveChanges();

                //VERIFY
                var envelope = uow.EnvelopeRepository.FindOne(e => e.Email.Id == _curEmailDO.Id);
                Assert.NotNull(envelope, "Envelope was not created.");
                Assert.AreEqual(envelope.Handler, EnvelopeDO.SendGridHander, "Envelope handler should be SendGrid");
                Assert.AreEqual(EmailState.Queued, _curEmailDO.EmailStatus);
            }
        }

        [Test]
        [Category("Email")]
        public void CanSendTemplateEmail()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                // SETUP
                EmailDO _curEmailDO = fixture.TestEmail1();
                const string templateName = "test_template";

                // EXECUTE
                uow.EnvelopeRepository.ConfigureTemplatedEmail(_curEmailDO,
                    templateName,
                    new Dictionary<string, string>()
                    {{"test_key", "test_value"}});

                uow.SaveChanges();

                // VERIFY
                var envelope = uow.EnvelopeRepository.FindOne(e => e.Email.Id == _curEmailDO.Id);
                Assert.NotNull(envelope, "Envelope was not created.");
                Assert.AreEqual(envelope.TemplateName, templateName);
                Assert.AreEqual(envelope.Handler, EnvelopeDO.SendGridHander, "Envelope handler should be Mandrill");
            }
        }

        [Test]
        [Category("Email")]
        public void FailsToSendInvalidEmail()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                // SETUP
                EmailDO curEmailDO = fixture.TestEmail1();
                curEmailDO.Subject = "";

                // EXECUTE

                // VERIFY
                Assert.Throws<ValidationException>(() =>
                {
                    uow.EnvelopeRepository.ConfigurePlainEmail(curEmailDO);
                    uow.SaveChanges();
                }, "Email should fail to be sent as it is invalid.");
            }
        }

        [Test]
        [Category("Email")]
        public void FailsToSendInvalidTemplatedEmail()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                var fixture = new FixtureData(uow);

                // SETUP
                EmailDO curEmailDO = fixture.TestEmail1();
                curEmailDO.Subject = "";

                // EXECUTE

                // VERIFY
                Assert.Throws<ValidationException>(
                    () => uow.EnvelopeRepository.ConfigureTemplatedEmail(curEmailDO, "test_template", null),
                    "Email should fail to be sent as it is invalid.");
            }
        }
    }
}
