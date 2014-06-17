using System;
using Daemons;
using Data.Entities;
using Data.Interfaces;
using KwasantCore.Managers.APIManager.Packagers;
using KwasantCore.Services;
using KwasantCore.StructureMap;
using KwasantTest.Fixtures;
using Moq;
using NUnit.Framework;
using StructureMap;

namespace KwasantTest.Daemons
{
    [TestFixture]
    public class OutboundEmailTests
    {
        private FixtureData _fixtureData;
        private IUnitOfWork _uow;
        private OutboundEmail _outboundEmailDaemon;

        [SetUp]
        public void Setup()
        {
            StructureMapBootStrapper.ConfigureDependencies(StructureMapBootStrapper.DependencyType.TEST);
            _uow = ObjectFactory.GetInstance<IUnitOfWork>();
            _fixtureData = new FixtureData();
            _outboundEmailDaemon = new OutboundEmail();
        }

        [Test]
        [Category("OutboundEmail")]
        public void CanSendGmailEnvelope()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // SETUP
                var email = _fixtureData.TestEmail1();

                uow.EmailRepository.Add(email);

                // EXECUTE
                var emailService = new Email(_uow, email);
                var envelope = emailService.Send();

                uow.SaveChanges();

                var mockEmailer = new Mock<IEmailPackager>();
                mockEmailer.Setup(a => a.Send(envelope)).Verifiable();
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockEmailer.Object).Named(EnvelopeDO.GmailHander));
                DaemonTests.RunDaemonOnce(_outboundEmailDaemon);

                // VERIFY
                mockEmailer.Verify(a => a.Send(envelope), "OutboundEmail daemon didn't dispatch email via Gmail.");
            }
        }

        [Test]
        [Category("OutboundEmail")]
        public void CanSendMandrillEnvelope()
        {
            using (var uow = ObjectFactory.GetInstance<IUnitOfWork>())
            {
                // SETUP
                var email = _fixtureData.TestEmail1();

                // EXECUTE
                var emailService = new Email(_uow, email);
                var envelope = emailService.SendTemplate("template", email, null);
                uow.SaveChanges();
                
                var mockEmailer = new Mock<IEmailPackager>();
                mockEmailer.Setup(a => a.Send(envelope)).Verifiable();
                ObjectFactory.Configure(
                    a => a.For<IEmailPackager>().Use(mockEmailer.Object).Named(EnvelopeDO.MandrillHander));
                DaemonTests.RunDaemonOnce(_outboundEmailDaemon);

                // VERIFY
                mockEmailer.Verify(a => a.Send(envelope), "OutboundEmail daemon didn't dispatch email via Mandrill.");
            }
        }

        [Test]
        [Category("OutboundEmail")]
        public void FailsToSendInvalidEnvelope()
        {
            // SETUP
            var email = _fixtureData.TestEmail1();

            // EXECUTE
            var emailService = new Email(_uow, email);
            var envelope = emailService.SendTemplate("template", email, null);
            
            envelope.Handler = "INVALID EMAIL PACKAGER";
            _uow.SaveChanges();

            var mockMandrillEmailer = new Mock<IEmailPackager>();
            mockMandrillEmailer.Setup(a => a.Send(envelope)).Throws<ApplicationException>(); // shouldn't be invoked
            ObjectFactory.Configure(a => a.For<IEmailPackager>().Use(mockMandrillEmailer.Object).Named(EnvelopeDO.MandrillHander));
            var mockGmailEmailer = new Mock<IEmailPackager>();
            mockGmailEmailer.Setup(a => a.Send(envelope)).Throws<ApplicationException>(); // shouldn't be invoked
            ObjectFactory.Configure(a => a.For<IEmailPackager>().Use(mockGmailEmailer.Object).Named(EnvelopeDO.GmailHander));
            
            // VERIFY
            Assert.Throws<UnknownEmailPackagerException>(
                () => DaemonTests.RunDaemonOnce(_outboundEmailDaemon),
                "OutboundEmail daemon didn't throw an exception for invalid EnvelopeDO.");
        }
    }
}
